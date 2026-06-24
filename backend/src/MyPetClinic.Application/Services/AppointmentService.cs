using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVaccinationScheduleChecker _vaccinationScheduleChecker;
        private readonly INotificationService _notificationService;

        public AppointmentService(IUnitOfWork unitOfWork, IVaccinationScheduleChecker vaccinationScheduleChecker, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _vaccinationScheduleChecker = vaccinationScheduleChecker;
            _notificationService = notificationService;
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
                    var finalDoctorId = dto.DoctorId;
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


                    if (finalDoctorId == Guid.Empty)
                    {
                        var appointmentTime = appointmentDate.TimeOfDay;
                        var allowedDoctorEmails = new List<string>();
                        var serviceEntity = _unitOfWork.Services.Query()
                            .Where(s => s.Id == dto.ServiceId)
                            .Select(s => new { CategoryName = s.Category != null ? s.Category.Name : null })
                            .FirstOrDefault();

                        if (serviceEntity != null && serviceEntity.CategoryName != null)
                        {
                            if (serviceEntity.CategoryName.Equals("Khám bệnh", StringComparison.OrdinalIgnoreCase))
                            {
                                allowedDoctorEmails.AddRange(new[] { "bacsi_test@gmail.com", "bacsituantran@gmail.com" });
                            }
                            else if (serviceEntity.CategoryName.Equals("Tiêm phòng", StringComparison.OrdinalIgnoreCase))
                            {
                                allowedDoctorEmails.AddRange(new[] { "bacsichung@gmail.com", "bacsiha@gmail.com" });
                            }
                        }

                        // 1. Lấy tất cả bác sĩ có lịch trực vào ngày hẹn mà thời gian hẹn nằm trong ca trực của họ
                        var doctorsWithSchedules = _unitOfWork.DoctorSchedules.Query()
                            .Where(s => s.WorkDate == targetDateStart && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true
                                        && (!allowedDoctorEmails.Any() || (s.Doctor.Email != null && allowedDoctorEmails.Contains(s.Doctor.Email))))
                            .ToList();

                        List<Guid> doctorsList;
                        if (doctorsWithSchedules.Any())
                        {
                            // Lọc bác sĩ nằm trong khung giờ ca trực
                            doctorsList = doctorsWithSchedules
                                .Where(s => appointmentTime >= s.StartTime && appointmentTime + TimeSpan.FromMinutes(30) <= s.EndTime)
                                .Select(s => s.DoctorId)
                                .Distinct()
                                .ToList();
                            
                            // Nếu không có bác sĩ nào trong khung giờ, fallback sang tất cả bác sĩ có ca trực ngày đó
                            if (!doctorsList.Any())
                            {
                                doctorsList = doctorsWithSchedules
                                    .Select(s => s.DoctorId)
                                    .Distinct()
                                    .ToList();
                            }
                        }
                        else
                        {
                            // Fallback nếu không có cấu hình lịch trực cho ngày đó
                            doctorsList = _unitOfWork.Users.Query()
                                .Where(u => u.Role != null && (u.Role.Name.ToLower() == "clinical_doctor" || u.Role.Name.ToLower() == "vaccination_doctor") && u.IsActive == true
                                        && (!allowedDoctorEmails.Any() || (u.Email != null && allowedDoctorEmails.Contains(u.Email))))
                                .Select(u => u.Id)
                                .ToList();
                        }

                        if (!doctorsList.Any())
                        {
                            throw new InvalidOperationException("Hệ thống hiện không có bác sĩ nào đang trực vào khung giờ này cho dịch vụ bạn chọn!");
                        }

                        // 2. Lọc ra danh sách các bác sĩ THỰC SỰ RẢNH (không trùng lịch trong khoảng +/- 30 phút)
                        // Appointment lưu AppointmentDate (chỉ ngày) + StartTime (giờ) riêng biệt
                        // Cần kết hợp cả hai để so sánh chính xác
                        var allAptsForDoctors = _unitOfWork.Appointments.Query()
                            .Where(a => a.Status != "cancelled"
                                        && a.AppointmentDate == appointmentDate.Date
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

                        var availableDoctors = doctorsList.Except(busyDoctorIds).ToList();

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
                        // Kiểm tra xem bác sĩ được chọn có ca trực trong ngày hẹn không
                        var schedule = _unitOfWork.DoctorSchedules.Query()
                            .FirstOrDefault(s => s.DoctorId == finalDoctorId && s.WorkDate == targetDateStart && s.IsAvailable);
                        
                        if (schedule == null)
                        {
                            throw new InvalidOperationException("Bác sĩ không có lịch trực trong ngày này.");
                        }

                        var appointmentTime = appointmentDate.TimeOfDay;
                        if (appointmentTime < schedule.StartTime || appointmentTime + TimeSpan.FromMinutes(30) > schedule.EndTime)
                        {
                            throw new InvalidOperationException($"Thời gian hẹn phải nằm trong ca trực của bác sĩ ({schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}).");
                        }
                    }

                    // Chặn đặt lịch nếu Bác sĩ đã có lịch trong khoảng +/- 30 phút (Double-Booking Check)
                    // AppointmentDate chỉ lưu ngày (00:00:00), giờ lưu riêng tại StartTime
                    // Phải kết hợp cả hai để kiểm tra trùng lịch chính xác
                    var sameDay_Apts = _unitOfWork.Appointments.Query()
                        .Where(a => a.DoctorId == finalDoctorId
                                    && a.Status != "cancelled"
                                    && a.AppointmentDate == appointmentDate.Date)
                        .Select(a => a.StartTime)
                        .ToList();

                    var isDoubleBooked = sameDay_Apts.Any(startTime => 
                        Math.Abs((startTime - appointmentDate.TimeOfDay).TotalMinutes) < 30);

                    if (isDoubleBooked)
                    {
                        throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
                    }

                    // Sinh QR Token duy nhất
                    string qrToken = string.Empty;
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

                    var appointment = new Appointment
                    {
                        CustomerId = dto.CustomerId,
                        PetId = dto.PetId,
                        ServiceId = dto.ServiceId,
                        DoctorId = finalDoctorId,
                        Symptom = dto.Symptom?.Trim(),
                        Note = dto.Note?.Trim(),
                        Status = "waiting", // Khám ngay / Chờ khám
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow,
                        AppointmentDate = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc),
                        StartTime = appointmentDate.TimeOfDay,
                        QrToken = qrToken
                    };

                    // Nếu thời gian lớn hơn hiện tại 1 giờ thì là đặt lịch trước
                    if (appointmentDate > DateTime.Now.AddHours(1))
                    {
                        appointment.Status = "pending"; 
                    }

                    if (dto.VaccineId.HasValue)
                    {
                        var vaccine = _unitOfWork.Vaccines.Query().FirstOrDefault(v => v.Id == dto.VaccineId.Value);
                        if (vaccine == null)
                        {
                            throw new InvalidOperationException("Không tìm thấy vắc-xin y khoa yêu cầu.");
                        }

                        if (vaccine.StockQuantity <= 0)
                        {
                            throw new InvalidOperationException($"Vắc-xin {vaccine.Name} đã hết hàng trong kho.");
                        }

                        var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.Id == dto.PetId);
                        if (pet == null)
                        {
                            throw new InvalidOperationException("Không tìm thấy thông tin thú cưng.");
                        }

                        var lastRecord = _unitOfWork.VaccinationRecords.Query()
                            .Where(vr => vr.PetId == dto.PetId && vr.VaccineId == dto.VaccineId.Value)
                            .OrderByDescending(vr => vr.InjectionDate)
                            .FirstOrDefault();

                        var validation = _vaccinationScheduleChecker.ValidateInterval(lastRecord, vaccine, appointmentDate, pet);
                        if (!validation.IsValid)
                        {
                            if (!validation.RequiresDoctorOverride)
                            {
                                throw new InvalidOperationException(validation.WarningMessage);
                            }
                            
                            appointment.Note = string.IsNullOrEmpty(appointment.Note) 
                                ? $"[CẢNH BÁO PHÁC ĐỒ] {validation.WarningMessage}"
                                : $"[CẢNH BÁO PHÁC ĐỒ] {validation.WarningMessage}\n{appointment.Note}";
                        }

                        vaccine.StockQuantity -= 1;
                        _unitOfWork.Vaccines.Update(vaccine);
                        appointment.VaccineId = dto.VaccineId;
                    }

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
                            $"Lịch hẹn của bạn vào lúc {appointment.AppointmentDate:HH:mm dd/MM/yyyy} {statusMsg}.",
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

                    // Kiểm tra ca trực của bác sĩ (chỉ kiểm tra khi có DoctorId hợp lệ)
                    if (dto.DoctorId != Guid.Empty)
                    {
                        var schedule = _unitOfWork.DoctorSchedules.Query()
                            .FirstOrDefault(s => s.DoctorId == dto.DoctorId && s.WorkDate == targetDateStart && s.IsAvailable);

                        // Chỉ báo lỗi nếu bác sĩ có cấu hình lịch trực nhưng ngày hẹn không có ca
                        if (schedule != null)
                        {
                            var appointmentTime = appointmentDate.TimeOfDay;
                            if (appointmentTime < schedule.StartTime || appointmentTime + TimeSpan.FromMinutes(30) > schedule.EndTime)
                            {
                                throw new InvalidOperationException($"Thời gian hẹn phải nằm trong ca trực của bác sĩ ({schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}).");
                            }
                        }
                    }

                    // Kiểm tra trùng lịch (Double-Booking Check) nằm trong transaction
                    // AppointmentDate chỉ lưu ngày (00:00:00), giờ lưu riêng tại StartTime
                    var sameDay_Apts_New = _unitOfWork.Appointments.Query()
                        .Where(a => a.DoctorId == dto.DoctorId
                                    && a.Status != "cancelled"
                                    && a.AppointmentDate == appointmentDate.Date)
                        .Select(a => a.StartTime)
                        .ToList();

                    var isDoubleBookedNew = sameDay_Apts_New.Any(startTime =>
                        Math.Abs((startTime - appointmentDate.TimeOfDay).TotalMinutes) < 30);

                    if (isDoubleBookedNew)
                    {
                        throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
                    }

                    // 1. Kiểm tra sđt đã tồn tại chưa
                    var customer = _unitOfWork.Users.Query()
                        .FirstOrDefault(u => u.Phone == dto.CustomerPhone && u.IsActive == true);

                    if (customer == null)
                    {
                        var role = _unitOfWork.Roles.Query().FirstOrDefault(r => r.Name.ToLower() == "customer");
                        customer = new User
                        {
                            Id = Guid.NewGuid(),
                            FullName = dto.CustomerName,
                            Phone = dto.CustomerPhone,
                            Email = $"{dto.CustomerPhone}@noemail.local",
                            RoleId = role?.Id ?? 3,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                        await _unitOfWork.Users.AddAsync(customer);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // 2. Tạo Pet mới
                    var pet = new Pet
                    {
                        CustomerId = customer.Id,
                        Name = dto.PetName,
                        Species = dto.Species,
                        Weight = (decimal?)dto.PetWeight,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Pets.AddAsync(pet);
                    await _unitOfWork.SaveChangesAsync();

                    // Sinh QR Token duy nhất
                    string qrToken = string.Empty;
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

                    // 3. Tạo Lịch hẹn
                    var appointment = new Appointment
                    {
                        CustomerId = customer.Id,
                        PetId = pet.Id,
                        ServiceId = dto.ServiceId,
                        DoctorId = dto.DoctorId,
                        Symptom = dto.Symptom?.Trim(),
                        Note = dto.Note?.Trim(),
                        Status = "waiting", // Khám ngay / Chờ khám
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow,
                        AppointmentDate = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc),   // Chỉ lưu ngày (với Utc kind)
                        StartTime = appointmentDate.TimeOfDay,    // Lưu giờ riêng
                        QrToken = qrToken
                    };

                    if (appointmentDate > DateTime.Now.AddHours(1))
                    {
                        appointment.Status = "pending"; 
                    }

                    await _unitOfWork.Appointments.AddAsync(appointment);
                    await _unitOfWork.SaveChangesAsync();

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
                predicate = a => a.AppointmentDate >= start && a.AppointmentDate <= end && a.DoctorId == doctorId.Value;
            }
            else
            {
                predicate = a => a.AppointmentDate >= start && a.AppointmentDate <= end;
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
            var appointments = await _unitOfWork.Appointments.FindAsync(a => a.Id == id);
            var appointment = appointments.FirstOrDefault();
            if (appointment == null) return false;

            // Kiểm tra tính hợp lệ của việc chuyển đổi trạng thái (State Machine)
            var currentStatus = appointment.Status.ToLower();
            var newStatus = status.ToLower();

            // Nếu đã completed thì không cho lùi về các trạng thái ban đầu
            if (currentStatus == "completed" && newStatus != "completed")
            {
                throw new InvalidOperationException("Chuyển đổi trạng thái không hợp lệ.");
            }
            if (currentStatus == "cancelled" && newStatus != "cancelled")
            {
                throw new InvalidOperationException("Chuyển đổi trạng thái không hợp lệ.");
            }

            appointment.Status = newStatus;
            
            if (newStatus == "cancelled" && !string.IsNullOrWhiteSpace(reason))
            {
                appointment.CancelReason = reason;
            }

            // Nếu Check-in -> chuyển sang waiting và cấp số queue (logic giống Walk-in)
            if (newStatus == "waiting" && appointment.QueueNumber == 0)
            {
                var today = DateTime.UtcNow.Date;
                var todayAppointments = await _unitOfWork.Appointments.FindAsync(x => x.AppointmentDate.Date == today && x.QueueNumber > 0);
                var lastQueue = todayAppointments.Any() ? todayAppointments.Max(x => (int?)x.QueueNumber) ?? 0 : 0;
                
                appointment.QueueNumber = lastQueue + 1;
                appointment.CheckInTime = DateTime.UtcNow;
            }

            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            // Gửi thông báo cho khách hàng
            var message = newStatus == "confirmed" ? $"Lịch hẹn của bạn vào ngày {appointment.AppointmentDate:dd/MM/yyyy} đã được phê duyệt."
                        : newStatus == "cancelled" ? $"Lịch hẹn của bạn vào ngày {appointment.AppointmentDate:dd/MM/yyyy} đã bị hủy."
                        : newStatus == "completed" ? $"Lịch hẹn của bạn vào ngày {appointment.AppointmentDate:dd/MM/yyyy} đã hoàn tất."
                        : $"Trạng thái lịch hẹn của bạn đã thay đổi thành: {newStatus}.";

            if (newStatus == "confirmed" || newStatus == "cancelled" || newStatus == "completed")
            {
                var customerUser = _unitOfWork.Users.Query().FirstOrDefault(u => u.CustomerId == appointment.CustomerId && u.IsActive == true);
                if (customerUser != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        customerUser.Id,
                        "Cập nhật lịch hẹn",
                        message,
                        "AppointmentUpdate"
                    );
                }
            }

            return true;
        }

        public async Task<bool> RescheduleAppointmentAsync(long id, DateTime newDate, bool force = false)
        {
            var targetDate = DateTime.SpecifyKind(newDate, DateTimeKind.Utc);

            if (!force && targetDate < DateTime.UtcNow.AddMinutes(-5)) // Trừ hao 5 phút do lệch giờ
            {
                throw new InvalidOperationException("Không thể dời lịch về quá khứ.");
            }

            var appointments = await _unitOfWork.Appointments.FindAsync(a => a.Id == id);
            var appointment = appointments.FirstOrDefault();
            if (appointment == null) return false;

            var currentStatus = appointment.Status.ToLower();
            if (currentStatus == "completed" || currentStatus == "cancelled" || currentStatus == "no_show")
            {
                throw new InvalidOperationException("Không thể dời lịch hẹn đã kết thúc hoặc bị hủy.");
            }

            // Kiểm tra double booking
            // Sử dụng so sánh loại trừ (strict inequality) để cho phép dời lịch liền kề nhau (back-to-back)
            var isDoubleBooked = await _unitOfWork.Appointments
                .AnyAsync(a => a.DoctorId == appointment.DoctorId 
                            && a.Id != id // Không tính chính nó
                            && a.Status != "cancelled"
                            && a.AppointmentDate > targetDate.AddMinutes(-30) 
                            && a.AppointmentDate < targetDate.AddMinutes(30));

            if (isDoubleBooked)
            {
                throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
            }

            appointment.AppointmentDate = targetDate.Date;
            appointment.StartTime = targetDate.TimeOfDay;
            
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAppointmentDoctorAsync(long id, Guid newDoctorId, bool force = false)
        {
            var appointments = await _unitOfWork.Appointments.FindAsync(a => a.Id == id);
            var appointment = appointments.FirstOrDefault();
            if (appointment == null) return false;

            var currentStatus = appointment.Status.ToLower();
            if (currentStatus == "completed" || currentStatus == "cancelled" || currentStatus == "no_show")
            {
                throw new InvalidOperationException("Không thể đổi bác sĩ cho lịch hẹn đã kết thúc hoặc bị hủy.");
            }

            if (!force)
            {
                // Kiểm tra double booking cho Bác sĩ mới
                var isDoubleBooked = await _unitOfWork.Appointments
                    .AnyAsync(a => a.DoctorId == newDoctorId 
                                && a.Id != id
                                && a.Status != "cancelled"
                                && a.Status != "no_show"
                                && a.AppointmentDate > appointment.AppointmentDate.AddMinutes(-30) 
                                && a.AppointmentDate < appointment.AppointmentDate.AddMinutes(30));

                if (isDoubleBooked)
                {
                    throw new InvalidOperationException("Bác sĩ mới đang có lịch hẹn bị trùng giờ.");
                }
            }

            appointment.DoctorId = newDoctorId;
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
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
                    Price = s.Price
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
            var rawList = _unitOfWork.Appointments.Query()
                .Where(a => a.Status == "pending")
                .OrderBy(a => a.AppointmentDate)
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
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            });

            return await Task.FromResult(mapped);
        }

        public async Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id)
        {
            var a = _unitOfWork.Appointments.Query()
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
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            };

            return await Task.FromResult(mapped);
        }

        public async Task<IEnumerable<AppointmentDetailDto>> GetCustomerAppointmentsAsync(Guid customerId)
        {
            var rawList = _unitOfWork.Appointments.Query()
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.AppointmentDate)
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
                    a.Status,
                    a.QrToken,
                    InvoiceId = a.Invoice != null ? (long?)a.Invoice.Id : null,
                    InvoiceStatus = a.Invoice != null ? a.Invoice.PaymentStatus : null,
                    InvoiceTotalAmount = a.Invoice != null ? (decimal?)a.Invoice.TotalAmount : null,
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
                Status = a.Status,
                QrToken = a.QrToken,
                InvoiceId = a.InvoiceId,
                InvoiceStatus = a.InvoiceStatus,
                InvoiceTotalAmount = a.InvoiceTotalAmount,
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            });

            return await Task.FromResult(mapped);
        }

        public async Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId)
        {
            var rawList = _unitOfWork.Appointments.Query()
                .Where(a => a.PetId == petId)
                .OrderByDescending(a => a.AppointmentDate)
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
                    a.Status,
                    a.QrToken,
                    InvoiceId = a.Invoice != null ? (long?)a.Invoice.Id : null,
                    InvoiceStatus = a.Invoice != null ? a.Invoice.PaymentStatus : null,
                    InvoiceTotalAmount = a.Invoice != null ? (decimal?)a.Invoice.TotalAmount : null,
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
                Status = a.Status,
                QrToken = a.QrToken,
                InvoiceId = a.InvoiceId,
                InvoiceStatus = a.InvoiceStatus,
                InvoiceTotalAmount = a.InvoiceTotalAmount,
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            });

            return await Task.FromResult(mapped);
        }

        public async Task<PaginatedResultDto<AppointmentDetailDto>> GetCustomerAppointmentsPaginatedAsync(Guid customerId, string? status, int page, int pageSize)
        {
            var query = _unitOfWork.Appointments.Query()
                .Where(a => a.CustomerId == customerId);

            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                var lowerStatus = status.ToLower();
                query = query.Where(a => a.Status.ToLower() == lowerStatus);
            }

            var totalCount = query.Count();

            var rawList = query
                .OrderByDescending(a => a.AppointmentDate)
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
                    a.Status,
                    a.QrToken,
                    InvoiceId = a.Invoice != null ? (long?)a.Invoice.Id : null,
                    InvoiceStatus = a.Invoice != null ? a.Invoice.PaymentStatus : null,
                    InvoiceTotalAmount = a.Invoice != null ? (decimal?)a.Invoice.TotalAmount : null,
                    a.VaccineId,
                    VaccineName = a.Vaccine != null ? a.Vaccine.Name : null
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                Status = a.Status,
                QrToken = a.QrToken,
                InvoiceId = a.InvoiceId,
                InvoiceStatus = a.InvoiceStatus,
                InvoiceTotalAmount = a.InvoiceTotalAmount,
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            });

            var result = new PaginatedResultDto<AppointmentDetailDto>(mapped, totalCount, page, pageSize);
            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid CustomerId)
        {
            var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.Id == petId && p.CustomerId == CustomerId);
            if (pet == null)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin bệnh án của thú cưng này.");
            }

            var records = _unitOfWork.MedicalRecords.Query()
                .Where(mr => mr.Appointment != null && mr.Appointment.PetId == petId)
                .OrderByDescending(mr => mr.CreatedAt)
                .Select(mr => new
                {
                    mr.Id,
                    mr.AppointmentId,
                    PetId = mr.PetId,
                    PetName = (mr.Appointment != null && mr.Appointment.Pet != null) ? mr.Appointment.Pet.Name : string.Empty,
                    VisitDate = mr.CreatedAt,
                    RecordType = mr.RecordType,
                    MedicalHistory = mr.MedicalHistory ?? string.Empty,
                    Diagnosis = mr.Diagnosis ?? string.Empty,
                    TreatmentPlan = mr.TreatmentPlan ?? string.Empty,
                    DoctorName = mr.Doctor != null ? mr.Doctor.FullName : string.Empty,
                    DoctorId = mr.DoctorId.ToString(),
                    mr.Weight,
                    mr.Temperature,
                    ClinicalSigns = mr.ClinicalSigns ?? string.Empty,
                    DoctorNotes = mr.DoctorNotes ?? string.Empty,
                    mr.FollowUpDate,
                    InvoiceId = mr.Appointment != null && mr.Appointment.Invoice != null ? (long?)mr.Appointment.Invoice.Id : null,
                    InvoiceStatus = mr.Appointment != null && mr.Appointment.Invoice != null ? mr.Appointment.Invoice.PaymentStatus : null,
                    InvoiceTotalAmount = mr.Appointment != null && mr.Appointment.Invoice != null ? (decimal?)mr.Appointment.Invoice.TotalAmount : null,
                    PrescribedMedicines = mr.Prescriptions
                        .SelectMany(p => p.PrescriptionItems)
                        .Where(pi => pi.Medicine != null)
                        .Select(pi => new PrescribedMedicineDto
                        {
                            MedicineName = pi.Medicine!.Name,
                            Dosage = pi.Dosage,
                            Frequency = pi.Frequency,
                            DurationDays = pi.DurationDays,
                            Quantity = pi.Quantity,
                            Instruction = pi.Instruction
                        })
                        .ToList()
                })
                .ToList();

            var mapped = records.Select(mr => new MedicalRecordDto
            {
                RecordId = mr.Id,
                AppointmentId = mr.AppointmentId,
                PetId = pet.Id,
                PetName = mr.PetName ?? string.Empty,
                VisitDate = mr.VisitDate,
                RecordType = mr.RecordType,
                MedicalHistory = mr.MedicalHistory,
                Diagnosis = mr.Diagnosis,
                TreatmentPlan = mr.TreatmentPlan,
                DoctorName = mr.DoctorName ?? string.Empty,
                DoctorId = mr.DoctorId,
                Weight = mr.Weight,
                Temperature = mr.Temperature,
                ClinicalSigns = mr.ClinicalSigns,
                DoctorNotes = mr.DoctorNotes,
                FollowUpDate = mr.FollowUpDate,
                PrescribedMedicines = mr.PrescribedMedicines,
                InvoiceId = mr.InvoiceId,
                InvoiceStatus = mr.InvoiceStatus,
                InvoiceTotalAmount = mr.InvoiceTotalAmount
            });

            return await Task.FromResult(mapped);
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

            var allowedDoctorEmails = new List<string>();
            if (serviceId.HasValue)
            {
                var service = await _unitOfWork.Services.FindWithIncludesAsync(s => s.Id == serviceId.Value, s => s.Category!);
                var firstService = service.FirstOrDefault();
                if (firstService != null && firstService.Category != null)
                {
                    if (firstService.Category.Name.Equals("Khám bệnh", StringComparison.OrdinalIgnoreCase))
                    {
                        allowedDoctorEmails.AddRange(new[] { "bacsi_test@gmail.com", "bacsituantran@gmail.com" });
                    }
                    else if (firstService.Category.Name.Equals("Tiêm phòng", StringComparison.OrdinalIgnoreCase))
                    {
                        allowedDoctorEmails.AddRange(new[] { "bacsichung@gmail.com", "bacsiha@gmail.com" });
                    }
                }
            }

            // 1. Lấy tất cả ca trực của bác sĩ còn hoạt động vào ngày chỉ định
            var schedules = await _unitOfWork.DoctorSchedules.FindWithIncludesAsync(
                s => s.WorkDate == targetDate && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true
                     && (!allowedDoctorEmails.Any() || (s.Doctor.Email != null && allowedDoctorEmails.Contains(s.Doctor.Email))),
                s => s.Doctor!
            );

            var result = new List<DoctorAvailableSlotsDto>();

            if (schedules.Any())
            {
                foreach (var schedule in schedules)
                {
                    var nextDay = targetDate.AddDays(1);
                    var appointments = await _unitOfWork.Appointments.FindAsync(
                        a => a.DoctorId == schedule.DoctorId 
                          && a.AppointmentDate >= targetDate 
                          && a.AppointmentDate < nextDay
                          && a.Status != "cancelled"
                    );

                    var availableTimes = MyPetClinic.Application.Helpers.SlotCalculationHelper.GetAvailableSlots(schedule, appointments, 30);

                    // Filter by ClinicOperatingShifts if available
                    if (clinicShifts.Any())
                    {
                        availableTimes = availableTimes.Where(t => 
                            clinicShifts.Any(s => s.StartTime <= t.TimeOfDay && s.EndTime >= t.TimeOfDay.Add(TimeSpan.FromMinutes(30)))
                        ).ToList();
                    }

                    result.Add(new DoctorAvailableSlotsDto
                    {
                        DoctorId = schedule.DoctorId,
                        DoctorName = schedule.Doctor?.FullName ?? "Bác sĩ thú y",
                        AvailableSlots = availableTimes.Select(t => t.ToString("HH:mm")).ToList()
                    });
                }
            }
            else
            {
                // Fallback: Nếu hoàn toàn chưa được cấu hình ca trực trong DB cho ngày này, 
                // ta tự động lấy toàn bộ các bác sĩ đang hoạt động và tạo ca trực in-memory dựa trên cấu hình slot_config.json
                var doctors = await _unitOfWork.Users.FindAsync(
                    u => u.Role != null && u.Role.Name.ToLower().Contains("doctor") && u.IsActive == true
                         && (!allowedDoctorEmails.Any() || (u.Email != null && allowedDoctorEmails.Contains(u.Email)))
                );

                if (doctors.Any())
                {
                    string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "slot_config.json");
                    string startTimeStr = "08:00:00";
                    string endTimeStr = "20:00:00";
                    int durationMinutes = 30;

                    if (File.Exists(configPath))
                    {
                        try
                        {
                            var json = File.ReadAllText(configPath);
                            using (var doc = System.Text.Json.JsonDocument.Parse(json))
                            {
                                var root = doc.RootElement;
                                if (root.TryGetProperty("StartTime", out var sProp)) startTimeStr = sProp.GetString() ?? startTimeStr;
                                if (root.TryGetProperty("EndTime", out var eProp)) endTimeStr = eProp.GetString() ?? endTimeStr;
                                if (root.TryGetProperty("DurationMinutes", out var dProp)) durationMinutes = dProp.GetInt32();
                            }
                        }
                        catch {}
                    }

                    var startTime = TimeSpan.Parse(startTimeStr);
                    var endTime = TimeSpan.Parse(endTimeStr);

                    foreach (var doctor in doctors)
                    {
                        var mockSchedule = new DoctorSchedule
                        {
                            DoctorId = doctor.Id,
                            WorkDate = targetDate,
                            StartTime = startTime,
                            EndTime = endTime,
                            IsAvailable = true,
                            Doctor = doctor
                        };

                        var nextDay = targetDate.AddDays(1);
                        var appointments = await _unitOfWork.Appointments.FindAsync(
                            a => a.DoctorId == doctor.Id 
                              && a.AppointmentDate >= targetDate 
                              && a.AppointmentDate < nextDay
                              && a.Status != "cancelled"
                        );

                        var availableTimes = MyPetClinic.Application.Helpers.SlotCalculationHelper.GetAvailableSlots(mockSchedule, appointments, durationMinutes);

                        // Filter by ClinicOperatingShifts if available
                        if (clinicShifts.Any())
                        {
                            availableTimes = availableTimes.Where(t => 
                                clinicShifts.Any(s => s.StartTime <= t.TimeOfDay && s.EndTime >= t.TimeOfDay.Add(TimeSpan.FromMinutes(durationMinutes)))
                            ).ToList();
                        }

                        result.Add(new DoctorAvailableSlotsDto
                        {
                            DoctorId = doctor.Id,
                            DoctorName = doctor.FullName ?? string.Empty,
                            AvailableSlots = availableTimes.Select(t => t.ToString("HH:mm")).ToList()
                        });
                    }
                }
            }

            return result;
        }

        public async Task<AppointmentDetailDto?> CheckInByQrAsync(string qrToken)
        {
            var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => a.QrToken == qrToken,
                a => a.Customer!, a => a.Pet!, a => a.Doctor!, a => a.Service!, a => a.Vaccine!, a => a.Invoice!
            );
            
            var appointment = appointments.FirstOrDefault();

            if (appointment == null)
            {
                return null;
            }

            if (appointment.Status.ToLower() != "confirmed")
            {
                throw new InvalidOperationException($"Lịch hẹn đang ở trạng thái '{appointment.Status}', không thể check-in. Chỉ có thể check-in khi lịch hẹn đã được xác nhận.");
            }

            // Xếp hàng đợi tương tự như Walk-in
            if (appointment.QueueNumber == 0)
            {
                var today = DateTime.UtcNow.Date;
                var todayAppointments = await _unitOfWork.Appointments.FindAsync(x => x.AppointmentDate.Date == today && x.QueueNumber > 0);
                var lastQueue = todayAppointments.Any() ? todayAppointments.Max(x => (int?)x.QueueNumber) ?? 0 : 0;
                
                appointment.QueueNumber = lastQueue + 1;
            }

            appointment.Status = "waiting";
            appointment.CheckInTime = DateTime.UtcNow;

            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            return await GetAppointmentDetailAsync(appointment.Id);
        }
    }
}

