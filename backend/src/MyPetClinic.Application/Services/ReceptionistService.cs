using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

using System.Threading;

namespace MyPetClinic.Application.Services
{
    public class ReceptionistService : IReceptionistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);

        public ReceptionistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<OmniSearchDto>> OmniSearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<OmniSearchDto>();

            var lowerQuery = query.ToLower();

            var matchedPets = await _unitOfWork.Pets.FindAsync(
                p => (p.Name != null && p.Name.ToLower().Contains(lowerQuery)) || 
                     (p.MicrochipCode != null && p.MicrochipCode.ToLower().Contains(lowerQuery))
            );
            var matchedOwnerIds = matchedPets.Select(p => p.OwnerId).ToList();

            var usersList = await _unitOfWork.Users.FindWithIncludesAsync(
                u => u.IsActive == true && u.Role != null && u.Role.Name.ToLower() == "customer" &&
                    ((u.FullName != null && u.FullName.ToLower().Contains(lowerQuery)) ||
                     (u.Phone != null && u.Phone.Contains(lowerQuery)) ||
                     (u.Email != null && u.Email.ToLower().Contains(lowerQuery)) ||
                     matchedOwnerIds.Contains(u.Id)),
                u => u.Role!
            );
            
            var users = usersList.Take(20).ToList();

            var result = new List<OmniSearchDto>();
            foreach (var user in users)
            {
                var userPets = await _unitOfWork.Pets.FindAsync(p => p.OwnerId == user.Id && !p.IsDeceased);
                var pets = userPets.Select(p => new OmniSearchPetDto
                    {
                        PetId = p.Id,
                        Name = p.Name,
                        Species = p.Species,
                        Breed = p.Breed,
                        Weight = p.Weight,
                        MicrochipCode = p.MicrochipCode
                    })
                    .ToList();

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
                appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                    a => a.QrToken == request.QrToken, 
                    a => a.Pet!
                );
            }
            else if (request.AppointmentId.HasValue)
            {
                appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                    a => a.Id == request.AppointmentId.Value,
                    a => a.Pet!
                );
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
                var todayAppointments = await _unitOfWork.Appointments.FindAsync(a => a.AppointmentDate.Date == today && a.QueueNumber > 0);
                var maxQueueToday = todayAppointments.Any() ? todayAppointments.Max(a => (int?)a.QueueNumber) ?? 0 : 0;

                // 3. Cập nhật các trường Workflow
                appointment.Status = "waiting";
                appointment.CheckInTime = DateTime.UtcNow;
                appointment.QueueNumber = maxQueueToday + 1;
                appointment.IsEmergency = request.IsEmergency;

                _unitOfWork.Appointments.Update(appointment);
                await _unitOfWork.SaveChangesAsync();
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
            
            var appointmentsList = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => a.AppointmentDate.Date == today && 
                     (a.Status == "waiting" || a.Status == "in_progress" || a.Status == "ready_to_pay"),
                a => a.Pet!, a => a.Customer!, a => a.Doctor!
            );

            var appointments = appointmentsList
                .OrderByDescending(a => a.IsEmergency) // Ưu tiên ca cấp cứu lên đầu
                .ThenBy(a => a.QueueNumber)           // Sau đó xếp theo số thứ tự
                .ToList();

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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Tìm hoặc tạo Customer
                var customers = await _unitOfWork.Users.FindAsync(u => u.Phone == request.Phone && u.IsActive == true);
                var customer = customers.FirstOrDefault();
                    
                if (customer == null)
                {
                    var roles = await _unitOfWork.Roles.FindAsync(r => r.Name.ToLower() == "customer");
                    var role = roles.FirstOrDefault();
                    customer = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = request.FullName,
                        Phone = request.Phone,
                        RoleId = role?.Id ?? 3, // Giả sử 3 là Customer
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _unitOfWork.Users.AddAsync(customer);
                    await _unitOfWork.SaveChangesAsync(); // Cần save để lấy Id
                }

                // 2. Tái sử dụng hoặc Tạo Pet
                var petNameLower = request.PetName?.Trim().ToLower();
                var pets = await _unitOfWork.Pets.FindAsync(p => p.OwnerId == customer.Id && p.Name != null && p.Name.ToLower() == petNameLower && !p.IsDeceased);
                var pet = pets.FirstOrDefault();

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
                    await _unitOfWork.Pets.AddAsync(pet);
                    await _unitOfWork.SaveChangesAsync(); // Lưu để lấy PetId
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
                        var doctors = await _unitOfWork.Users.FindWithIncludesAsync(
                            u => u.Role != null && u.Role.Name.ToLower() == "doctor" && u.IsActive == true,
                            u => u.Role!
                        );
                        var doctor = doctors.FirstOrDefault();
                            
                        if (doctor != null) {
                            finalDoctorId = doctor.Id;
                        } else {
                            throw new Exception("Hệ thống hiện không có bác sĩ nào đang trực để phân công!");
                        }
                    }

                    var today = DateTime.UtcNow.Date;
                    var todayAppointments = await _unitOfWork.Appointments.FindAsync(a => a.AppointmentDate.Date == today && a.QueueNumber > 0);
                    var maxQueueToday = todayAppointments.Any() ? todayAppointments.Max(a => (int?)a.QueueNumber) ?? 0 : 0;

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
                    
                    await _unitOfWork.Appointments.AddAsync(appointment);
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CommitTransactionAsync();
                    return appointment.Id;
                }
                finally
                {
                    _queueSemaphore.Release();
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateQueueStatusAsync(long appointmentId, string newStatus)
        {
            var appointments = await _unitOfWork.Appointments.FindAsync(a => a.Id == appointmentId);
            var appointment = appointments.FirstOrDefault();
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
            
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEmergencyCustomerAsync(long appointmentId, System.Guid customerId, long petId)
        {
            var appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                a => a.Id == appointmentId,
                a => a.Customer!, a => a.Pet!
            );

            if (appointment == null) return false;

            // Kiểm tra xem đây có phải là ca cấp cứu đang cần update không
            if (!appointment.IsEmergency || appointment.Customer.FullName != "Khách Cấp Cứu")
            {
                return false; 
            }

            var realCustomers = await _unitOfWork.Users.FindAsync(u => u.Id == customerId);
            var realCustomer = realCustomers.FirstOrDefault();
            var realPets = await _unitOfWork.Pets.FindAsync(p => p.Id == petId);
            var realPet = realPets.FirstOrDefault();

            if (realCustomer == null || realPet == null) return false;

            // Xóa Khách ẩn danh / Thú cưng ẩn danh cũ (nếu muốn dọn rác DB)
            var dummyCustomer = appointment.Customer;
            var dummyPet = appointment.Pet;

            // Gán lại
            appointment.CustomerId = customerId;
            appointment.PetId = petId;

            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            // Dọn dẹp dữ liệu rác (tùy chọn)
            if (dummyCustomer.FullName == "Khách Cấp Cứu")
            {
                if (dummyPet != null) _unitOfWork.Pets.Remove(dummyPet);
                if (dummyCustomer != null) _unitOfWork.Users.Remove(dummyCustomer);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<List<DoctorDto>> GetActiveDoctorsAsync()
        {
            var doctors = await _unitOfWork.Users.FindWithIncludesAsync(
                u => u.IsActive == true && u.Role != null && u.Role.Name.ToLower() == "doctor" && u.DeletedAt == null,
                u => u.Role!
            );

            return doctors.Select(u => new DoctorDto { Id = u.Id, FullName = u.FullName }).ToList();
        }

        public async Task<CustomerWithPetsDto?> GetCustomerWithPetsByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var users = await _unitOfWork.Users.FindWithIncludesAsync(
                u => u.Phone == phone.Trim() && u.IsActive == true,
                u => u.Role!
            );
            var user = users.FirstOrDefault();

            if (user == null)
                return null;

            var userPets = await _unitOfWork.Pets.FindAsync(p => p.OwnerId == user.Id && !p.IsDeceased);
            var pets = userPets.Select(p => new PetBasicDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Species = p.Species,
                    Breed = p.Breed,
                    Weight = p.Weight
                })
                .ToList();

            return new CustomerWithPetsDto
            {
                Found = true,
                CustomerId = user.Id,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                Pets = pets
            };
        }
    }
}
