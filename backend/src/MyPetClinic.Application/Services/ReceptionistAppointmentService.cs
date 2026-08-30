using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyPetClinic.Application.Services
{
    public partial class ReceptionistAppointmentService : IReceptionistAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVaccinationScheduleChecker _vaccinationScheduleChecker;
        private readonly INotificationService _notificationService;
        private readonly IEmailQueue _emailQueue;
        private static readonly System.Threading.SemaphoreSlim _queueSemaphore = new System.Threading.SemaphoreSlim(1, 1);
        private readonly IAppointmentService _appointmentService;

        public ReceptionistAppointmentService(IUnitOfWork unitOfWork, IVaccinationScheduleChecker vaccinationScheduleChecker, INotificationService notificationService, IEmailQueue emailQueue, IAppointmentService appointmentService)
        {
            _unitOfWork = unitOfWork;
            _vaccinationScheduleChecker = vaccinationScheduleChecker;
            _notificationService = notificationService;
            _emailQueue = emailQueue;
            _appointmentService = appointmentService;
        }

        private bool IsTransientConflict(Exception ex)
        {
            var msg = ex.ToString().ToLower();
            return msg.Contains("serialization") || msg.Contains("deadlock") || msg.Contains("conflict") || msg.Contains("transaction failed");
        }

        public async Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, Guid createdBy)
        {
            // Giữ nguyên giờ VN (Unspecified) từ frontend, không ép thành UTC để tránh lệch 7 tiếng
            var appointmentDate = dto.AppointmentDate.HasValue 
                ? DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Unspecified) 
                : DateTime.Now;

            if (appointmentDate.Year < 2000)
            {
                throw new InvalidOperationException("Năm của ngày hẹn không hợp lệ (phải từ năm 2000 trở lên). Vui lòng kiểm tra lại ngày.");
            }

            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var targetDateStart = appointmentDate.Date;
                    var targetDateEnd = targetDateStart.AddDays(1);

                    // 0. Kiểm tra ngày nghỉ lễ và khung giờ hoạt động chung của phòng khám
                    var isHoliday = _unitOfWork.ClinicHolidays.Query()
                        .Any(h => h.IsActive && h.StartDate <= targetDateStart && h.EndDate >= targetDateStart);
                    if (isHoliday)
                    {
                        throw new InvalidOperationException("Phòng khám đóng cửa vào ngày nghỉ lễ này. Vui lòng chọn ngày khác.");
                    }

                    var clinicDay = _unitOfWork.ClinicOperatingDays.Query().FirstOrDefault(d => d.DayOfWeek == targetDateStart.DayOfWeek);
                    if (clinicDay != null && !clinicDay.IsOpen)
                    {
                        throw new InvalidOperationException($"Phòng khám không hoạt động vào {targetDateStart.DayOfWeek}.");
                    }

                    if (clinicDay != null && clinicDay.IsOpen)
                    {
                        var appointmentTimeCheck = appointmentDate.TimeOfDay;
                        var shifts = _unitOfWork.ClinicOperatingShifts.Query().Where(s => s.ClinicOperatingDayId == clinicDay.Id).ToList();
                        if (shifts.Any())
                        {
                            var isInShift = shifts.Any(s => s.StartTime <= appointmentTimeCheck && s.EndTime >= appointmentTimeCheck.Add(TimeSpan.FromMinutes(30)));
                            if (!isInShift)
                            {
                                throw new InvalidOperationException("Thời gian hẹn không nằm trong khung giờ hoạt động của phòng khám.");
                            }
                        }
                    }

                    var finalDoctorId = ResolveAndValidateDoctorId(dto.DoctorId, appointmentDate, dto.ServiceId);

                    var targetDateUtc = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc);
                    var activeStatuses = new[] { "pending", "confirmed", "waiting", "in_progress" };
                    
                    var requestedService = _unitOfWork.Services.Query().FirstOrDefault(s => s.Id == dto.ServiceId);
                    if (requestedService == null) throw new InvalidOperationException("Dịch vụ không tồn tại.");
                    
                    var appointmentTimeCheck2 = appointmentDate.TimeOfDay;
                    var isDuplicatePetService = _unitOfWork.Appointments.Query()
                        .Include(a => a.Service)
                        .Any(a => a.PetId == dto.PetId
                               && activeStatuses.Contains(a.Status.ToLower())
                               && a.AppointmentDate == targetDateUtc
                               && Math.Abs((a.StartTime - appointmentTimeCheck2).TotalMinutes) < 30
                               && a.Service != null
                               && a.Service.CategoryId == requestedService.CategoryId);

                    if (isDuplicatePetService)
                    {
                        throw new InvalidOperationException("Thú cưng đã có lịch hẹn cho cùng nhóm dịch vụ này vào khung giờ này. Vui lòng chọn khung giờ khác!");
                    }

                    // Sinh QR Token (Sử dụng lại QR cũ nếu có lịch hẹn cùng khung giờ)
                    var existingAppointment = _unitOfWork.Appointments.Query()
                        .FirstOrDefault(a => a.CustomerId == dto.CustomerId
                                          && a.Status != "cancelled"
                                          && a.AppointmentDate == targetDateUtc
                                          && a.StartTime == appointmentDate.TimeOfDay);

                    string qrToken = string.Empty;
                    if (existingAppointment != null && !string.IsNullOrEmpty(existingAppointment.QrToken))
                    {
                        qrToken = existingAppointment.QrToken; // Group QR
                    }
                    else
                    {
                        bool isQrUnique = false;
                        for (int q = 0; q < 5 && !isQrUnique; q++)
                        {
                            qrToken = "QR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                            isQrUnique = !_unitOfWork.Appointments.Query().Any(a => a.QrToken == qrToken);
                        }
                        if (!isQrUnique)
                        {
                            throw new InvalidOperationException("Không thể tạo mã QR duy nhất cho lịch hẹn.");
                        }
                    }

                    var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.Id == dto.PetId);
                    if (pet == null)
                    {
                        throw new InvalidOperationException("Không tìm thấy thông tin thú cưng.");
                    }
                    if (pet.CustomerId != dto.CustomerId)
                    {
                        throw new InvalidOperationException("Thú cưng không thuộc sở hữu của khách hàng này.");
                    }

                    var appointment = new Appointment
                    {
                        CustomerId = dto.CustomerId,
                        PetId = dto.PetId,
                        ServiceId = dto.ServiceId,
                        DoctorId = finalDoctorId,
                        Symptom = dto.Symptom?.Trim(),
                        Note = dto.Note?.Trim(),
                        Status = "pending", // Mặc định là chờ xác nhận
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow,
                        AppointmentDate = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc),
                        StartTime = appointmentDate.TimeOfDay,
                        QrToken = qrToken
                    };

                    await _unitOfWork.Appointments.AddAsync(appointment);
                    await _unitOfWork.SaveChangesAsync();

                    // Gửi thông báo đặt lịch thành công cho khách hàng
                    var statusMsg = appointment.Status == "pending_approval" ? "đang chờ được phê duyệt" : "đã được xác nhận";
                    var customerUser = _unitOfWork.Users.Query().FirstOrDefault(u => u.CustomerId == appointment.CustomerId && u.IsActive == true);
                    if (customerUser != null)
                    {
                        await _notificationService.CreateNotificationAsync(
                            customerUser.Id,
                            "Đặt lịch thành công",
                            $"Lịch hẹn của bạn vào lúc {appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy} {statusMsg}.",
                            "System"
                        );
                    }

                    await _unitOfWork.CommitTransactionAsync();
                    return appointment.Id;
                }
                catch (Exception ex) when (IsTransientConflict(ex) && i < retryCount - 1)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    await Task.Delay(new Random().Next(50, 150));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Không thể hoàn tất đăng ký lịch hẹn do tranh chấp dữ liệu kéo dài.");
        }

        public async Task<long> CreateAppointmentWithNewCustomerAsync(AppointmentWithNewCustomerDto dto, Guid createdBy)
        {
            // Giữ nguyên giờ VN (Unspecified) từ frontend, không ép thành UTC để tránh lệch 7 tiếng
            var appointmentDate = dto.AppointmentDate.HasValue 
                ? DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Unspecified) 
                : DateTime.Now;

            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var targetDateStart = appointmentDate.Date;

                    var finalDoctorId = ResolveAndValidateDoctorId(dto.DoctorId, appointmentDate, dto.ServiceId);


                    // 1. Kiểm tra sđt đã tồn tại chưa
                    var customer = _unitOfWork.Customers.Query()
                        .FirstOrDefault(c => c.Phone == dto.CustomerPhone && c.DeletedAt == null);

                    if (customer == null) 
                    {
                        var requestedEmail = !string.IsNullOrWhiteSpace(dto.CustomerEmail) ? dto.CustomerEmail.Trim().ToLower() : null;
                        if (requestedEmail != null)
                        {
                            var isEmailTaken = _unitOfWork.Customers.Query().Any(c => c.Email == requestedEmail);
                            if (isEmailTaken)
                            {
                                throw new InvalidOperationException("Email này đã tồn tại trong hệ thống. Vui lòng sử dụng tính năng tìm kiếm thay vì đăng ký mới.");
                            }
                        }

                        customer = new Customer
                        {
                            Id = Guid.NewGuid(),
                            CustomerCode = "CUS-" + DateTime.UtcNow.ToString("yyyyMMdd") + new Random().Next(100, 999).ToString(),
                            FullName = dto.CustomerName,
                            Phone = dto.CustomerPhone,
                            Email = !string.IsNullOrWhiteSpace(dto.CustomerEmail) ? dto.CustomerEmail.Trim().ToLower() : null,
                            HasAccount = false,
                            Status = "Active",
                            CreatedAt = DateTime.UtcNow
                        };
                        Console.WriteLine($"DEBUG: Creating new Customer with ID={customer.Id}");
                        await _unitOfWork.Customers.AddAsync(customer);
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Found existing Customer with ID={customer.Id}");
                        
                    }

                    // 2. Tạo Pet mới
                    var pet = new Pet
                    {
                        Customer = customer,
                        Name = dto.PetName,
                        Species = dto.Species,
                        Weight = (decimal?)dto.PetWeight,
                        CreatedAt = DateTime.UtcNow
                    };
                    Console.WriteLine($"DEBUG: Creating Pet for CustomerId={customer.Id}");
                    await _unitOfWork.Pets.AddAsync(pet);

                    // Sinh QR Token (Sử dụng lại QR cũ nếu có lịch hẹn cùng khung giờ)
                    var targetDateUtc = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc);
                    var existingAppointment = _unitOfWork.Appointments.Query()
                        .FirstOrDefault(a => a.CustomerId == customer.Id
                                          && a.Status != "cancelled"
                                          && a.AppointmentDate == targetDateUtc
                                          && a.StartTime == appointmentDate.TimeOfDay);

                    string qrToken = string.Empty;
                    if (existingAppointment != null && !string.IsNullOrEmpty(existingAppointment.QrToken))
                    {
                        qrToken = existingAppointment.QrToken; // Group QR
                    }
                    else
                    {
                        bool isQrUnique = false;
                        for (int q = 0; q < 5 && !isQrUnique; q++)
                        {
                            qrToken = "QR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                            isQrUnique = !_unitOfWork.Appointments.Query().Any(a => a.QrToken == qrToken);
                        }
                        if (!isQrUnique)
                        {
                            throw new InvalidOperationException("Không thể tạo mã QR duy nhất cho lịch hẹn.");
                        }
                    }

                    // 3. Tạo Lịch hẹn
                    var appointment = new Appointment
                    {
                        Customer = customer,
                        Pet = pet,
                        ServiceId = dto.ServiceId,
                        DoctorId = finalDoctorId,
                        Symptom = dto.Symptom?.Trim(),
                        Note = dto.Note?.Trim(),
                        Status = "confirmed", // Khách đặt lịch trước luôn gán là confirmed
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow,
                        AppointmentDate = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc),   // Chỉ lưu ngày (với Utc kind)
                        StartTime = appointmentDate.TimeOfDay,    // Lưu giờ riêng
                        QrToken = qrToken
                    };

                    await _unitOfWork.Appointments.AddAsync(appointment);
                    await _unitOfWork.SaveChangesAsync(); // <-- ONE single save

                    // Gửi email vé điện tử nếu có email
                    if (!string.IsNullOrEmpty(customer.Email))
                    {
                        var doctorName = _unitOfWork.Users.Query().FirstOrDefault(u => u.Id == finalDoctorId)?.FullName ?? "Bác sĩ";
                        
                        var emailHtml = MyPetClinic.Application.Utils.EmailTemplateBuilder.BuildAppointmentConfirmedEmail(
                            customerName: customer.FullName ?? "Khách hàng",
                            petName: pet.Name ?? "thú cưng",
                            appointmentDate: appointment.AppointmentDate,
                            doctorName: doctorName,
                            timeSlot: appointment.StartTime.ToString(@"hh\:mm"),
                            qrToken: appointment.QrToken
                        );
                        await _emailQueue.QueueEmailAsync(new MyPetClinic.Application.DTOs.Notification.EmailMessageDto
                        {
                            ToEmail = customer.Email,
                            Subject = "MyPetClinic - Xác nhận đặt lịch khám thành công",
                            BodyHtml = emailHtml
                        });
                    }

                    await _unitOfWork.CommitTransactionAsync();
                    return appointment.Id;
                }
                catch (Exception ex) when (IsTransientConflict(ex) && i < retryCount - 1)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    await Task.Delay(new Random().Next(50, 150));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Không thể hoàn tất đăng ký lịch hẹn do tranh chấp dữ liệu kéo dài.");
        }

        public async Task<System.Collections.Generic.IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId)
        {
            System.Linq.Expressions.Expression<Func<Appointment, bool>> predicate;
            if (doctorId.HasValue)
            {
                predicate = a => a.AppointmentDate >= start && a.AppointmentDate <= end && a.DoctorId == doctorId.Value && a.Status != "pending";
            }
            else
            {
                predicate = a => a.AppointmentDate >= start && a.AppointmentDate <= end && a.Status != "pending";
            }

            var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(
                predicate,
                a => a.Pet!, a => a.Customer!, a => a.Doctor!, a => a.Service!
            );

            var events = new System.Collections.Generic.List<CalendarEventDto>();
            foreach (var a in appointments)
            {
                // Determine color based on status
                var color = "#6c757d"; // default gray
                if (a.Status == "pending") color = "#ffc107"; // yellow
                else if (a.Status == "confirmed") color = "#0dcaf0"; // cyan
                else if (a.Status == "waiting") color = "#0d6efd"; // blue (Kanban queue)
                else if (a.Status == "in_progress") color = "#fd7e14"; // orange
                else if (a.Status == "ready_to_pay" || a.Status == "completed") color = "#198754"; // green
                else if (a.Status == "cancelled") color = "#dc3545"; // red

                var startDateTime = a.AppointmentDate.Date.Add(a.StartTime);
                if (startDateTime.TimeOfDay == TimeSpan.Zero)
                {
                    // If time is 00:00:00 UTC (created from month view without setting time), 
                    // default to 08:00 Local Time (01:00 UTC for VN)
                    startDateTime = startDateTime.AddHours(1);
                }

                events.Add(new CalendarEventDto
                {
                    Id = a.Id.ToString(),
                    Title = $"{a.Pet?.Name} - {a.Customer?.FullName}",
                    Start = startDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    End = startDateTime.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ss"), // Default 30 min block
                    Color = color,
                    AllDay = false,
                    ExtendedProps = new
                    {
                        status = a.Status,
                        petId = a.PetId,
                        petName = a.Pet?.Name,
                        species = a.Pet?.Species,
                        breed = a.Pet?.Breed,
                        weight = a.Pet?.Weight,
                        isAggressive = a.Pet?.IsAggressive ?? false,
                        customerId = a.CustomerId,
                        customerName = a.Customer?.FullName,
                        phone = a.Customer?.Phone,
                        symptom = a.Symptom,
                        note = a.Note,
                        doctorId = a.DoctorId,
                        doctorName = a.Doctor?.FullName,
                        serviceName = a.Service?.Name,
                        qrToken = a.QrToken
                    }
                });
            }

            return events;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(long id, string status, string? reason = null)
        {
            return await _appointmentService.UpdateAppointmentStatusAsync(id, status, reason);
        }

        public async Task<bool> RescheduleAppointmentAsync(long id, DateTime newDate, bool force = false)
        {
            var targetDate = DateTime.SpecifyKind(newDate, DateTimeKind.Utc);

            if (!force && targetDate < DateTime.UtcNow.AddMinutes(-5)) // Trừ hao 5 phút do lệch giờ
            {
                throw new InvalidOperationException("Không thể dời lịch về quá khứ.");
            }

            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                        a => a.Id == id,
                        a => a.Customer!, a => a.Pet!);
                    if (appointment == null) return false;

                    var currentStatus = appointment.Status.ToLower();
                    if (currentStatus == "completed" || currentStatus == "cancelled" || currentStatus == "no_show")
                    {
                        throw new InvalidOperationException("Không thể dời lịch hẹn đã kết thúc hoặc bị hủy.");
                    }

                    // Kiểm tra double booking
                    var targetDateOnly = targetDate.Date;
                    var targetTime = targetDate.TimeOfDay;

                    var sameDayAppointments = await _unitOfWork.Appointments
                        .Query()
                        .Where(a => a.DoctorId == appointment.DoctorId 
                                 && a.Id != id 
                                 && a.Status != "cancelled"
                                 && a.AppointmentDate == targetDateOnly)
                        .Select(a => a.StartTime)
                        .ToListAsync();

                    var isDoubleBooked = sameDayAppointments.Any(startTime => 
                        Math.Abs((startTime - targetTime).TotalMinutes) < 30);

                    if (isDoubleBooked)
                    {
                        throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
                    }

                    var oldDate = appointment.AppointmentDate;
                    appointment.AppointmentDate = targetDateOnly;
                    appointment.StartTime = targetTime;
                    
                    _unitOfWork.Appointments.Update(appointment);
                    await _unitOfWork.SaveChangesAsync();

                    // TH3: Gửi email khi dời lịch
                    try 
                    {
                        var customerUser = _unitOfWork.Users.Query().FirstOrDefault(u => u.CustomerId == appointment.CustomerId && u.IsActive == true);
                        if (customerUser != null && !string.IsNullOrEmpty(customerUser.Email))
                        {
                            var emailHtml = MyPetClinic.Application.Utils.EmailTemplateBuilder.BuildAppointmentRescheduledEmail(
                                customerName: appointment.Customer?.FullName ?? "Khách hàng",
                                petName: appointment.Pet?.Name ?? "thú cưng",
                                oldDate: oldDate,
                                newDate: appointment.AppointmentDate,
                                newTimeSlot: appointment.StartTime.ToString(@"hh\:mm")
                            );
                            await _emailQueue.QueueEmailAsync(new MyPetClinic.Application.DTOs.Notification.EmailMessageDto
                            {
                                ToEmail = customerUser.Email,
                                Subject = "MyPetClinic - Thông báo dời lịch khám",
                                BodyHtml = emailHtml
                            });
                        }
                    } 
                    catch { /* Bỏ qua lỗi gửi email để không rollback data */ }

                    await _unitOfWork.CommitTransactionAsync();
                    return true;
                }
                catch (Exception ex) when (IsTransientConflict(ex) && i < retryCount - 1)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    await Task.Delay(new Random().Next(50, 150));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Không thể hoàn tất dời lịch hẹn do tranh chấp dữ liệu kéo dài.");
        }

        public async Task<bool> UpdateAppointmentDoctorAsync(long id, ChangeDoctorRequestDto request)
        {
            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(
                        a => a.Id == id,
                        a => a.Customer!, a => a.Pet!, a => a.Doctor!
                    );
                    var appointment = appointments.FirstOrDefault();
                    if (appointment == null) return false;

                    var currentStatus = appointment.Status.ToLower();
                    if (currentStatus == "completed" || currentStatus == "cancelled" || currentStatus == "no_show")
                    {
                        throw new InvalidOperationException("Không thể đổi bác sĩ cho lịch hẹn đã kết thúc hoặc bị hủy.");
                    }

                    var newDoctorId = request.NewDoctorId;
                    var force = request.Force;
                    
                    var targetDate = appointment.AppointmentDate.Date;
                    var startTime = appointment.StartTime;
                    var durationMinutes = 30; // Giả sử mặc định 30 phút
                    var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));
                    var targetStartDateTimeOffset = new DateTimeOffset(targetDate.Add(startTime), TimeSpan.Zero); // assuming UTC base
                    var targetEndDateTimeOffset = targetStartDateTimeOffset.AddMinutes(durationMinutes);

                    if (!force)
                    {
                        // Kiểm tra BlockTime
                        var isBlocked = await _unitOfWork.BlockTimes.Query()
                            .AnyAsync(b => b.DoctorId == newDoctorId && 
                                ((targetStartDateTimeOffset >= b.StartTime && targetStartDateTimeOffset < b.EndTime) || 
                                 (targetEndDateTimeOffset > b.StartTime && targetEndDateTimeOffset <= b.EndTime) ||
                                 (targetStartDateTimeOffset <= b.StartTime && targetEndDateTimeOffset >= b.EndTime))
                            );
                        if (isBlocked)
                        {
                            throw new InvalidOperationException("Bác sĩ mới đang trong thời gian nghỉ phép/bận.");
                        }

                        // Kiểm tra double booking cho Bác sĩ mới
                        var allAptsForDoctor = await _unitOfWork.Appointments.Query()
                            .Where(a => a.DoctorId == newDoctorId
                                        && a.Id != id
                                        && a.Status != "cancelled"
                                        && a.AppointmentDate == targetDate)
                            .Select(a => new { a.StartTime, EndTime = a.StartTime.Add(TimeSpan.FromMinutes(30)) })
                            .ToListAsync();

                        var isDoubleBooked = allAptsForDoctor.Any(a => 
                            (startTime >= a.StartTime && startTime < a.EndTime) || 
                            (endTime > a.StartTime && endTime <= a.EndTime) ||
                            (startTime <= a.StartTime && endTime >= a.EndTime)
                        );

                        if (isDoubleBooked)
                        {
                            throw new InvalidOperationException("Bác sĩ mới đang có lịch hẹn bị trùng giờ.");
                        }
                    }

                    var oldDoctor = appointment.Doctor;

                    appointment.DoctorId = newDoctorId;
                    // Lưu lý do nếu có trường Ghi chú đổi bác sĩ, hiện tại chưa có cột ChangeReason trong DB Appointment, ta có thể lưu vào Note hoặc AuditLog.
                    if (!string.IsNullOrWhiteSpace(request.Reason))
                    {
                        appointment.Note = $"[Đổi bác sĩ: {request.Reason}] " + appointment.Note;
                    }

                    _unitOfWork.Appointments.Update(appointment);
                    await _unitOfWork.SaveChangesAsync();

                    // Gửi Notification cho bác sĩ mới
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            newDoctorId,
                            "Lịch khám mới được phân công",
                            $"Bạn vừa được phân công tiếp nhận ca khám mới vào lúc {appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy} cho {appointment.Pet?.Name ?? "thú cưng"}.",
                            "AppointmentAssigned"
                        );

                        // Gửi Notification cho bác sĩ cũ (nếu có)
                        if (oldDoctor != null && oldDoctor.Id != newDoctorId)
                        {
                            await _notificationService.CreateNotificationAsync(
                                oldDoctor.Id,
                                "Lịch khám đã được chuyển",
                                $"Ca khám lúc {appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy} đã được chuyển sang bác sĩ khác.",
                                "AppointmentReassigned"
                            );
                        }
                    }
                    catch { /* Ignore notification errors to not block the main transaction */ }

                    await _unitOfWork.CommitTransactionAsync();
                    return true;
                }
                catch (Exception ex) when (IsTransientConflict(ex) && i < retryCount - 1)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    await Task.Delay(new Random().Next(50, 150));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Không thể hoàn tất đổi bác sĩ do tranh chấp dữ liệu kéo dài.");
        }
        public async Task<IEnumerable<ServiceDto>> GetServicesAsync()
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return services
                .OrderBy(s => s.Name)
                .Select(s => new ServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    CategoryId = s.CategoryId,
                    DurationMinutes = s.DurationMinutes,
                    Description = s.Description,
                    IsActive = s.IsActive
                });
        }

        public async Task<AppointmentStatsDto> GetAppointmentStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var todayAppointmentsList = await _unitOfWork.Appointments.FindAsync(a => a.AppointmentDate.Date == today);
            var todayAppointments = todayAppointmentsList.ToList();

            return new AppointmentStatsDto
            {
                Total = todayAppointments.Count,
                Pending = todayAppointments.Count(a => a.Status == "pending"),
                Confirmed = todayAppointments.Count(a => a.Status == "confirmed"),
                Waiting = todayAppointments.Count(a => a.Status == "waiting"),
                InProgress = todayAppointments.Count(a => a.Status == "in_progress"),
                Completed = todayAppointments.Count(a => a.Status == "completed" || a.Status == "ready_to_pay"),
                Cancelled = todayAppointments.Count(a => a.Status == "cancelled")
            };
        }

        public async Task<IEnumerable<AppointmentDetailDto>> GetPendingAppointmentsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var rawList = _unitOfWork.Appointments.Query()
                .Where(a => a.Status == "pending" && a.AppointmentDate >= today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.PetId,
                    PetName = a.Pet != null ? a.Pet.Name : null,
                    Species = a.Pet != null ? a.Pet.Species : null,
                    Breed = a.Pet != null ? a.Pet.Breed : null,
                    Weight = a.Pet != null ? a.Pet.Weight : null,
                    IsAggressive = a.Pet != null ? (bool?)a.Pet.IsAggressive : null,
                    a.CustomerId,
                    CustomerName = a.Customer != null ? a.Customer.FullName : null,
                    CustomerPhone = a.Customer != null ? a.Customer.Phone : null,
                    a.ServiceId,
                    ServiceName = a.Service != null ? a.Service.Name : null,
                    ServicePrice = a.Service != null ? (decimal?)a.Service.Price : null,
                    a.DoctorId,
                    DoctorName = a.Doctor != null ? a.Doctor.FullName : null,
                    a.AppointmentDate,
                    a.StartTime,
                    a.Symptom,
                    a.Note,
                    a.QrToken,
                    a.Type,
                    a.IsSystemGenerated,
                    a.ReferenceRecordId,
                    a.VaccineId,
                    VaccineName = a.Vaccine != null ? a.Vaccine.Name : null
                })
                .ToList();

            var mapped = rawList.Select(a => new AppointmentDetailDto
            {
                Id = a.Id,
                PetId = a.PetId,
                PetName = a.PetName,
                Species = a.Species,
                Breed = a.Breed,
                Weight = a.Weight,
                IsAggressive = a.IsAggressive ?? false,
                CustomerId = a.CustomerId,
                CustomerName = a.CustomerName,
                CustomerPhone = a.CustomerPhone,
                ServiceId = a.ServiceId,
                ServiceName = a.ServiceName,
                ServicePrice = a.ServicePrice,
                DoctorId = a.DoctorId,
                DoctorName = a.DoctorName,
                AppointmentDate = a.AppointmentDate.ToLocalTime().Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                Symptom = a.Symptom,
                Note = a.Note,
                QrToken = a.QrToken,
                Type = a.Type,
                IsSystemGenerated = a.IsSystemGenerated,
                ReferenceRecordId = a.ReferenceRecordId,
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            });

            return await Task.FromResult(mapped);
        }

        public async Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id)
        {
            var a = _unitOfWork.Appointments.Query().IgnoreQueryFilters()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.PetId,
                    PetName = x.Pet != null ? x.Pet.Name : null,
                    Species = x.Pet != null ? x.Pet.Species : null,
                    Breed = x.Pet != null ? x.Pet.Breed : null,
                    Weight = x.Pet != null ? x.Pet.Weight : null,
                    IsAggressive = x.Pet != null ? (bool?)x.Pet.IsAggressive : null,
                    x.CustomerId,
                    CustomerName = x.Customer != null ? x.Customer.FullName : null,
                    CustomerPhone = x.Customer != null ? x.Customer.Phone : null,
                    x.ServiceId,
                    ServiceName = x.Service != null ? x.Service.Name : null,
                    ServicePrice = x.Service != null ? (decimal?)x.Service.Price : null,
                    x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    x.AppointmentDate,
                    x.StartTime,
                    x.Symptom,
                    x.Note,
                    x.Status,
                    x.QrToken,
                    x.Type,
                    x.IsSystemGenerated,
                    x.ReferenceRecordId,
                    x.VaccineId,
                    VaccineName = x.Vaccine != null ? x.Vaccine.Name : null
                })
                .FirstOrDefault();

            if (a == null) return null;

            var mapped = new AppointmentDetailDto
            {
                Id = a.Id,
                PetId = a.PetId,
                PetName = a.PetName,
                Species = a.Species,
                Breed = a.Breed,
                Weight = a.Weight,
                IsAggressive = a.IsAggressive ?? false,
                CustomerId = a.CustomerId,
                CustomerName = a.CustomerName,
                CustomerPhone = a.CustomerPhone,
                ServiceId = a.ServiceId,
                ServiceName = a.ServiceName,
                ServicePrice = a.ServicePrice,
                DoctorId = a.DoctorId,
                DoctorName = a.DoctorName,
                AppointmentDate = a.AppointmentDate.ToLocalTime().Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                Symptom = a.Symptom,
                Note = a.Note,
                Status = a.Status,
                QrToken = a.QrToken,
                Type = a.Type,
                IsSystemGenerated = a.IsSystemGenerated,
                ReferenceRecordId = a.ReferenceRecordId,
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            };

            return await Task.FromResult(mapped);
        }

        public async Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId)
        {
            var rawList = _unitOfWork.Appointments.Query().IgnoreQueryFilters()
                .Where(a => a.PetId == petId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .ToList();

            var mapped = await MapToDetailDtoAsync(rawList);

            return mapped;
        }

        private async Task<List<AppointmentDetailDto>> MapToDetailDtoAsync(IEnumerable<Appointment> appointments)
        {
            if (!appointments.Any()) return new List<AppointmentDetailDto>();

            var petIds = appointments.Select(a => a.PetId).Distinct().ToList();
            var pets = await _unitOfWork.Pets.Query().IgnoreQueryFilters().Where(p => petIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            var customerIds = appointments.Select(a => a.CustomerId).Distinct().ToList();
            var customers = await _unitOfWork.Customers.Query().IgnoreQueryFilters().Where(c => customerIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);

            var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
            var doctors = await _unitOfWork.Users.Query().IgnoreQueryFilters().Where(u => doctorIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

            var serviceIds = appointments.Select(a => a.ServiceId).Distinct().ToList();
            var services = await _unitOfWork.Services.Query().IgnoreQueryFilters().Where(s => serviceIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

            var vaccineIds = appointments.Where(a => a.VaccineId.HasValue).Select(a => a.VaccineId!.Value).Distinct().ToList();
            var vaccines = await _unitOfWork.Vaccines.Query().IgnoreQueryFilters().Where(v => vaccineIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id);

            var appointmentIds = appointments.Select(a => a.Id).ToList();
            var invoices = await _unitOfWork.Invoices.Query().IgnoreQueryFilters().Where(i => appointmentIds.Contains(i.AppointmentId)).ToDictionaryAsync(i => i.AppointmentId);

            var result = new List<AppointmentDetailDto>();
            foreach (var a in appointments)
            {
                pets.TryGetValue(a.PetId, out var pet);
                customers.TryGetValue(a.CustomerId, out var customer);
                doctors.TryGetValue(a.DoctorId, out var doctor);
                services.TryGetValue(a.ServiceId, out var service);
                Vaccine? vaccine = null;
                if (a.VaccineId.HasValue) vaccines.TryGetValue(a.VaccineId.Value, out vaccine);
                invoices.TryGetValue(a.Id, out var invoice);

                result.Add(new AppointmentDetailDto
                {
                    Id = a.Id,
                    PetId = a.PetId,
                    PetName = pet?.Name ?? "Thú cưng đã xóa",
                    Species = pet?.Species ?? "Không rõ",
                    Breed = pet?.Breed,
                    Weight = pet?.Weight,
                    IsAggressive = pet?.IsAggressive ?? false,
                    CustomerId = a.CustomerId,
                    CustomerName = customer?.FullName ?? "Khách hàng",
                    CustomerPhone = customer?.Phone,
                    ServiceId = a.ServiceId,
                    ServiceName = service?.Name ?? "Dịch vụ đã xóa",
                    ServicePrice = service?.Price,
                    DoctorId = a.DoctorId,
                    DoctorName = doctor?.FullName ?? "Bác sĩ đã nghỉ",
                    AppointmentDate = a.AppointmentDate.ToLocalTime().Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                    Symptom = a.Symptom,
                    Note = a.Note,
                    Status = a.Status,
                    QrToken = a.QrToken,
                    Type = a.Type,
                    IsSystemGenerated = a.IsSystemGenerated,
                    ReferenceRecordId = a.ReferenceRecordId,
                    InvoiceId = invoice?.Id,
                    InvoiceStatus = invoice?.PaymentStatus,
                    InvoiceTotalAmount = invoice?.TotalAmount,
                    VaccineId = a.VaccineId,
                    VaccineName = vaccine?.Name
                });
            }

            return result;
        }

        public async Task<IEnumerable<DoctorAvailableSlotsDto>> GetAvailableSlotsAsync(DateTime date, long? serviceId = null)
        {
            var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            
            // 0. Kiểm tra ngày nghỉ lễ và khung giờ hoạt động chung
            var isHoliday = await _unitOfWork.ClinicHolidays.AnyAsync(h => h.IsActive && h.StartDate <= targetDate && h.EndDate >= targetDate);
            if (isHoliday)
            {
                return new List<DoctorAvailableSlotsDto>(); // Trả về rỗng nếu là ngày lễ
            }

            var clinicDay = await _unitOfWork.ClinicOperatingDays.FindAsync(d => d.DayOfWeek == targetDate.DayOfWeek);
            var clinicDayObj = clinicDay.FirstOrDefault();
            if (clinicDayObj != null && !clinicDayObj.IsOpen)
            {
                return new List<DoctorAvailableSlotsDto>(); // Trả về rỗng nếu đóng cửa
            }

            var clinicShifts = new List<ClinicOperatingShift>();
            if (clinicDayObj != null)
            {
                var shifts = await _unitOfWork.ClinicOperatingShifts.FindAsync(s => s.ClinicOperatingDayId == clinicDayObj.Id);
                clinicShifts = shifts.ToList();
            }

            var targetRole = "";
            int serviceDuration = 30; // Default
            if (serviceId.HasValue)
            {
                var service = await _unitOfWork.Services.FindWithIncludesAsync(s => s.Id == serviceId.Value, s => s.Category!);
                var firstService = service.FirstOrDefault();
                if (firstService != null)
                {
                    if (firstService.DurationMinutes.HasValue && firstService.DurationMinutes.Value > 0)
                    {
                        serviceDuration = firstService.DurationMinutes.Value;
                    }

                    if (firstService.Category != null)
                    {
                        if (firstService.Category.Name.Equals("Khám bệnh", StringComparison.OrdinalIgnoreCase))
                        {
                            targetRole = "clinical_doctor";
                        }
                        else if (firstService.Category.Name.Equals("Tiêm phòng", StringComparison.OrdinalIgnoreCase))
                        {
                            targetRole = "vaccination_doctor";
                        }
                    }
                }
            }

            // 1. Lấy tất cả ca trực của bác sĩ còn hoạt động vào ngày chỉ định
            var schedulesList = (await _unitOfWork.DoctorSchedules.FindWithIncludesAsync(
                s => s.WorkDate == targetDate && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true
                     && (string.IsNullOrEmpty(targetRole) || (s.Doctor.Role != null && s.Doctor.Role.Name.ToLower() == targetRole)),
                s => s.Doctor!
            )).ToList();

            var result = new List<DoctorAvailableSlotsDto>();

            if (schedulesList.Any())
            {
                // Group schedules by Doctor to handle multiple shifts per day
                var groupedSchedules = schedulesList.GroupBy(s => s.DoctorId).ToList();

                foreach (var group in groupedSchedules)
                {
                    var doctorId = group.Key;
                    var doctorName = group.First().Doctor?.FullName ?? "Bác sĩ thú y";
                    var nextDay = targetDate.AddDays(1);
                    
                    var appointments = await _unitOfWork.Appointments.FindAsync(
                        a => a.DoctorId == doctorId 
                          && a.AppointmentDate >= targetDate 
                          && a.AppointmentDate < nextDay
                          && a.Status != "cancelled"
                    );

                    var blockTimes = await _unitOfWork.BlockTimes.FindAsync(
                        b => b.DoctorId == doctorId
                          && b.StartTime < new DateTimeOffset(nextDay)
                          && b.EndTime > new DateTimeOffset(targetDate)
                    );

                    var allAvailableTimesForDoctor = new List<DateTime>();

                    foreach (var schedule in group)
                    {
                        var availableTimes = MyPetClinic.Application.Helpers.SlotCalculationHelper.GetAvailableSlots(schedule, appointments, blockTimes, serviceDuration);

                        // Filter by ClinicOperatingShifts if available
                        if (clinicShifts.Any())
                        {
                            availableTimes = availableTimes.Where(t => 
                                clinicShifts.Any(s => s.StartTime <= t.TimeOfDay && s.EndTime >= t.TimeOfDay.Add(TimeSpan.FromMinutes(serviceDuration)))
                            ).ToList();
                        }
                        
                        allAvailableTimesForDoctor.AddRange(availableTimes);
                    }

                    // Remove any accidental duplicates across shifts and sort chronologically
                    allAvailableTimesForDoctor = allAvailableTimesForDoctor.Distinct().OrderBy(t => t).ToList();

                    if (allAvailableTimesForDoctor.Any())
                    {
                        result.Add(new DoctorAvailableSlotsDto
                        {
                            DoctorId = doctorId,
                            DoctorName = doctorName,
                            AvailableSlots = allAvailableTimesForDoctor.Select(t => t.ToString("HH:mm")).ToList()
                        });
                    }
                }
            }

            return result;
        }

        public async Task<List<AppointmentDetailDto>> CheckInAsync(CheckInBulkRequestDto request)
        {
            var results = new List<AppointmentDetailDto>();
            var appointmentIds = request.Items?.Select(x => x.AppointmentId).ToList() ?? new List<long>();
            IEnumerable<Appointment> appointments = new List<Appointment>();

            if (appointmentIds.Any())
            {
                appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(
                    a => appointmentIds.Contains(a.Id),
                    a => a.Customer!, a => a.Pet!, a => a.Doctor!, a => a.Service!, a => a.Vaccine!, a => a.Invoice!
                );

                if (appointments.Count() != appointmentIds.Count)
                {
                    throw new InvalidOperationException("Không tìm thấy một số ca khám trong danh sách yêu cầu.");
                }
            }
            else if (!string.IsNullOrEmpty(request.QrToken))
            {
                appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(
                    a => a.QrToken == request.QrToken,
                    a => a.Customer!, a => a.Pet!, a => a.Doctor!, a => a.Service!, a => a.Vaccine!, a => a.Invoice!
                );
            }

            if (!appointments.Any())
            {
                throw new InvalidOperationException("Danh sách Check-in trống hoặc không tìm thấy ca khám.");
            }

            await _queueSemaphore.WaitAsync();
            try
            {
                var today = DateTime.UtcNow.Date;
                var todayAppointments = await _unitOfWork.Appointments.FindAsync(x => x.AppointmentDate.Date == today && x.QueueNumber > 0);
                var lastQueue = todayAppointments.Any() ? todayAppointments.Max(x => (int?)x.QueueNumber) ?? 0 : 0;

                foreach (var appointment in appointments)
                {
                    if (appointment.Status.ToLower() != "confirmed")
                    {
                        throw new InvalidOperationException($"Lịch hẹn của bé {appointment.Pet?.Name ?? "Thú cưng"} đang ở trạng thái '{appointment.Status}', không thể check-in.");
                    }

                    var requestItem = request.Items?.FirstOrDefault(x => x.AppointmentId == appointment.Id);

                    if (requestItem?.CurrentWeight.HasValue == true)
                    {
                        var pet = appointment.Pet;
                        if (pet != null)
                        {
                            pet.Weight = requestItem.CurrentWeight.Value;
                            _unitOfWork.Pets.Update(pet);
                        }
                    }

                    if (appointment.QueueNumber == 0)
                    {
                        lastQueue++;
                        appointment.QueueNumber = lastQueue;
                    }

                    appointment.Status = "waiting";
                    appointment.CheckInTime = DateTime.UtcNow;

                    _unitOfWork.Appointments.Update(appointment);
                }

                await _unitOfWork.SaveChangesAsync();
                
                foreach (var a in appointments)
                {
                    var detail = await GetAppointmentDetailAsync(a.Id);
                    if (detail != null) results.Add(detail);
                }
            }
            finally
            {
                _queueSemaphore.Release();
            }

            return results;
        }

        public async Task<IEnumerable<EligibleDoctorDto>> GetSuitableDoctorsForAppointmentAsync(long appointmentId)
        {
            var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => a.Id == appointmentId,
                a => a.Service!, a => a.Customer!, a => a.Pet!
            );
            var appointment = appointments.FirstOrDefault();
            if (appointment == null) throw new InvalidOperationException("Không tìm thấy ca khám.");

            var service = appointment.Service;
            bool isVaccine = false;
            
            if (service != null && !string.IsNullOrEmpty(service.Name))
            {
                var lowerName = service.Name.ToLower();
                if (lowerName.Contains("tiêm") || lowerName.Contains("vaccin") || lowerName.Contains("vắc xin"))
                {
                    isVaccine = true;
                }
            }

            // 1. HARD CONSTRAINTS: Kiểm tra Role
            var targetRole = isVaccine ? "vaccination_doctor" : "clinical_doctor";

            var allDoctors = await _unitOfWork.Users.FindWithIncludesAsync(
                u => u.IsActive == true && u.DeletedAt == null && u.Role != null && 
                     (u.Role.Name.ToLower() == targetRole || u.Role.Name.ToLower() == "clinical_doctor" || u.Role.Name.ToLower().Contains("doctor")),
                u => u.Role!, u => u.EmployeeProfile!
            );

            // Filter Role ưu tiên
            var targetDoctors = allDoctors.Where(u => u.Role!.Name.ToLower() == targetRole).ToList();
            if (!targetDoctors.Any())
            {
                targetDoctors = allDoctors.ToList();
            }

            var eligibleDoctors = new List<EligibleDoctorDto>();
            var targetDate = appointment.AppointmentDate.Date;
            var startTime = appointment.StartTime;
            var durationMinutes = 30; // Giả sử mặc định 30 phút, có thể lấy từ Service.DurationMinutes nếu có
            var targetStartDateTimeOffset = new DateTimeOffset(targetDate.Add(startTime), TimeSpan.Zero);
            var targetEndDateTimeOffset = targetStartDateTimeOffset.AddMinutes(durationMinutes);
            var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));

            // Lấy tất cả BlockTime trong ngày
            var startOfDayOffset = new DateTimeOffset(targetDate, TimeSpan.Zero);
            var endOfDayOffset = startOfDayOffset.AddDays(1);
            var allBlockTimes = await _unitOfWork.BlockTimes.Query()
                .Where(b => b.StartTime < endOfDayOffset && b.EndTime > startOfDayOffset)
                .ToListAsync();

            // Lấy tất cả lịch hẹn trong ngày để check trùng
            var allAppointmentsToday = await _unitOfWork.Appointments.Query()
                .Where(a => a.AppointmentDate == targetDate && a.Status != "cancelled" && a.Id != appointmentId)
                .Select(a => new { a.DoctorId, a.StartTime, EndTime = a.StartTime.Add(TimeSpan.FromMinutes(30)) }) // Giả định 30 phút
                .ToListAsync();

            // Lấy tất cả Review để tính điểm
            var allReviewsData = await _unitOfWork.Reviews.Query()
                .Include(r => r.Appointment)
                .Where(r => r.DeletedAt == null && r.Appointment != null)
                .Select(r => new { r.Appointment!.DoctorId, r.Rating })
                .ToListAsync();
            var allReviews = allReviewsData.GroupBy(r => r.DoctorId).Select(g => new { DoctorId = g.Key, AvgRating = g.Average(r => (double)r.Rating) }).ToList();

            // Lịch sử khám để tính điểm W1
            var customerPastApts = await _unitOfWork.Appointments.Query()
                .Where(a => a.CustomerId == appointment.CustomerId && a.Status == "completed")
                .Select(a => new { a.DoctorId, a.PetId })
                .ToListAsync();

            foreach (var doc in targetDoctors)
            {
                // 2. HARD CONSTRAINTS: DoctorSchedule & BlockTime
                // (Trong dự án hiện tại, bác sĩ có thể không có DoctorSchedule cụ thể từng ngày mà dùng OperatingHours chung. 
                // Ở đây ta tập trung check BlockTime và Overlap)

                var docBlockTimes = allBlockTimes.Where(b => b.DoctorId == doc.Id).ToList();
                bool isBlocked = docBlockTimes.Any(b => 
                    (targetStartDateTimeOffset >= b.StartTime && targetStartDateTimeOffset < b.EndTime) || 
                    (targetEndDateTimeOffset > b.StartTime && targetEndDateTimeOffset <= b.EndTime) ||
                    (targetStartDateTimeOffset <= b.StartTime && targetEndDateTimeOffset >= b.EndTime)
                );

                if (isBlocked) continue; // Bỏ qua bác sĩ đang nghỉ phép/bận

                // 3. HARD CONSTRAINTS: Overlap
                var docAppointments = allAppointmentsToday.Where(a => a.DoctorId == doc.Id).ToList();
                bool isOverlapped = docAppointments.Any(a => 
                    (startTime >= a.StartTime && startTime < a.EndTime) || 
                    (endTime > a.StartTime && endTime <= a.EndTime) ||
                    (startTime <= a.StartTime && endTime >= a.EndTime)
                );

                if (isOverlapped) continue; // Bỏ qua bác sĩ bị trùng lịch

                // ----- SCORING ENGINE -----
                int scoreW1 = 0;
                int scoreW2 = 0;
                int scoreW3 = 0;
                var tags = new List<string>();

                // W1: Lịch sử gắn kết (Tối đa +50đ)
                if (customerPastApts.Any(a => a.DoctorId == doc.Id && a.PetId == appointment.PetId))
                {
                    scoreW1 = 50;
                    tags.Add("Đã khám bé");
                }
                else if (customerPastApts.Any(a => a.DoctorId == doc.Id))
                {
                    scoreW1 = 25;
                    tags.Add("Quen thuộc");
                }

                // W2: Chất lượng đánh giá (Tối đa +30đ)
                var reviewOpt = allReviews.FirstOrDefault(r => r.DoctorId == doc.Id);
                double avgRating = reviewOpt != null ? reviewOpt.AvgRating : 5.0; // Mặc định 5.0 nếu chưa có đánh giá
                scoreW2 = (int)Math.Round(30 * (avgRating / 5.0));
                if (avgRating > 0) tags.Add($"{avgRating:F1} sao");

                // W3: Thâm niên (Tối đa +20đ)
                if (doc.EmployeeProfile != null && doc.EmployeeProfile.CreatedAt.HasValue)
                {
                    var months = (DateTime.UtcNow.Year - doc.EmployeeProfile.CreatedAt.Value.Year) * 12 + DateTime.UtcNow.Month - doc.EmployeeProfile.CreatedAt.Value.Month;
                    scoreW3 = Math.Min(20, months); // Mỗi tháng 1 điểm, tối đa 20 điểm
                    if (months >= 12) tags.Add("Kinh nghiệm");
                }

                int totalScore = scoreW1 + scoreW2 + scoreW3;

                eligibleDoctors.Add(new EligibleDoctorDto
                {
                    DoctorId = doc.Id,
                    FullName = doc.FullName ?? string.Empty,
                    Role = doc.Role?.Name ?? string.Empty,
                    AverageRating = Math.Round(avgRating, 1),
                    ExperienceMonths = (doc.EmployeeProfile != null && doc.EmployeeProfile.CreatedAt.HasValue) ? 
                        ((DateTime.UtcNow.Year - doc.EmployeeProfile.CreatedAt.Value.Year) * 12 + DateTime.UtcNow.Month - doc.EmployeeProfile.CreatedAt.Value.Month) : 0,
                    TotalScore = totalScore,
                    IsRecommended = totalScore >= 75,
                    Tags = tags
                });
            }

            // Sắp xếp theo điểm giảm dần
            return eligibleDoctors.OrderByDescending(d => d.TotalScore).ToList();
        }

        private Guid ResolveAndValidateDoctorId(Guid requestedDoctorId, DateTime appointmentDate, long serviceId)
        {
            var finalDoctorId = requestedDoctorId;
            var targetDateStart = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc);
            var targetDateEnd = targetDateStart.AddDays(1);

            var targetRole = "";
            var serviceEntity = _unitOfWork.Services.Query()
                .Where(s => s.Id == serviceId)
                .Select(s => new { CategoryName = s.Category != null ? s.Category.Name : null })
                .FirstOrDefault();

            if (serviceEntity != null && serviceEntity.CategoryName != null)
            {
                if (serviceEntity.CategoryName.Equals("Khám bệnh", StringComparison.OrdinalIgnoreCase))
                {
                    targetRole = "clinical_doctor";
                }
                else if (serviceEntity.CategoryName.Equals("Tiêm phòng", StringComparison.OrdinalIgnoreCase))
                {
                    targetRole = "vaccination_doctor";
                }
            }

            if (finalDoctorId == Guid.Empty)
            {
                var appointmentTime = appointmentDate.TimeOfDay;

                // 1. Lấy tất cả bác sĩ có lịch trực vào ngày hẹn mà thời gian hẹn nằm trong ca trực của họ
                var doctorsWithSchedules = _unitOfWork.DoctorSchedules.Query()
                    .Where(s => s.WorkDate == targetDateStart && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true
                                && (string.IsNullOrEmpty(targetRole) || (s.Doctor.Role != null && s.Doctor.Role.Name.ToLower() == targetRole)))
                    .ToList();

                List<Guid> doctorsList = new List<Guid>();
                if (doctorsWithSchedules.Any())
                {
                    // Lọc bác sĩ nằm trong khung giờ ca trực
                    doctorsList = doctorsWithSchedules
                        .Where(s => appointmentTime >= s.StartTime && appointmentTime + TimeSpan.FromMinutes(30) <= s.EndTime)
                        .Select(s => s.DoctorId)
                        .Distinct()
                        .ToList();
                    
                    // Đã loại bỏ fallback. Nếu không có ai trong khung giờ thì sẽ văng exception ở dưới.
                }

                if (!doctorsList.Any())
                {
                    throw new InvalidOperationException("Hệ thống hiện không có bác sĩ nào được phân lịch trực vào khung giờ này cho dịch vụ bạn chọn!");
                }

                // 2. Lọc ra danh sách các bác sĩ THỰC SỰ RẢNH (không trùng lịch trong khoảng +/- 30 phút)
                var allAptsForDoctors = _unitOfWork.Appointments.Query()
                    .Where(a => a.Status != "cancelled"
                                && a.AppointmentDate == targetDateStart
                                && doctorsList.Contains(a.DoctorId))
                    .Select(a => new { a.DoctorId, a.AppointmentDate, a.StartTime })
                    .ToList();

                var busyDoctorIds = allAptsForDoctors
                    .Where(a => {
                        var apptStartTime = a.StartTime;
                        var diff = (apptStartTime - appointmentDate.TimeOfDay).TotalMinutes;
                        return Math.Abs(diff) < 30;
                    })
                    .Select(a => a.DoctorId)
                    .Distinct()
                    .ToList();

                var blockTimesForDoctors = _unitOfWork.BlockTimes.Query()
                    .Where(b => doctorsList.Contains(b.DoctorId) 
                             && b.StartTime < new DateTimeOffset(targetDateEnd)
                             && b.EndTime > new DateTimeOffset(targetDateStart))
                    .ToList();

                var blockedDoctorIds = blockTimesForDoctors
                    .Where(b => {
                        var blockStart = b.StartTime.LocalDateTime;
                        var blockEnd = b.EndTime.LocalDateTime;
                        var apptEnd = appointmentDate.AddMinutes(30);
                        return appointmentDate < blockEnd && apptEnd > blockStart;
                    })
                    .Select(b => b.DoctorId)
                    .Distinct()
                    .ToList();

                var availableDoctors = doctorsList.Except(busyDoctorIds).Except(blockedDoctorIds).ToList();

                if (!availableDoctors.Any())
                {
                    throw new InvalidOperationException("Tất cả bác sĩ trực khung giờ này đã kín lịch. Vui lòng chọn khung giờ khác.");
                }

                // 3. Chọn bác sĩ có ít lịch hẹn nhất trong ngày hôm đó từ danh sách bác sĩ rảnh
                var doctorApptCounts = _unitOfWork.Appointments.Query()
                    .Where(a => a.AppointmentDate >= targetDateStart && a.AppointmentDate < targetDateEnd && a.Status != "cancelled" && availableDoctors.Contains(a.DoctorId))
                    .GroupBy(a => a.DoctorId)
                    .Select(g => new { DoctorId = g.Key, Count = g.Count() })
                    .ToList();

                var doctorWithCount = availableDoctors
                    .Select(id => new { DoctorId = id, Count = doctorApptCounts.FirstOrDefault(c => c.DoctorId == id)?.Count ?? 0 })
                    .OrderBy(x => x.Count)
                    .First();

                finalDoctorId = doctorWithCount.DoctorId;
            }
            else
            {
                // Validate doctor role first
                var doctorInfo = _unitOfWork.Users.Query()
                    .Where(u => u.Id == finalDoctorId)
                    .Select(u => new { RoleName = u.Role != null ? u.Role.Name : null })
                    .FirstOrDefault();

                if (doctorInfo != null && !string.IsNullOrEmpty(targetRole) && 
                    !string.Equals(doctorInfo.RoleName, targetRole, StringComparison.OrdinalIgnoreCase))
                {
                    var friendlyTargetRole = targetRole == "clinical_doctor" ? "Khám bệnh" : "Tiêm phòng";
                    throw new InvalidOperationException($"Bác sĩ được chọn không thuộc chuyên khoa {friendlyTargetRole}.");
                }

                // Kiểm tra xem bác sĩ được chọn có ca trực trong ngày hẹn không
                var schedules = _unitOfWork.DoctorSchedules.Query()
                    .Where(s => s.DoctorId == finalDoctorId && s.WorkDate == targetDateStart && s.IsAvailable)
                    .ToList();
                
                if (!schedules.Any())
                {
                    throw new InvalidOperationException("Bác sĩ không có lịch trực trong ngày này.");
                }

                var appointmentTime = appointmentDate.TimeOfDay;
                var fitsAnyShift = schedules.Any(s => appointmentTime >= s.StartTime && appointmentTime + TimeSpan.FromMinutes(30) <= s.EndTime);
                
                if (!fitsAnyShift)
                {
                    var shiftsText = string.Join(", ", schedules.Select(s => $"{s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm}"));
                    throw new InvalidOperationException($"Thời gian hẹn phải nằm trong ca trực của bác sĩ ({shiftsText}).");
                }
            }

            // Chặn đặt lịch nếu Bác sĩ đã có lịch trong khoảng +/- 30 phút (Double-Booking Check)
            var sameDay_Apts = _unitOfWork.Appointments.Query()
                .Where(a => a.DoctorId == finalDoctorId
                            && a.Status != "cancelled"
                            && a.AppointmentDate == targetDateStart)
                .Select(a => a.StartTime)
                .ToList();

            var isDoubleBooked = sameDay_Apts.Any(startTime => 
                Math.Abs((startTime - appointmentDate.TimeOfDay).TotalMinutes) < 30);

            if (isDoubleBooked)
            {
                throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
            }

            // Chặn đặt lịch nếu Bác sĩ đang có Lịch Nghỉ/Bận (BlockTime)
            var doctorBlockTimes = _unitOfWork.BlockTimes.Query()
                .Where(b => b.DoctorId == finalDoctorId
                         && b.StartTime < new DateTimeOffset(targetDateEnd)
                         && b.EndTime > new DateTimeOffset(targetDateStart))
                .ToList();

            var isBlockedByLeave = doctorBlockTimes.Any(b => {
                var blockStart = b.StartTime.LocalDateTime.TimeOfDay;
                var blockEnd = b.EndTime.LocalDateTime.TimeOfDay;
                var apptStart = appointmentDate.TimeOfDay;
                var apptEnd = apptStart + TimeSpan.FromMinutes(30);
                return apptStart < blockEnd && apptEnd > blockStart;
            });

            if (isBlockedByLeave)
            {
                throw new InvalidOperationException("Bác sĩ đang có lịch nghỉ/bận trong khoảng thời gian này.");
            }

            return finalDoctorId;
        }
    }
}

