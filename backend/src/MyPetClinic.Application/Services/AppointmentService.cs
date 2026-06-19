using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace MyPetClinic.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVaccinationScheduleChecker _vaccinationScheduleChecker;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private static readonly object _holdLock = new object();

        public AppointmentService(IUnitOfWork unitOfWork, IVaccinationScheduleChecker vaccinationScheduleChecker, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _vaccinationScheduleChecker = vaccinationScheduleChecker;
            _cache = cache;
        }

        private bool IsTransientConflict(Exception ex)
        {
            var msg = ex.ToString().ToLower();
            return msg.Contains("serialization") || msg.Contains("deadlock") || msg.Contains("conflict") || msg.Contains("transaction failed");
        }

        public async Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, Guid createdBy)
        {
            var appointmentDate = dto.AppointmentDate.HasValue 
                ? DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Utc) 
                : DateTime.UtcNow;

            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var finalDoctorId = dto.DoctorId;
                    var targetDateStart = appointmentDate.Date;
                    var targetDateEnd = targetDateStart.AddDays(1);

                    if (!finalDoctorId.HasValue || finalDoctorId.Value == Guid.Empty)
                    {
                        var appointmentTime = appointmentDate.TimeOfDay;
                        // 1. Lấy tất cả bác sĩ có lịch trực vào ngày hẹn mà thời gian hẹn nằm trong ca trực của họ
                        var doctorsWithSchedules = _unitOfWork.DoctorSchedules.Query()
                            .Where(s => s.WorkDate == targetDateStart && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true)
                            .ToList();

                        List<Guid> doctorsList;
                        if (doctorsWithSchedules.Any())
                        {
                            doctorsList = doctorsWithSchedules
                                .Where(s => appointmentTime >= s.StartTime && appointmentTime + TimeSpan.FromMinutes(30) <= s.EndTime)
                                .Select(s => s.DoctorId)
                                .Distinct()
                                .ToList();
                        }
                        else
                        {
                            // Fallback nếu không có cấu hình lịch trực cho ngày đó
                            doctorsList = _unitOfWork.Users.Query()
                                .Where(u => u.Role != null && u.Role.Name.ToLower() == "doctor" && u.IsActive == true)
                                .Select(u => u.Id)
                                .ToList();
                        }

                        if (!doctorsList.Any())
                        {
                            throw new InvalidOperationException("Hệ thống hiện không có bác sĩ nào đang trực vào khung giờ này!");
                        }

                        // 2. Lọc ra danh sách các bác sĩ THỰC SỰ RẢNH (không trùng lịch trong khoảng +/- 30 phút)
                        var busyDoctorIds = _unitOfWork.Appointments.Query()
                            .Where(a => a.Status != "cancelled" 
                                        && a.AppointmentDate > appointmentDate.AddMinutes(-30) 
                                        && a.AppointmentDate < appointmentDate.AddMinutes(30)
                                        && doctorsList.Contains(a.DoctorId))
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
                    // Sử dụng so sánh loại trừ (strict inequality) để cho phép đặt các ca liền kề nhau (back-to-back)
                    var isDoubleBooked = _unitOfWork.Appointments.Query()
                        .Any(a => a.DoctorId == finalDoctorId 
                                    && a.Status != "cancelled"
                                    && a.AppointmentDate > appointmentDate.AddMinutes(-30) 
                                    && a.AppointmentDate < appointmentDate.AddMinutes(30));

                    if (isDoubleBooked)
                    {
                        throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
                    }

                    // --- [BỔ SUNG EDGE CASES] ---
                    
                    // 1. Kiểm tra Giữ chỗ (Hold)
                    var holdKey = $"SlotHold_{finalDoctorId}_{appointmentDate:yyyyMMddHHmm}";
                    if (_cache.TryGetValue(holdKey, out Guid holdingCustomerId))
                    {
                        // Nếu đang bị người khác giữ và người tạo yêu cầu chính là Customer (không phải Lễ tân/Admin)
                        if (holdingCustomerId != dto.CustomerId && createdBy == dto.CustomerId)
                        {
                            throw new InvalidOperationException("Khung giờ này đã bị người khác chọn. Vui lòng chọn khung giờ khác.");
                        }
                    }

                    // 2. Kiểm tra Lead Time và Spam (Chỉ áp dụng cho Customer tự đặt trên web)
                    if (createdBy == dto.CustomerId)
                    {
                        // Lead Time: Phải đặt trước ít nhất 1 tiếng
                        if (appointmentDate > DateTime.UtcNow && appointmentDate < DateTime.UtcNow.AddHours(1))
                        {
                            throw new InvalidOperationException("Vui lòng đặt lịch khám trước ít nhất 1 tiếng để phòng khám kịp chuẩn bị.");
                        }

                        // Không đặt quá khứ
                        if (appointmentDate < DateTime.UtcNow.AddMinutes(-5))
                        {
                            throw new InvalidOperationException("Không thể đặt lịch trong quá khứ.");
                        }

                        // Spam Check: Tối đa 3 lịch hẹn đang Pending/Confirmed
                        var activeApptsCount = _unitOfWork.Appointments.Query()
                            .Count(a => a.CustomerId == dto.CustomerId && (a.Status == "pending" || a.Status == "confirmed"));
                        if (activeApptsCount >= 3)
                        {
                            throw new InvalidOperationException("Bạn đang có quá 3 lịch hẹn chưa khám. Vui lòng hoàn thành hoặc huỷ lịch cũ trước khi đặt thêm.");
                        }
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
                        DoctorId = finalDoctorId.Value,
                        Symptom = dto.Symptom?.Trim(),
                        Note = dto.Note?.Trim(),
                        Status = "waiting", // Khám ngay / Chờ khám
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow,
                        AppointmentDate = appointmentDate.Date,
                        StartTime = appointmentDate.TimeOfDay,
                        QrToken = qrToken
                    };

                    // Nếu thời gian lớn hơn hiện tại 1 giờ thì là đặt lịch trước
                    if (appointmentDate > DateTime.UtcNow.AddHours(1))
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

                    await _unitOfWork.CommitTransactionAsync();
                    
                    // Xoá Hold sau khi đặt lịch thành công
                    var holdKeyToRemove = $"SlotHold_{finalDoctorId}_{appointmentDate:yyyyMMddHHmm}";
                    _cache.Remove(holdKeyToRemove);
                    _cache.Remove($"ActiveHold_{dto.CustomerId}");

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
            var appointmentDate = dto.AppointmentDate.HasValue 
                ? DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Utc) 
                : DateTime.UtcNow;

            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var targetDateStart = appointmentDate.Date;

                    // Kiểm tra ca trực của bác sĩ
                    var schedule = _unitOfWork.DoctorSchedules.Query()
                        .FirstOrDefault(s => s.DoctorId == dto.DoctorId && s.WorkDate == targetDateStart && s.IsAvailable);
                    
                    if (schedule == null)
                    {
                        throw new InvalidOperationException("Bác sĩ không có lịch trực trong ngày này.");
                    }

                    var appointmentTime = appointmentDate.TimeOfDay;
                    if (appointmentTime < schedule.StartTime || appointmentTime + TimeSpan.FromMinutes(30) > schedule.EndTime)
                    {
                        throw new InvalidOperationException($"Thời gian hẹn phải nằm trong ca trực của bác sĩ ({schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}).");
                    }

                    // Kiểm tra trùng lịch (Double-Booking Check) nằm trong transaction
                    // Sử dụng so sánh loại trừ (strict inequality) để cho phép đặt các ca liền kề nhau (back-to-back)
                    var isDoubleBooked = _unitOfWork.Appointments.Query()
                        .Any(a => a.DoctorId == dto.DoctorId 
                                    && a.Status != "cancelled"
                                    && a.AppointmentDate > appointmentDate.AddMinutes(-30) 
                                    && a.AppointmentDate < appointmentDate.AddMinutes(30));

                    if (isDoubleBooked)
                    {
                        throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
                    }

                    // Xử lý Lễ tân chèn ngang (Override Hold)
                    var holdKey = $"SlotHold_{dto.DoctorId}_{appointmentDate:yyyyMMddHHmm}";
                    if (_cache.TryGetValue(holdKey, out Guid holdingCustomerId))
                    {
                        // Xoá Hold của người dùng online để ưu tiên Lễ tân
                        _cache.Remove(holdKey);
                        _cache.Remove($"ActiveHold_{holdingCustomerId}");
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
                        OwnerId = customer.Id,
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
                        AppointmentDate = appointmentDate,
                        QrToken = qrToken
                    };

                    if (appointmentDate > DateTime.UtcNow.AddHours(1))
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
                    Start = startDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                    End = startDateTime.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ss") + "Z", // Default 30 min block
                    Color = color,
                    AllDay = false,
                    ExtendedProps = new
                    {
                        status = a.Status,
                        petName = a.Pet?.Name,
                        species = a.Pet?.Species,
                        breed = a.Pet?.Breed,
                        weight = a.Pet?.Weight,
                        isAggressive = a.Pet?.IsAggressive ?? false,
                        customerName = a.Customer?.FullName,
                        phone = a.Customer?.Phone,
                        symptom = a.Symptom,
                        note = a.Note,
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
            return true;
        }

        public async Task<bool> RescheduleAppointmentAsync(long id, DateTime newStart, bool force = false)
        {
            var newDate = DateTime.SpecifyKind(newStart, DateTimeKind.Utc);
            if (!force && newDate < DateTime.UtcNow.AddMinutes(-5)) // Trừ hao 5 phút do lệch giờ
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
                            && a.AppointmentDate > newDate.AddMinutes(-30) 
                            && a.AppointmentDate < newDate.AddMinutes(30));

            if (isDoubleBooked)
            {
                throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
            }

            appointment.AppointmentDate = newDate;
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
                AppointmentDate = a.AppointmentDate.Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
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
                AppointmentDate = a.AppointmentDate.Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
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
                AppointmentDate = a.AppointmentDate.Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
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
                AppointmentDate = a.AppointmentDate.Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
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
                AppointmentDate = a.AppointmentDate.Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
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

        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid ownerId)
        {
            var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.Id == petId && p.OwnerId == ownerId);
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
                    PetId = mr.Appointment != null ? mr.Appointment.PetId : 0,
                    PetName = (mr.Appointment != null && mr.Appointment.Pet != null) ? mr.Appointment.Pet.Name : string.Empty,
                    VisitDate = mr.CreatedAt,
                    Diagnosis = mr.Diagnosis ?? string.Empty,
                    Treatment = mr.TreatmentPlan ?? string.Empty,
                    DoctorName = mr.Doctor != null ? mr.Doctor.FullName : string.Empty,
                    DoctorId = mr.DoctorId.ToString(),
                    mr.Weight,
                    mr.Temperature,
                    mr.HeartRate,
                    Symptoms = mr.Symptoms ?? string.Empty,
                    Note = mr.Note ?? string.Empty,
                    PrescribedMedicines = mr.Prescriptions
                        .SelectMany(p => p.PrescriptionItems)
                        .Where(pi => pi.Medicine != null)
                        .Select(pi => pi.Medicine!.Name)
                        .ToList()
                })
                .ToList();

            var mapped = records.Select(mr => new MedicalRecordDto
            {
                RecordId = mr.Id,
                AppointmentId = mr.AppointmentId,
                PetId = pet.Id,
                PetName = mr.PetName,
                VisitDate = mr.VisitDate,
                Diagnosis = mr.Diagnosis,
                Treatment = mr.Treatment,
                DoctorName = mr.DoctorName,
                DoctorId = mr.DoctorId,
                Weight = mr.Weight,
                Temperature = mr.Temperature,
                HeartRate = mr.HeartRate,
                Symptoms = mr.Symptoms,
                Note = mr.Note,
                PrescribedMedicines = mr.PrescribedMedicines
            });

            return await Task.FromResult(mapped);
        }

        public async Task<IEnumerable<DoctorAvailableSlotsDto>> GetAvailableSlotsAsync(DateTime date)
        {
            var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            
            // 1. Lấy tất cả ca trực của bác sĩ còn hoạt động vào ngày chỉ định
            var schedules = await _unitOfWork.DoctorSchedules.FindWithIncludesAsync(
                s => s.WorkDate == targetDate && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true,
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

                    // Lọc bỏ các slot đang bị hold bởi bất kỳ ai
                    var freeTimes = availableTimes.Where(t => 
                    {
                        var key = $"SlotHold_{schedule.DoctorId}_{t:yyyyMMddHHmm}";
                        return !_cache.TryGetValue(key, out Guid _);
                    }).ToList();

                    result.Add(new DoctorAvailableSlotsDto
                    {
                        DoctorId = schedule.DoctorId,
                        DoctorName = schedule.Doctor?.FullName ?? "Bác sĩ thú y",
                        AvailableSlots = freeTimes.Select(t => t.ToString("HH:mm")).ToList()
                    });
                }
            }
            else
            {
                // Fallback: Nếu hoàn toàn chưa được cấu hình ca trực trong DB cho ngày này, 
                // ta tự động lấy toàn bộ các bác sĩ đang hoạt động và tạo ca trực in-memory dựa trên cấu hình slot_config.json
                var doctors = await _unitOfWork.Users.FindAsync(
                    u => u.Role != null && u.Role.Name.ToLower() == "doctor" && u.IsActive == true
                );

                if (doctors.Any())
                {
                    string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "slot_config.json");
                    string startTimeStr = "08:00:00";
                    string endTimeStr = "17:00:00";
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

                        var freeTimes = availableTimes.Where(t => 
                        {
                            var key = $"SlotHold_{doctor.Id}_{t:yyyyMMddHHmm}";
                            return !_cache.TryGetValue(key, out Guid _);
                        }).ToList();

                        result.Add(new DoctorAvailableSlotsDto
                        {
                            DoctorId = doctor.Id,
                            DoctorName = doctor.FullName,
                            AvailableSlots = freeTimes.Select(t => t.ToString("HH:mm")).ToList()
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

        public async Task<Guid?> HoldSlotAsync(DateTime slotTime, Guid? doctorId, Guid customerId)
        {
            Guid finalDoctorId;

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
            {
                finalDoctorId = doctorId.Value;
            }
            else
            {
                var targetDateStart = slotTime.Date;
                var targetDateEnd = slotTime.Date.AddDays(1);
                var slotTimeEnd = slotTime.AddMinutes(30);

                var schedules = _unitOfWork.DoctorSchedules.Query()
                    .Where(s => s.WorkDate == targetDateStart && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true)
                    .ToList();

                var doctorsWorking = schedules
                    .Where(s => slotTime.TimeOfDay >= s.StartTime && slotTimeEnd.TimeOfDay <= s.EndTime)
                    .Select(s => s.DoctorId)
                    .ToList();

                if (!doctorsWorking.Any())
                {
                    return null;
                }

                var busyDoctors = _unitOfWork.Appointments.Query()
                    .Where(a => a.Status != "cancelled" && a.AppointmentDate > slotTime.AddMinutes(-30) && a.AppointmentDate < slotTime.AddMinutes(30) && doctorsWorking.Contains(a.DoctorId))
                    .Select(a => a.DoctorId)
                    .ToList();

                var availableDoctors = doctorsWorking.Except(busyDoctors).ToList();

                // Lock check for held slots
                List<Guid> realAvailableDoctors = new List<Guid>();
                lock (_holdLock)
                {
                    realAvailableDoctors = availableDoctors.Where(dId => 
                    {
                        var holdKey = $"SlotHold_{dId}_{slotTime:yyyyMMddHHmm}";
                        return !_cache.TryGetValue(holdKey, out Guid _);
                    }).ToList();
                }

                if (!realAvailableDoctors.Any())
                {
                    return null;
                }

                var doctorApptCounts = _unitOfWork.Appointments.Query()
                    .Where(a => a.AppointmentDate >= targetDateStart && a.AppointmentDate < targetDateEnd && a.Status != "cancelled" && realAvailableDoctors.Contains(a.DoctorId))
                    .GroupBy(a => a.DoctorId)
                    .Select(g => new { DoctorId = g.Key, Count = g.Count() })
                    .ToList();

                finalDoctorId = realAvailableDoctors
                    .Select(id => new { DoctorId = id, Count = doctorApptCounts.FirstOrDefault(c => c.DoctorId == id)?.Count ?? 0 })
                    .OrderBy(x => x.Count)
                    .First().DoctorId;
            }

            var key = $"SlotHold_{finalDoctorId}_{slotTime:yyyyMMddHHmm}";

            lock (_holdLock)
            {
                if (_cache.TryGetValue(key, out Guid holdingCustomerId))
                {
                    if (holdingCustomerId != customerId)
                    {
                        return null; // Đã bị người khác giữ
                    }
                }

                if (_cache.TryGetValue($"ActiveHold_{customerId}", out string oldSlotKey))
                {
                    if (oldSlotKey != key)
                    {
                        _cache.Remove(oldSlotKey); // Huỷ slot cũ
                    }
                }

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(key, customerId, cacheOptions);
                _cache.Set($"ActiveHold_{customerId}", key, cacheOptions);
                
                return finalDoctorId;
            }
        }

        public async Task<bool> ReleaseSlotAsync(DateTime slotTime, Guid? doctorId, Guid customerId)
        {
            lock (_holdLock)
            {
                if (_cache.TryGetValue($"ActiveHold_{customerId}", out string oldSlotKey))
                {
                    if (oldSlotKey.EndsWith($"_{slotTime:yyyyMMddHHmm}"))
                    {
                        _cache.Remove(oldSlotKey);
                        _cache.Remove($"ActiveHold_{customerId}");
                        return true;
                    }
                }

                if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                {
                    var key = $"SlotHold_{doctorId.Value}_{slotTime:yyyyMMddHHmm}";
                    if (_cache.TryGetValue(key, out Guid holdingCustomerId))
                    {
                        if (holdingCustomerId == customerId)
                        {
                            _cache.Remove(key);
                            _cache.Remove($"ActiveHold_{customerId}");
                            return true;
                        }
                    }
                }
                
                return false;
            }
        }
    }
}

