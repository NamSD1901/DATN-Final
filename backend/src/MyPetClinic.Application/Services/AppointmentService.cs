using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, Guid createdBy)
        {
            var appointmentDate = dto.AppointmentDate.HasValue 
                ? DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Utc) 
                : DateTime.UtcNow;

            // Chặn đặt lịch nếu Bác sĩ đã có lịch trong khoảng +/- 30 phút
            var isDoubleBooked = await _unitOfWork.Appointments
                .AnyAsync(a => a.DoctorId == dto.DoctorId 
                            && a.Status != "cancelled"
                            && a.AppointmentDate >= appointmentDate.AddMinutes(-30) 
                            && a.AppointmentDate <= appointmentDate.AddMinutes(30));

            if (isDoubleBooked)
            {
                throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
            }

            var appointment = new Appointment
            {
                CustomerId = dto.CustomerId,
                PetId = dto.PetId,
                ServiceId = dto.ServiceId,
                DoctorId = dto.DoctorId, // Phải truyền DoctorId do DB bắt buộc
                Symptom = dto.Symptom?.Trim(),
                Note = dto.Note?.Trim(),
                Status = "waiting", // Khám ngay / Chờ khám
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                AppointmentDate = appointmentDate,
                QrToken = "QR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
            };

            // Nếu thời gian lớn hơn hiện tại 1 giờ thì là đặt lịch trước
            if (appointment.AppointmentDate > DateTime.UtcNow.AddHours(1))
            {
                appointment.Status = "pending"; 
            }

            await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return appointment.Id;
        }

        public async Task<long> CreateAppointmentWithNewCustomerAsync(AppointmentWithNewCustomerDto dto, Guid createdBy)
        {
            var appointmentDate = dto.AppointmentDate.HasValue 
                ? DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Utc) 
                : DateTime.UtcNow;

            // Kiểm tra trùng lịch
            var isDoubleBooked = await _unitOfWork.Appointments
                .AnyAsync(a => a.DoctorId == dto.DoctorId 
                            && a.Status != "cancelled"
                            && a.AppointmentDate >= appointmentDate.AddMinutes(-30) 
                            && a.AppointmentDate <= appointmentDate.AddMinutes(30));

            if (isDoubleBooked)
            {
                throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra sđt đã tồn tại chưa
                var customers = await _unitOfWork.Users.FindAsync(u => u.Phone == dto.CustomerPhone && u.IsActive == true);
                var customer = customers.FirstOrDefault();

                if (customer == null)
                {
                    var roles = await _unitOfWork.Roles.FindAsync(r => r.Name.ToLower() == "customer");
                    var role = roles.FirstOrDefault();
                    customer = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = dto.CustomerName,
                        Phone = dto.CustomerPhone,
                        Email = $"{dto.CustomerPhone}@noemail.local", // Cấp email giả để qua được NOT NULL constraint của DB
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
                    QrToken = "QR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
                };

                if (appointment.AppointmentDate > DateTime.UtcNow.AddHours(1))
                {
                    appointment.Status = "pending"; 
                }

                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
                return appointment.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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

                var startDateTime = a.AppointmentDate;
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

        public async Task<bool> UpdateAppointmentStatusAsync(long id, string status)
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

        public async Task<bool> RescheduleAppointmentAsync(long id, DateTime newStart)
        {
            var newDate = DateTime.SpecifyKind(newStart, DateTimeKind.Utc);
            if (newDate < DateTime.UtcNow.AddMinutes(-5)) // Trừ hao 5 phút do lệch giờ
            {
                throw new InvalidOperationException("Không thể dời lịch về quá khứ.");
            }

            var appointments = await _unitOfWork.Appointments.FindAsync(a => a.Id == id);
            var appointment = appointments.FirstOrDefault();
            if (appointment == null) return false;

            // Kiểm tra double booking
            var isDoubleBooked = await _unitOfWork.Appointments
                .AnyAsync(a => a.DoctorId == appointment.DoctorId 
                            && a.Id != id // Không tính chính nó
                            && a.Status != "cancelled"
                            && a.AppointmentDate >= newDate.AddMinutes(-30) 
                            && a.AppointmentDate <= newDate.AddMinutes(30));

            if (isDoubleBooked)
            {
                throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
            }

            appointment.AppointmentDate = newDate;
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
            var pendingAppointmentsList = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => a.Status == "pending",
                a => a.Pet!, a => a.Customer!, a => a.Doctor!, a => a.Service!);
                
            var pendingAppointments = pendingAppointmentsList.OrderBy(a => a.AppointmentDate).ToList();

            return pendingAppointments.Select(a => new AppointmentDetailDto
            {
                Id = a.Id,
                PetId = a.PetId,
                PetName = a.Pet?.Name,
                Species = a.Pet?.Species,
                Breed = a.Pet?.Breed,
                Weight = a.Pet?.Weight,
                IsAggressive = a.Pet?.IsAggressive ?? false,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.FullName,
                CustomerPhone = a.Customer?.Phone,
                ServiceId = a.ServiceId,
                ServiceName = a.Service?.Name,
                ServicePrice = a.Service?.Price,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.FullName,
                AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                Symptom = a.Symptom,
                Note = a.Note,
                QrToken = a.QrToken
            });
        }

        public async Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id)
        {
            var appt = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                a => a.Id == id,
                a => a.Pet!, a => a.Customer!, a => a.Doctor!, a => a.Service!);
                
            if (appt == null) return null;
            
            return new AppointmentDetailDto
            {
                Id = appt.Id,
                PetId = appt.PetId,
                PetName = appt.Pet?.Name,
                Species = appt.Pet?.Species,
                Breed = appt.Pet?.Breed,
                Weight = appt.Pet?.Weight,
                IsAggressive = appt.Pet?.IsAggressive ?? false,
                CustomerId = appt.CustomerId,
                CustomerName = appt.Customer?.FullName,
                CustomerPhone = appt.Customer?.Phone,
                ServiceId = appt.ServiceId,
                ServiceName = appt.Service?.Name,
                ServicePrice = appt.Service?.Price,
                DoctorId = appt.DoctorId,
                DoctorName = appt.Doctor?.FullName,
                AppointmentDate = appt.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                Symptom = appt.Symptom,
                Note = appt.Note,
                Status = appt.Status,
                QrToken = appt.QrToken
            };
        }
        public async Task<IEnumerable<AppointmentDetailDto>> GetCustomerAppointmentsAsync(Guid customerId)
        {
            var appts = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => a.CustomerId == customerId,
                a => a.Pet!, a => a.Customer!, a => a.Doctor!, a => a.Service!, a => a.Invoice!
            );
            
            return appts.OrderByDescending(a => a.AppointmentDate).Select(a => new AppointmentDetailDto
            {
                Id = a.Id,
                PetId = a.PetId,
                PetName = a.Pet?.Name,
                Species = a.Pet?.Species,
                Breed = a.Pet?.Breed,
                Weight = a.Pet?.Weight,
                IsAggressive = a.Pet?.IsAggressive ?? false,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.FullName,
                CustomerPhone = a.Customer?.Phone,
                ServiceId = a.ServiceId,
                ServiceName = a.Service?.Name,
                ServicePrice = a.Service?.Price,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.FullName,
                AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                Symptom = a.Symptom,
                Note = a.Note,
                Status = a.Status,
                QrToken = a.QrToken,
                InvoiceId = a.Invoice?.Id,
                InvoiceStatus = a.Invoice?.PaymentStatus,
                InvoiceTotalAmount = a.Invoice?.TotalAmount
            });
        }

        public async Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId)
        {
            var appts = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => a.PetId == petId,
                a => a.Pet!, a => a.Customer!, a => a.Doctor!, a => a.Service!, a => a.Invoice!
            );
            
            return appts.OrderByDescending(a => a.AppointmentDate).Select(a => new AppointmentDetailDto
            {
                Id = a.Id,
                PetId = a.PetId,
                PetName = a.Pet?.Name,
                Species = a.Pet?.Species,
                Breed = a.Pet?.Breed,
                Weight = a.Pet?.Weight,
                IsAggressive = a.Pet?.IsAggressive ?? false,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.FullName,
                CustomerPhone = a.Customer?.Phone,
                ServiceId = a.ServiceId,
                ServiceName = a.Service?.Name,
                ServicePrice = a.Service?.Price,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.FullName,
                AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                Symptom = a.Symptom,
                Note = a.Note,
                Status = a.Status,
                QrToken = a.QrToken,
                InvoiceId = a.Invoice?.Id,
                InvoiceStatus = a.Invoice?.PaymentStatus,
                InvoiceTotalAmount = a.Invoice?.TotalAmount
            });
        }
    }
}

