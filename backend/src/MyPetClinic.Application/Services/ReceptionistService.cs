using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<QrGroupPreviewDto> GetAppointmentPreviewByQrAsync(string qrToken)
        {
            var appointments = await _unitOfWork.Appointments.Query()
                .Include(a => a.Pet).ThenInclude(p => p.Customer)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .Where(a => a.QrToken == qrToken)
                .ToListAsync();

            if (!appointments.Any())
            {
                return new QrGroupPreviewDto
                {
                    QrToken = qrToken,
                    CustomerName = "Không xác định",
                    CustomerPhone = "",
                    HasGlobalError = true,
                    GlobalErrorMessage = "Mã QR không hợp lệ hoặc không tồn tại trên hệ thống."
                };
            }

            var firstAppt = appointments.First();
            var customerName = firstAppt.Pet?.Customer?.FullName ?? "Khách vãng lai";
            var customerPhone = firstAppt.Pet?.Customer?.Phone ?? "";

            var dto = new QrGroupPreviewDto
            {
                QrToken = qrToken,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                HasGlobalError = false,
                GlobalErrorMessage = null,
                Appointments = new List<AppointmentPreviewItemDto>()
            };

            var (todayStartUtc, todayEndUtc) = GetVietnamTodayUtcRange();

            foreach (var a in appointments)
            {
                var itemDto = new AppointmentPreviewItemDto
                {
                    AppointmentId = a.Id,
                    PetName = a.Pet?.Name ?? "Thú cưng",
                    PetSpecies = a.Pet?.Species,
                    PetWeight = (double?)a.Pet?.Weight,
                    DoctorName = a.Doctor?.FullName,
                    ServiceName = a.Service?.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    Status = a.Status,
                    Notes = a.Note,
                    HasError = false,
                    ErrorMessage = null
                };

                // 1. Kiểm tra trạng thái
                if (a.Status == "cancelled")
                {
                    itemDto.HasError = true;
                    itemDto.ErrorMessage = "Lịch hẹn này đã bị hủy.";
                }
                else if (a.Status == "completed")
                {
                    itemDto.HasError = true;
                    itemDto.ErrorMessage = "Lịch hẹn này đã hoàn thành.";
                }
                else if (a.Status == "waiting" || a.Status == "in_progress" || a.Status == "ready_to_pay")
                {
                    itemDto.HasError = true;
                    itemDto.ErrorMessage = "Đã nằm trong hàng đợi.";
                }
                // 2. Kiểm tra ngày giờ
                else if (a.AppointmentDate < todayStartUtc || a.AppointmentDate >= todayEndUtc)
                {
                    var localApptDate = a.AppointmentDate.AddHours(7).Date;
                    itemDto.HasError = true;
                    itemDto.ErrorMessage = $"Lịch hẹn cho ngày {localApptDate:dd/MM/yyyy}.";
                }

                dto.Appointments.Add(itemDto);
            }

            // Nếu TẤT CẢ đều lỗi, thì set Global Error
            if (dto.Appointments.All(x => x.HasError))
            {
                dto.HasGlobalError = true;
                dto.GlobalErrorMessage = "Tất cả các lịch hẹn trong nhóm này đều không đủ điều kiện Check-in (sai ngày, đã hủy hoặc đã nằm trong hàng đợi).";
            }

            return dto;
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

            var customers = await _unitOfWork.Customers.FindAsync(
                c => c.Phone == phone.Trim() && c.DeletedAt == null
            );
            var customer = customers.FirstOrDefault();

            if (customer == null)
                return null;

            var userPets = await _unitOfWork.Pets.FindAsync(p => p.CustomerId == customer.Id && !p.IsDeceased);
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
                CustomerId = customer.Id,
                FullName = customer.FullName,
                Phone = customer.Phone,
                Email = customer.Email,
                Pets = pets
            };
        }

        // --- HÀM REFACTOR TỪ CONTROLLER SANG ---

        public async Task<PaginatedResultDto<UserProfileDto>> GetCustomersPaginatedAsync(string? search, int pageIndex, int pageSize) => await _customerService.GetCustomersPaginatedAsync(search, pageIndex, pageSize);

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
