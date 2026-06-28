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
        private readonly ICustomerService _customerService;
        private readonly IAppointmentService _appointmentService;
        private static readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);

        public ReceptionistService(IUnitOfWork unitOfWork, ICustomerService customerService, IAppointmentService appointmentService)
        {
            _unitOfWork = unitOfWork;
            _customerService = customerService;
            _appointmentService = appointmentService;
        }

        private (DateTime Start, DateTime End) GetVietnamTodayUtcRange()
        {
            var vnNow = DateTime.UtcNow.AddHours(7);
            var vnTodayStartLocal = vnNow.Date;
            var startUtc = DateTime.SpecifyKind(vnTodayStartLocal.AddHours(-7), DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(startUtc.AddDays(1), DateTimeKind.Utc);
            return (startUtc, endUtc);
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
            var matchedOwnerIds = matchedPets.Select(p => p.CustomerId).ToList();

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
                var userPets = await _unitOfWork.Pets.FindAsync(p => p.CustomerId == user.Id && !p.IsDeceased);
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

        public async Task<AppointmentPreviewDto> GetAppointmentPreviewByQrAsync(string qrToken)
        {
            var appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(
                a => a.QrToken == qrToken,
                a => a.Pet!,
                a => a.Pet!.Customer!,
                a => a.Doctor!,
                a => a.Service!
            );

            if (appointment == null)
            {
                return new AppointmentPreviewDto
                {
                    AppointmentId = 0,
                    QrToken = qrToken,
                    CustomerName = "Không xác định",
                    CustomerPhone = "",
                    PetName = "Không xác định",
                    Status = "invalid",
                    HasError = true,
                    ErrorMessage = "Mã QR không hợp lệ hoặc không tồn tại trên hệ thống."
                };
            }

            bool hasError = false;
            string? errorMessage = null;

            if (appointment.Status == "cancelled")
            {
                hasError = true;
                errorMessage = "Lịch hẹn này đã bị hủy, không thể Check-in.";
            }
            else if (appointment.Status == "completed")
            {
                hasError = true;
                errorMessage = "Lịch hẹn này đã hoàn thành.";
            }
            else if (appointment.Status == "waiting" || appointment.Status == "in_progress" || appointment.Status == "ready_to_pay")
            {
                hasError = true;
                errorMessage = "Khách hàng này đã nằm trong hàng đợi rồi.";
            }
            else
            {
                // Chỉ kiểm tra ngày khám nếu các trạng thái khác hợp lệ
                var (todayStartUtc, todayEndUtc) = GetVietnamTodayUtcRange();
                if (appointment.AppointmentDate < todayStartUtc || appointment.AppointmentDate >= todayEndUtc)
                {
                    var localApptDate = appointment.AppointmentDate.AddHours(7).Date;
                    hasError = true;
                    errorMessage = $"Lịch hẹn này dành cho ngày {localApptDate:dd/MM/yyyy}. Không thể Check-in hôm nay.";
                }
            }

            return new AppointmentPreviewDto
            {
                AppointmentId = appointment.Id,
                QrToken = appointment.QrToken!,
                CustomerName = appointment.Pet?.Customer?.FullName ?? "Khách vãng lai",
                CustomerPhone = appointment.Pet?.Customer?.Phone ?? "",
                PetName = appointment.Pet?.Name ?? "Thú cưng",
                PetSpecies = appointment.Pet?.Species,
                PetWeight = (double?)appointment.Pet?.Weight,
                DoctorName = appointment.Doctor?.FullName,
                ServiceName = appointment.Service?.Name,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                Status = appointment.Status,
                Notes = appointment.Note,
                HasError = hasError,
                ErrorMessage = errorMessage
            };
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
            var (todayStartUtc, todayEndUtc) = GetVietnamTodayUtcRange();
            if (appointment.AppointmentDate < todayStartUtc || appointment.AppointmentDate >= todayEndUtc)
            {
                var localApptDate = appointment.AppointmentDate.AddHours(7).Date;
                throw new InvalidOperationException($"Lịch hẹn này dành cho ngày {localApptDate:dd/MM/yyyy}. Không thể Check-in hôm nay.");
            }

            // 1. Nếu có cân nặng mới thì cập nhật luôn cho Pet
            if (request.CurrentWeight.HasValue && appointment.Pet != null)
            {
                appointment.Pet.Weight = request.CurrentWeight.Value;
            }

            await _queueSemaphore.WaitAsync();
            try
            {
                // 2. Tự động tính toán Queue Number an toàn bằng Semaphore
                var todayAppointments = await _unitOfWork.Appointments.FindAsync(a => 
                    a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc && a.QueueNumber > 0);
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
            var (todayStartUtc, todayEndUtc) = GetVietnamTodayUtcRange();
            
            // Lấy tất cả ca còn active (waiting/in_progress/ready_to_pay):
            // - Hoặc được đặt lịch hôm nay (AppointmentDate)
            // - Hoặc đã check-in hôm nay (CheckInTime) — bắt cả lịch đặt trước nhưng khám hôm nay
            var appointmentsList = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => (a.Status == "waiting" || a.Status == "in_progress" || a.Status == "ready_to_pay")
                     && (
                         (a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc)
                         || (a.CheckInTime != null && a.CheckInTime >= todayStartUtc && a.CheckInTime < todayEndUtc)
                     ),
                a => a.Pet!, a => a.Customer!, a => a.Doctor!
            );

            var appointments = appointmentsList
                .OrderByDescending(a => a.IsEmergency) // Ưu tiên ca cấp cứu lên đầu
                .ThenBy(a => a.QueueNumber > 0 ? a.QueueNumber : int.MaxValue) // Sắp xếp theo số thứ tự, ca chưa có số để cuối
                .ThenBy(a => a.CheckInTime)            // Trong cùng nhóm, theo thời gian check-in
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
                AppointmentDate = a.AppointmentDate,
                IsAggressive = a.Pet?.IsAggressive ?? false
            }).ToList();
        }

        public async Task<long> CreateWalkInAsync(WalkInRequestDto request, Guid createdBy)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Tìm hoặc tạo Customer
                var customers = await _unitOfWork.Customers.FindAsync(c => c.Phone == request.Phone && c.DeletedAt == null);
                var customer = customers.FirstOrDefault();
                    
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        CustomerCode = "CUS-" + DateTime.UtcNow.ToString("yyyyMMdd") + new Random().Next(100, 999).ToString(),
                        FullName = request.FullName,
                        Phone = request.Phone,
                        HasAccount = false,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Customers.AddAsync(customer);
                }

                // 2. Tái sử dụng hoặc Tạo Pet
                var petNameLower = request.PetName?.Trim().ToLower();
                var pets = await _unitOfWork.Pets.FindAsync(p => p.CustomerId == customer.Id && p.Name != null && p.Name.ToLower() == petNameLower && !p.IsDeceased);
                var pet = pets.FirstOrDefault();

                if (pet == null)
                {
                    pet = new Pet
                    {
                        Customer = customer,
                        Name = request.PetName?.Trim(),
                        Species = request.Species,
                        Breed = request.Breed,
                        Weight = request.Weight,
                        Gender = request.Gender,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Pets.AddAsync(pet);
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
                        var doctors = _unitOfWork.Users.Query()
                            .Where(u => u.Role != null && 
                                        (u.Role.Name.ToLower() == "clinical_doctor" || u.Role.Name.ToLower() == "vaccination_doctor" || u.Role.Name.ToLower() == "doctor") && 
                                        u.IsActive == true && u.DeletedAt == null)
                            .ToList();
                        var doctor = doctors.FirstOrDefault();
                            
                        if (doctor != null) {
                            finalDoctorId = doctor.Id;
                        } else {
                            throw new Exception("Hệ thống hiện không có bác sĩ nào đang trực để phân công!");
                        }
                    }

                    var (todayStartUtc, todayEndUtc) = GetVietnamTodayUtcRange();
                    var todayAppointments = await _unitOfWork.Appointments.FindAsync(a => 
                        a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc && a.QueueNumber > 0);
                    var maxQueueToday = todayAppointments.Any() ? todayAppointments.Max(a => (int?)a.QueueNumber) ?? 0 : 0;

                    var utcNow = DateTime.UtcNow;
                    var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    var vnTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTimeZone);

                    // 5. Tạo Appointment với trạng thái Waiting luôn
                    var appointment = new Appointment
                    {
                        Pet = pet,
                        Customer = customer,
                        DoctorId = finalDoctorId,
                        ServiceId = request.ServiceId,
                        AppointmentDate = vnTime,
                        StartTime = vnTime.TimeOfDay,
                        Status = "waiting", // Đã vô phòng khám chờ
                        Symptom = request.Symptom,
                        CreatedBy = createdBy,
                        CreatedAt = utcNow,
                        
                        // Workflow fields
                        IsWalkIn = true,
                        IsEmergency = request.IsEmergency,
                        CheckInTime = vnTime,
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

            // Khi bắt đầu vào hàng chờ hoặc vào phòng khám:
            // đảm bảo CheckInTime và QueueNumber luôn được gán
            if (newStatus.ToLower() == "waiting" || newStatus.ToLower() == "in_progress")
            {
                if (appointment.CheckInTime == null)
                    appointment.CheckInTime = DateTime.UtcNow;

                if (appointment.QueueNumber <= 0)
                {
                    await _queueSemaphore.WaitAsync();
                    try
                    {
                        var (todayStartUtc, todayEndUtc) = GetVietnamTodayUtcRange();
                        var todayAppts = await _unitOfWork.Appointments.FindAsync(a =>
                            a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc
                            && a.QueueNumber > 0 && a.Id != appointmentId);
                        var maxQueue = todayAppts.Any() ? todayAppts.Max(a => (int?)a.QueueNumber) ?? 0 : 0;
                        appointment.QueueNumber = maxQueue + 1;
                    }
                    finally
                    {
                        _queueSemaphore.Release();
                    }
                }
            }

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
            if (!appointment.IsEmergency || appointment.Customer?.FullName != "Khách Cấp Cứu")
            {
                return false; 
            }

            var realCustomers = await _unitOfWork.Customers.FindAsync(c => c.Id == customerId && c.DeletedAt == null);
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
                if (dummyCustomer != null) _unitOfWork.Customers.Remove(dummyCustomer);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<List<DoctorDto>> GetActiveDoctorsAsync()
        {
            var doctors = _unitOfWork.Users.Query()
                .Where(u => u.Role != null && 
                            (u.Role.Name.ToLower() == "clinical_doctor" || u.Role.Name.ToLower() == "vaccination_doctor" || u.Role.Name.ToLower() == "doctor") && 
                            u.IsActive == true && u.DeletedAt == null)
                .ToList();
                
            return doctors.Select(u => new DoctorDto { Id = u.Id, FullName = u.FullName ?? string.Empty }).ToList();
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

            var userPets = await _unitOfWork.Pets.FindAsync(p => p.CustomerId == user.Id && !p.IsDeceased);
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

        // --- HÀM REFACTOR TỪ CONTROLLER SANG ---

        public async Task<IEnumerable<UserProfileDto>> GetAllCustomersAsync() => await _customerService.GetAllCustomersAsync();

        public async Task<IEnumerable<UserProfileDto>> SearchCustomersAsync(string search) => await _customerService.SearchCustomersAsync(search);

        public async Task<System.Guid> CreateCustomerWithPetsAsync(CustomerCreateDto dto) => await _customerService.CreateCustomerWithPetsAsync(dto);

        public async Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, System.Guid createdBy) => await _appointmentService.CreateAppointmentAsync(dto, createdBy);

        public async Task<CustomerDashboardDetailDto?> GetCustomerDashboardDetailAsync(System.Guid id)
        {
            var customer = await _customerService.GetCustomerDetailAsync(id);
            if (customer == null) return null;

            var pets = await _customerService.GetPetsByCustomerAsync(id);
            var doctors = await GetActiveDoctorsAsync();
            var services = await _appointmentService.GetServicesAsync();
            var appointments = await _appointmentService.GetCustomerAppointmentsAsync(id);

            return new CustomerDashboardDetailDto
            {
                Customer = customer,
                Pets = pets,
                ActiveDoctors = doctors,
                Services = services,
                Appointments = appointments,
                TotalVisits = appointments.Count(a => a.Status == "completed" || a.Status == "ready_to_pay"),
                TotalSpent = appointments.Where(a => a.InvoiceStatus == "paid").Sum(a => a.InvoiceTotalAmount ?? 0),
                NoShowCount = appointments.Count(a => a.Status == "cancelled"),
                UnpaidBalance = appointments.Where(a => a.InvoiceStatus == "unpaid").Sum(a => a.InvoiceTotalAmount ?? 0)
            };
        }

        public async Task<PetDashboardDetailDto?> GetPetDashboardDetailAsync(long id)
        {
            var pet = await _unitOfWork.Pets.GetFirstOrDefaultWithIncludesAsync(p => p.Id == id, p => p.Customer!);
            if (pet == null) return null;

            var appointments = await _appointmentService.GetPetAppointmentsAsync(id);

            return new PetDashboardDetailDto
            {
                Pet = new PetDto 
                { 
                    Id = pet.Id, 
                    CustomerId = pet.CustomerId, 
                    Name = pet.Name, 
                    Species = pet.Species, 
                    Breed = pet.Breed, 
                    Gender = pet.Gender, 
                    BirthDate = pet.BirthDate, 
                    Weight = pet.Weight, 
                    Color = pet.Color, 
                    BloodType = pet.BloodType, 
                    Sterilized = pet.Sterilized, 
                    MicrochipCode = pet.MicrochipCode, 
                    AllergyNote = pet.AllergyNote 
                },
                Customer = new UserProfileDto 
                { 
                    Id = pet.Customer?.Id ?? Guid.Empty, 
                    FullName = pet.Customer?.FullName ?? string.Empty, 
                    Email = pet.Customer?.Email ?? string.Empty, 
                    Phone = pet.Customer?.Phone ?? string.Empty, 
                    Address = pet.Customer?.Address, 
                    Gender = pet.Customer?.Gender, 
                    DateOfBirth = pet.Customer?.DateOfBirth, 
                    Avatar = pet.Customer?.Avatar, 
                    RoleName = "Customer" 
                },
                Appointments = appointments
            };
        }

        public async Task<long> AddPetAsync(System.Guid customerId, CreatePetDto model)
        {
            var customer = await _customerService.GetCustomerDetailAsync(customerId);
            if (customer == null) throw new InvalidOperationException("Không tìm thấy khách hàng");

            var newPet = new MyPetClinic.Domain.Entities.Pet
            {
                CustomerId = customerId,
                Name = model.Name?.Trim(),
                Species = model.Species?.Trim(),
                Breed = model.Breed?.Trim(),
                Gender = model.Gender,
                BirthDate = model.BirthDate,
                Weight = model.Weight,
                Color = model.Color?.Trim(),
                AllergyNote = model.AllergyNote?.Trim(),
                Sterilized = model.Sterilized,
                MicrochipCode = model.MicrochipCode?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Pets.AddAsync(newPet);
            await _unitOfWork.SaveChangesAsync();

            return newPet.Id;
        }

        public async Task UpdatePetAsync(long petId, UpdatePetDto model)
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            if (pet == null) throw new InvalidOperationException("Không tìm thấy thú cưng");

            pet.Name = model.Name?.Trim();
            pet.Species = model.Species?.Trim();
            pet.Breed = model.Breed?.Trim();
            pet.Gender = model.Gender;
            pet.BirthDate = model.BirthDate;
            pet.Weight = model.Weight;
            pet.Color = model.Color?.Trim();
            pet.AllergyNote = model.AllergyNote?.Trim();
            pet.Sterilized = model.Sterilized;
            pet.MicrochipCode = model.MicrochipCode?.Trim();

            _unitOfWork.Pets.Update(pet);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
