using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;

using System.Threading;

namespace MyPetClinic.Infrastructure.Services
{
    public class ReceptionistService : IReceptionistService
    {
        private readonly ApplicationDbContext _context;
        private static readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);

        public ReceptionistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<OmniSearchDto>> OmniSearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<OmniSearchDto>();

            var lowerQuery = query.ToLower();

            // Lấy danh sách khách hàng khớp thông tin khách HẶC có thú cưng khớp thông tin
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role != null && u.Role.Name == "Customer")
                .Where(u => 
                    (u.FullName != null && u.FullName.ToLower().Contains(lowerQuery)) ||
                    (u.Phone != null && u.Phone.Contains(lowerQuery)) ||
                    (u.Email != null && u.Email.ToLower().Contains(lowerQuery)) ||
                    _context.Pets.Any(p => p.OwnerId == u.Id && 
                        (p.Name != null && p.Name.ToLower().Contains(lowerQuery) || 
                         p.MicrochipCode != null && p.MicrochipCode.ToLower().Contains(lowerQuery)))
                )
                .Take(20) // Limit results for speed
                .ToListAsync();

            var result = new List<OmniSearchDto>();
            foreach (var user in users)
            {
                var pets = await _context.Pets
                    .Where(p => p.OwnerId == user.Id && !p.IsDeceased)
                    .Select(p => new OmniSearchPetDto
                    {
                        PetId = p.Id,
                        Name = p.Name,
                        Species = p.Species,
                        Breed = p.Breed,
                        Weight = p.Weight,
                        MicrochipCode = p.MicrochipCode
                    })
                    .ToListAsync();

                result.Add(new OmniSearchDto
                {
                    CustomerId = user.Id,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Email = user.Email,
                    Pets = pets
                });
            }

            return result;
        }

        public async Task<bool> CheckInAsync(CheckInRequestDto request)
        {
            Appointment? appointment = null;
            if (!string.IsNullOrEmpty(request.QrToken))
            {
                appointment = await _context.Appointments
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.QrToken == request.QrToken);
            }
            else if (request.AppointmentId.HasValue)
            {
                appointment = await _context.Appointments
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == request.AppointmentId.Value);
            }

            if (appointment == null)
                throw new InvalidOperationException("Không tìm thấy Lịch hẹn.");

            if (appointment.Status == "cancelled")
                throw new InvalidOperationException("Lịch hẹn này đã bị hủy, không thể Check-in.");

            if (appointment.Status == "completed")
                throw new InvalidOperationException("Lịch hẹn này đã hoàn thành.");

            if (appointment.Status == "waiting" || appointment.Status == "in_progress" || appointment.Status == "ready_to_pay")
                throw new InvalidOperationException("Khách hàng này đã nằm trong hàng đợi rồi.");

            // Kiểm tra ngày khám
            var localNow = DateTime.UtcNow.AddHours(7).Date; // Giả sử múi giờ Việt Nam (UTC+7)
            var apptDate = appointment.AppointmentDate.AddHours(7).Date;
            if (apptDate != localNow)
                throw new InvalidOperationException($"Lịch hẹn này dành cho ngày {apptDate:dd/MM/yyyy}. Không thể Check-in hôm nay.");

            // 1. Nếu có cân nặng mới thì cập nhật luôn cho Pet
            if (request.CurrentWeight.HasValue && appointment.Pet != null)
            {
                appointment.Pet.Weight = request.CurrentWeight.Value;
            }

            await _queueSemaphore.WaitAsync();
            try
            {
                // 2. Tự động tính toán Queue Number an toàn bằng Semaphore
                var today = DateTime.UtcNow.Date;
                var maxQueueToday = await _context.Appointments
                    .Where(a => a.AppointmentDate.Date == today && a.QueueNumber > 0)
                    .MaxAsync(a => (int?)a.QueueNumber) ?? 0;

                // 3. Cập nhật các trường Workflow
                appointment.Status = "waiting";
                appointment.CheckInTime = DateTime.UtcNow;
                appointment.QueueNumber = maxQueueToday + 1;
                appointment.IsEmergency = request.IsEmergency;

                await _context.SaveChangesAsync();
            }
            finally
            {
                _queueSemaphore.Release();
            }

            return true;
        }

        public async Task<List<QueueItemDto>> GetTodayQueueAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            // Lấy các ca khám CỦA NGÀY HÔM NAY đang ở trạng thái Waiting hoặc InProgress
            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Customer)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate.Date == today && 
                            (a.Status == "waiting" || a.Status == "in_progress" || a.Status == "ready_to_pay"))
                .OrderByDescending(a => a.IsEmergency) // Ưu tiên ca cấp cứu lên đầu
                .ThenBy(a => a.QueueNumber)           // Sau đó xếp theo số thứ tự
                .ToListAsync();

            return appointments.Select(a => new QueueItemDto
            {
                AppointmentId = a.Id,
                PetId = a.PetId,
                PetName = a.Pet?.Name,
                Species = a.Pet?.Species,
                Weight = a.Pet?.Weight,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.FullName,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.FullName,
                Status = a.Status,
                Symptom = a.Symptom,
                IsEmergency = a.IsEmergency,
                IsWalkIn = a.IsWalkIn,
                QueueNumber = a.QueueNumber,
                CheckInTime = a.CheckInTime,
                IsAggressive = a.Pet?.IsAggressive ?? false
            }).ToList();
        }

        public async Task<long> CreateWalkInAsync(WalkInRequestDto request, Guid createdBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm hoặc tạo Customer
                var customer = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == request.Phone && u.IsActive);
                    
                if (customer == null)
                {
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
                    customer = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = request.FullName,
                        Phone = request.Phone,
                        RoleId = role?.Id ?? 3, // Giả sử 3 là Customer
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Users.Add(customer);
                    await _context.SaveChangesAsync(); // Cần save để lấy Id
                }

                // 2. Tái sử dụng hoặc Tạo Pet
                var petNameLower = request.PetName?.Trim().ToLower();
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.OwnerId == customer.Id && p.Name != null && p.Name.ToLower() == petNameLower && !p.IsDeceased);

                if (pet == null)
                {
                    pet = new Pet
                    {
                        OwnerId = customer.Id,
                        Name = request.PetName?.Trim(),
                        Species = request.Species,
                        Breed = request.Breed,
                        Weight = request.Weight,
                        Gender = request.Gender,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Pets.Add(pet);
                    await _context.SaveChangesAsync(); // Lưu để lấy PetId
                }
                else
                {
                    // Cập nhật nhẹ thông tin nếu người dùng có nhập thêm
                    if (request.Weight.HasValue) pet.Weight = request.Weight;
                    if (!string.IsNullOrEmpty(request.Species)) pet.Species = request.Species;
                    // Không cần save ngay, sẽ được save lúc tạo Appointment
                }

                await _queueSemaphore.WaitAsync();
                try
                {
                    // 3. Xử lý DoctorId (tự động phân công nếu để trống)
                    var finalDoctorId = request.DoctorId ?? Guid.Empty;
                    if (finalDoctorId == Guid.Empty)
                    {
                        var doctor = await _context.Users
                            .Include(u => u.Role)
                            .Where(u => u.Role != null && u.Role.Name == "Doctor" && u.IsActive)
                            .FirstOrDefaultAsync();
                            
                        if (doctor != null) {
                            finalDoctorId = doctor.Id;
                        } else {
                            throw new Exception("Hệ thống hiện không có bác sĩ nào đang trực để phân công!");
                        }
                    }

                    // 4. Sinh số Queue an toàn
                    var today = DateTime.UtcNow.Date;
                    var maxQueueToday = await _context.Appointments
                        .Where(a => a.AppointmentDate.Date == today && a.QueueNumber > 0)
                        .MaxAsync(a => (int?)a.QueueNumber) ?? 0;

                    // 5. Tạo Appointment với trạng thái Waiting luôn
                    var appointment = new Appointment
                    {
                        PetId = pet.Id,
                        CustomerId = customer.Id,
                        DoctorId = finalDoctorId,
                        ServiceId = request.ServiceId,
                        AppointmentDate = DateTime.UtcNow,
                        StartTime = DateTime.UtcNow.TimeOfDay,
                        Status = "waiting", // Đã vô phòng khám chờ
                        Symptom = request.Symptom,
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow,
                        
                        // Workflow fields
                        IsWalkIn = true,
                        IsEmergency = request.IsEmergency,
                        CheckInTime = DateTime.UtcNow,
                        QueueNumber = maxQueueToday + 1
                    };
                    
                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return appointment.Id;
                }
                finally
                {
                    _queueSemaphore.Release();
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateQueueStatusAsync(long appointmentId, string newStatus)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return false;

            // Kiểm tra trạng thái hợp lệ
            var validStatuses = new[] { "waiting", "in_progress", "ready_to_pay", "completed", "cancelled" };
            if (!validStatuses.Contains(newStatus.ToLower()))
                return false;

            appointment.Status = newStatus.ToLower();
            
            if (newStatus.ToLower() == "ready_to_pay")
            {
                appointment.CheckOutTime = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEmergencyCustomerAsync(long appointmentId, System.Guid customerId, long petId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return false;

            // Kiểm tra xem đây có phải là ca cấp cứu đang cần update không
            if (!appointment.IsEmergency || appointment.Customer.FullName != "Khách Cấp Cứu")
            {
                return false; 
            }

            var realCustomer = await _context.Users.FindAsync(customerId);
            var realPet = await _context.Pets.FindAsync(petId);

            if (realCustomer == null || realPet == null) return false;

            // Xóa Khách ẩn danh / Thú cưng ẩn danh cũ (nếu muốn dọn rác DB)
            var dummyCustomer = appointment.Customer;
            var dummyPet = appointment.Pet;

            // Gán lại
            appointment.CustomerId = customerId;
            appointment.PetId = petId;

            // Lưu thay đổi Appointment
            await _context.SaveChangesAsync();

            // Dọn dẹp dữ liệu rác (tùy chọn)
            if (dummyCustomer.FullName == "Khách Cấp Cứu")
            {
                _context.Pets.Remove(dummyPet);
                _context.Users.Remove(dummyCustomer);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
