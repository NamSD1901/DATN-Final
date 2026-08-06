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
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVaccinationScheduleChecker _vaccinationScheduleChecker;
        private readonly INotificationService _notificationService;
        private readonly IEmailQueue _emailQueue;

        public AppointmentService(IUnitOfWork unitOfWork, IVaccinationScheduleChecker vaccinationScheduleChecker, INotificationService notificationService, IEmailQueue emailQueue)
        {
            _unitOfWork = unitOfWork;
            _vaccinationScheduleChecker = vaccinationScheduleChecker;
            _notificationService = notificationService;
            _emailQueue = emailQueue;
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

            if (appointmentDate < DateTime.Now.AddMinutes(5))
            {
                throw new InvalidOperationException("Thời gian hẹn không thể nằm trong quá khứ hoặc quá sát giờ hiện tại. Vui lòng chọn khung giờ khác.");
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


                    var targetDateUtc = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc);

                    // Kiểm tra xem khách hàng có lịch hẹn trùng giờ không (cách nhau dưới 30 phút)
                    var customerSameDayApts = _unitOfWork.Appointments.Query()
                        .Where(a => a.CustomerId == dto.CustomerId
                                    && a.Status != "cancelled"
                                    && a.AppointmentDate == targetDateUtc)
                        .Select(a => a.StartTime)
                        .ToList();

                    var isCustomerDoubleBooked = customerSameDayApts.Any(startTime => 
                        Math.Abs((startTime - appointmentDate.TimeOfDay).TotalMinutes) < 30);

                    if (isCustomerDoubleBooked)
                    {
                        string msg = createdBy == dto.CustomerId
                            ? "Bạn đã có lịch hẹn trong khung giờ này. Vui lòng chọn khung giờ khác."
                            : "Khách hàng này đã có lịch hẹn trong khung giờ này. Vui lòng chọn khung giờ khác.";
                        throw new InvalidOperationException(msg);
                    }

                    // Kiểm tra tổng số lịch hẹn đang active (để chống spam)
                    // var activeAppointmentsCount = _unitOfWork.Appointments.Query()
                    //     .Count(a => a.CustomerId == dto.CustomerId
                    //                 && (a.Status == "pending" || a.Status == "confirmed"));

                    // if (activeAppointmentsCount >= 3)
                    // {
                    //     string msg = createdBy == dto.CustomerId
                    //         ? "Bạn đang có 3 lịch hẹn chờ khám. Vui lòng hoàn tất hoặc hủy bớt lịch cũ trước khi đặt lịch mới."
                    //         : "Khách hàng này đang có 3 lịch hẹn chờ khám. Vui lòng hoàn tất hoặc hủy bớt lịch cũ trước khi tạo thêm lịch.";
                    //     throw new InvalidOperationException(msg);
                    // }

                    var finalDoctorId = ResolveAndValidateDoctorId(dto.DoctorId, appointmentDate, dto.ServiceId);

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
                        Status = "pending", // Mặc định là chờ xác nhận
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow,
                        AppointmentDate = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc),
                        StartTime = appointmentDate.TimeOfDay,
                        QrToken = qrToken
                    };

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

        public async Task<bool> UpdateAppointmentStatusAsync(long id, string status, string? reason = null)
        {
            var appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                a => a.Id == id,
                a => a.Customer!, a => a.Pet!, a => a.Doctor!);
                
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
            var message = newStatus == "confirmed" ? $"Lịch hẹn của bạn vào lúc {appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy} đã được phê duyệt."
                        : newStatus == "cancelled" ? $"Lịch hẹn của bạn vào lúc {appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy} đã bị hủy."
                        : newStatus == "completed" ? $"Lịch hẹn của bạn vào lúc {appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy} đã hoàn tất."
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

                    // TH1 & TH2: Gửi email khi xác nhận hoặc hủy lịch
                    if (!string.IsNullOrEmpty(customerUser.Email))
                    {
                        if (newStatus == "confirmed")
                        {
                            var emailHtml = MyPetClinic.Application.Utils.EmailTemplateBuilder.BuildAppointmentConfirmedEmail(
                                customerName: appointment.Customer?.FullName ?? "Khách hàng",
                                petName: appointment.Pet?.Name ?? "thú cưng",
                                appointmentDate: appointment.AppointmentDate,
                                doctorName: appointment.Doctor?.FullName ?? "Bác sĩ",
                                timeSlot: appointment.StartTime.ToString(@"hh\:mm")
                            );
                            await _emailQueue.QueueEmailAsync(new MyPetClinic.Application.DTOs.Notification.EmailMessageDto
                            {
                                ToEmail = customerUser.Email,
                                Subject = "MyPetClinic - Xác nhận đặt lịch khám thành công",
                                BodyHtml = emailHtml
                            });
                        }
                        else if (newStatus == "cancelled")
                        {
                            var emailHtml = MyPetClinic.Application.Utils.EmailTemplateBuilder.BuildAppointmentCancelledEmail(
                                customerName: appointment.Customer?.FullName ?? "Khách hàng",
                                petName: appointment.Pet?.Name ?? "thú cưng",
                                appointmentDate: appointment.AppointmentDate,
                                reason: appointment.CancelReason ?? "Lý do khác"
                            );
                            await _emailQueue.QueueEmailAsync(new MyPetClinic.Application.DTOs.Notification.EmailMessageDto
                            {
                                ToEmail = customerUser.Email,
                                Subject = "MyPetClinic - Thông báo hủy lịch khám",
                                BodyHtml = emailHtml
                            });
                        }
                    }
                }
            }

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
                    Price = s.Price,
                    CategoryId = s.CategoryId,
                    DurationMinutes = s.DurationMinutes,
                    Description = s.Description,
                    IsActive = s.IsActive
                });
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

        public async Task<IEnumerable<AppointmentDetailDto>> GetCustomerAppointmentsAsync(Guid customerId)
        {
            var rawList = _unitOfWork.Appointments.Query().IgnoreQueryFilters()
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .ToList();

            var mapped = await MapToDetailDtoAsync(rawList);

            return mapped;
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

        public async Task<PaginatedResultDto<AppointmentDetailDto>> GetCustomerAppointmentsPaginatedAsync(Guid customerId, string? status, int page, int pageSize)
        {
            var query = _unitOfWork.Appointments.Query().IgnoreQueryFilters()
                .Where(a => a.CustomerId == customerId);

            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                var lowerStatus = status.ToLower();
                query = query.Where(a => a.Status.ToLower() == lowerStatus);
            }

            var totalCount = query.Count();

            var rawList = query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var mapped = await MapToDetailDtoAsync(rawList);

            var result = new PaginatedResultDto<AppointmentDetailDto>(mapped, totalCount, page, pageSize);
            return result;
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

        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid CustomerId)
        {
            var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.Id == petId && p.CustomerId == CustomerId);
            if (pet == null)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin bệnh án của thú cưng này.");
            }

            var records = _unitOfWork.MedicalRecords.Query().IgnoreQueryFilters()
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
            }).ToList();

            var vacRecords = _unitOfWork.VaccinationRecords.Query().IgnoreQueryFilters()
                .Where(v => v.PetId == petId)
                .Select(v => new
                {
                    v.Id,
                    v.AppointmentId,
                    v.DoctorId,
                    DoctorName = v.Doctor != null ? v.Doctor.FullName : string.Empty,
                    v.CreatedAt,
                    v.Weight,
                    v.Temperature,
                    v.ClinicalAssessment,
                    v.ReasonForVisit,
                    v.DoctorRemarks,
                    v.NextDueDate,
                    VaccineName = v.Vaccine != null ? v.Vaccine.Name : string.Empty,
                    v.Dose,
                    InvoiceId = v.Appointment != null && v.Appointment.Invoice != null ? (long?)v.Appointment.Invoice.Id : null,
                    InvoiceStatus = v.Appointment != null && v.Appointment.Invoice != null ? v.Appointment.Invoice.PaymentStatus : null,
                    InvoiceTotalAmount = v.Appointment != null && v.Appointment.Invoice != null ? (decimal?)v.Appointment.Invoice.TotalAmount : null
                })
                .ToList();

            var vacMapped = vacRecords.Select(v => new MedicalRecordDto
            {
                RecordId = v.Id + 1000000, 
                AppointmentId = v.AppointmentId ?? 0,
                PetId = pet.Id,
                PetName = pet.Name ?? string.Empty,
                VisitDate = v.CreatedAt,
                RecordType = "Vaccination",
                MedicalHistory = v.ReasonForVisit ?? "Tiêm phòng",
                Diagnosis = v.VaccineName ?? string.Empty,
                TreatmentPlan = v.Dose != null ? $"Tiêm {v.Dose} ml {v.VaccineName}" : $"Tiêm {v.VaccineName}",
                DoctorName = v.DoctorName ?? string.Empty,
                DoctorId = v.DoctorId.ToString(),
                Weight = v.Weight,
                Temperature = v.Temperature ?? 0m,
                ClinicalSigns = v.ClinicalAssessment ?? string.Empty,
                DoctorNotes = v.DoctorRemarks ?? string.Empty,
                FollowUpDate = v.NextDueDate,
                PrescribedMedicines = new List<PrescribedMedicineDto>(),
                InvoiceId = v.InvoiceId,
                InvoiceStatus = v.InvoiceStatus,
                InvoiceTotalAmount = v.InvoiceTotalAmount
            }).ToList();

            var combinedList = mapped.Concat(vacMapped).OrderByDescending(r => r.VisitDate).ToList();

            return await Task.FromResult(combinedList);
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
                                clinicShifts.Any(s => {
                                    var effEnd = s.EndTime == new TimeSpan(23, 59, 59) ? TimeSpan.FromDays(1) : s.EndTime;
                                    return s.StartTime <= t.TimeOfDay && effEnd >= t.TimeOfDay.Add(TimeSpan.FromMinutes(serviceDuration));
                                })
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

