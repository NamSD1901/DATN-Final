using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        // DbContext is needed for advanced queries, better to inject DbContext here since AppointmentRepository is simple
        private readonly MyPetClinic.Infrastructure.Persistence.ApplicationDbContext _context;

        public AppointmentService(IAppointmentRepository appointmentRepository, MyPetClinic.Infrastructure.Persistence.ApplicationDbContext context)
        {
            _appointmentRepository = appointmentRepository;
            _context = context;
        }

        public async Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, Guid createdBy)
        {
            var appointmentDate = dto.AppointmentDate.HasValue 
                ? DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Utc) 
                : DateTime.UtcNow;

            // Chặn đặt lịch nếu Bác sĩ đã có lịch trong khoảng +/- 30 phút
            var isDoubleBooked = await _context.Appointments
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

            await _appointmentRepository.CreateAppointmentAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return appointment.Id;
        }

        public async Task<long> CreateAppointmentWithNewCustomerAsync(AppointmentWithNewCustomerDto dto, Guid createdBy)
        {
            var appointmentDate = dto.AppointmentDate.HasValue 
                ? DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Utc) 
                : DateTime.UtcNow;

            // Kiểm tra trùng lịch
            var isDoubleBooked = await _context.Appointments
                .AnyAsync(a => a.DoctorId == dto.DoctorId 
                            && a.Status != "cancelled"
                            && a.AppointmentDate >= appointmentDate.AddMinutes(-30) 
                            && a.AppointmentDate <= appointmentDate.AddMinutes(30));

            if (isDoubleBooked)
            {
                throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra sđt đã tồn tại chưa
                var customer = await _context.Users.FirstOrDefaultAsync(u => u.Phone == dto.CustomerPhone && u.IsActive);
                if (customer == null)
                {
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "customer");
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
                    _context.Users.Add(customer);
                    await _context.SaveChangesAsync();
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
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();

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

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return appointment.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<System.Collections.Generic.IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId)
        {
            var query = _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Customer)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end);

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }

            var appointments = await query.ToListAsync();

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
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
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
                var lastQueue = await _context.Appointments
                    .Where(x => x.AppointmentDate.Date == today && x.QueueNumber > 0)
                    .MaxAsync(x => (int?)x.QueueNumber) ?? 0;
                
                appointment.QueueNumber = lastQueue + 1;
                appointment.CheckInTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RescheduleAppointmentAsync(long id, DateTime newStart)
        {
            var newDate = DateTime.SpecifyKind(newStart, DateTimeKind.Utc);
            if (newDate < DateTime.UtcNow.AddMinutes(-5)) // Trừ hao 5 phút do lệch giờ
            {
                throw new InvalidOperationException("Không thể dời lịch về quá khứ.");
            }

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return false;

            // Kiểm tra double booking
            var isDoubleBooked = await _context.Appointments
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
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
