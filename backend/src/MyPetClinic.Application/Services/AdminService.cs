using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "slot_config.json");

        public AdminService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<object> GetUsersAsync()
        {
            var users = await _unitOfWork.Users.FindWithIncludesAsync(u => u.DeletedAt == null, u => u.Role!);
            return users.Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                role = u.Role?.Name ?? "customer",
                isActive = u.IsActive
            });
        }

        public async Task UpdateUserRoleAsync(string userId, UpdateRoleDto dto, string currentUserId)
        {
            if (currentUserId == userId)
                throw new InvalidOperationException("Bạn không thể tự thay đổi vai trò của chính mình.");

            if (!Guid.TryParse(userId, out var userGuid))
                throw new InvalidOperationException("Id người dùng không hợp lệ.");

            var users = await _unitOfWork.Users.FindAsync(u => u.Id == userGuid);
            var user = users.FirstOrDefault() ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

            var roles = await _unitOfWork.Roles.FindAsync(r => r.Name.ToLower() == dto.NewRole.ToLower());
            var role = roles.FirstOrDefault() ?? throw new InvalidOperationException("Vai trò không hợp lệ.");

            user.RoleId = role.Id;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "ChangeRole", $"Đổi vai trò user {userId} thành {dto.NewRole}");
        }

        public async Task ToggleUserStatusAsync(string userId, ToggleStatusDto dto, string currentUserId)
        {
            if (currentUserId == userId)
                throw new InvalidOperationException("Bạn không thể tự khóa tài khoản của chính mình.");

            if (!Guid.TryParse(userId, out var userGuid))
                throw new InvalidOperationException("Id người dùng không hợp lệ.");

            var users = await _unitOfWork.Users.FindAsync(u => u.Id == userGuid);
            var user = users.FirstOrDefault() ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

            user.IsActive = dto.IsActive;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, dto.IsActive ? "ActivateUser" : "SuspendUser", $"Trạng thái hoạt động user {userId} đặt thành {dto.IsActive}");
        }

        public async Task<IEnumerable<ServiceDto>> GetServicesAsync()
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return services.Select(s => new ServiceDto
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

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, string currentUserId)
        {
            var service = new Service
            {
                Name = dto.Name,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                DurationMinutes = dto.DurationMinutes,
                Description = dto.Description,
                IsActive = true
            };

            await _unitOfWork.Services.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateService", $"Tạo dịch vụ mới: {dto.Name}");
            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                CategoryId = service.CategoryId,
                DurationMinutes = service.DurationMinutes,
                Description = service.Description,
                IsActive = service.IsActive
            };
        }

        public async Task<ServiceDto> UpdateServiceAsync(long id, CreateServiceDto dto, string currentUserId)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy dịch vụ.");

            service.Name = dto.Name;
            service.Price = dto.Price;
            service.CategoryId = dto.CategoryId;
            service.DurationMinutes = dto.DurationMinutes;
            service.Description = dto.Description;

            _unitOfWork.Services.Update(service);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "UpdateService", $"Cập nhật dịch vụ ID {id}: {dto.Name}");
            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                CategoryId = service.CategoryId,
                DurationMinutes = service.DurationMinutes,
                Description = service.Description,
                IsActive = service.IsActive
            };
        }

        public async Task DeleteServiceAsync(long id, string currentUserId)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy dịch vụ.");

            service.IsActive = false;
            _unitOfWork.Services.Update(service);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteService", $"Khóa dịch vụ ID {id}");
        }

        public async Task<IEnumerable<MedicineDto>> GetMedicinesAsync()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();
            return medicines.Select(m => new MedicineDto
            {
                Id = m.Id,
                Name = m.Name,
                Unit = m.Unit ?? "",
                StockQuantity = m.StockQuantity,
                SellPrice = m.SellPrice ?? 0
            });
        }

        public async Task<object> GetMedicineWarningsAsync()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();
            var today = DateTime.UtcNow.Date;
            var expireThreshold = today.AddDays(30);

            var lowStock = medicines.Where(m => m.StockQuantity <= 10).ToList();
            var expiring = medicines.Where(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date <= expireThreshold).ToList();

            return new { lowStock, expiring };
        }

        public async Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto dto, string currentUserId)
        {
            var medicine = new Medicine
            {
                Name = dto.Name,
                Unit = dto.Unit,
                StockQuantity = dto.StockQuantity,
                ImportPrice = dto.ImportPrice,
                SellPrice = dto.SellPrice,
                ExpiryDate = dto.ExpiryDate.HasValue ? DateTime.SpecifyKind(dto.ExpiryDate.Value, DateTimeKind.Utc) : null,
                Description = dto.Description
            };

            await _unitOfWork.Medicines.AddAsync(medicine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateMedicine", $"Tạo thuốc mới: {dto.Name}");
            return new MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                Unit = medicine.Unit ?? "",
                StockQuantity = medicine.StockQuantity,
                SellPrice = medicine.SellPrice ?? 0
            };
        }

        public async Task<MedicineDto> UpdateMedicineAsync(long id, CreateMedicineDto dto, string currentUserId)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thuốc.");

            medicine.Name = dto.Name;
            medicine.Unit = dto.Unit;
            medicine.StockQuantity = dto.StockQuantity;
            medicine.ImportPrice = dto.ImportPrice;
            medicine.SellPrice = dto.SellPrice;
            medicine.ExpiryDate = dto.ExpiryDate.HasValue ? DateTime.SpecifyKind(dto.ExpiryDate.Value, DateTimeKind.Utc) : null;
            medicine.Description = dto.Description;

            _unitOfWork.Medicines.Update(medicine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "UpdateMedicine", $"Cập nhật thuốc ID {id}: {dto.Name}");
            return new MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                Unit = medicine.Unit ?? "",
                StockQuantity = medicine.StockQuantity,
                SellPrice = medicine.SellPrice ?? 0
            };
        }

        public async Task DeleteMedicineAsync(long id, string currentUserId)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thuốc.");

            _unitOfWork.Medicines.Remove(medicine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteMedicine", $"Xoá thuốc ID {id}");
        }

        // ================= VACCINES MANAGEMENT =================
        public async Task<IEnumerable<VaccineAdminDto>> GetVaccinesAsync()
        {
            var vaccines = await _unitOfWork.Vaccines.FindWithIncludesAsync(v => true, v => v.VaccineBatches!);
            
            return vaccines.Select(v => new VaccineAdminDto
            {
                Id = v.Id,
                Name = v.Name,
                Manufacturer = v.Manufacturer,
                Description = v.Description,
                StockQuantity = v.StockQuantity,
                TargetSpecies = v.TargetSpecies,
                MinAgeWeeks = v.MinAgeWeeks,
                IntervalDays = v.IntervalDays,
                Batches = v.VaccineBatches.Select(b => new VaccineBatchAdminDto
                {
                    Id = b.Id,
                    VaccineId = b.VaccineId,
                    BatchNumber = b.BatchNumber,
                    ExpirationDate = b.ExpirationDate,
                    ImportDate = b.ImportDate,
                    StockQuantity = b.StockQuantity,
                    ImportPrice = b.ImportPrice,
                    SellingPrice = b.SellingPrice
                }).OrderBy(b => b.ExpirationDate).ToList()
            }).ToList();
        }

        public async Task<VaccineAdminDto> CreateVaccineAsync(CreateVaccineDto dto, string currentUserId)
        {
            var vaccine = new Vaccine
            {
                Name = dto.Name,
                Manufacturer = dto.Manufacturer,
                Description = dto.Description,
                TargetSpecies = dto.TargetSpecies,
                MinAgeWeeks = dto.MinAgeWeeks,
                IntervalDays = dto.IntervalDays,
                StockQuantity = 0 // Will be updated via batches
            };

            await _unitOfWork.Vaccines.AddAsync(vaccine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateVaccine", $"Thêm loại vắc-xin mới: {dto.Name}");
            return new VaccineAdminDto
            {
                Id = vaccine.Id,
                Name = vaccine.Name,
                Manufacturer = vaccine.Manufacturer,
                Description = vaccine.Description,
                StockQuantity = vaccine.StockQuantity,
                TargetSpecies = vaccine.TargetSpecies,
                MinAgeWeeks = vaccine.MinAgeWeeks,
                IntervalDays = vaccine.IntervalDays
            };
        }

        public async Task<VaccineAdminDto> UpdateVaccineAsync(long id, CreateVaccineDto dto, string currentUserId)
        {
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy vắc-xin.");

            vaccine.Name = dto.Name;
            vaccine.Manufacturer = dto.Manufacturer;
            vaccine.Description = dto.Description;
            vaccine.TargetSpecies = dto.TargetSpecies;
            vaccine.MinAgeWeeks = dto.MinAgeWeeks;
            vaccine.IntervalDays = dto.IntervalDays;

            _unitOfWork.Vaccines.Update(vaccine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "UpdateVaccine", $"Cập nhật loại vắc-xin ID {id}: {dto.Name}");
            return new VaccineAdminDto
            {
                Id = vaccine.Id,
                Name = vaccine.Name,
                Manufacturer = vaccine.Manufacturer,
                Description = vaccine.Description,
                StockQuantity = vaccine.StockQuantity,
                TargetSpecies = vaccine.TargetSpecies,
                MinAgeWeeks = vaccine.MinAgeWeeks,
                IntervalDays = vaccine.IntervalDays
            };
        }

        public async Task DeleteVaccineAsync(long id, string currentUserId)
        {
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy vắc-xin.");
            
            // Check if there are remaining batches with stock
            var batches = await _unitOfWork.VaccineBatches.FindAsync(b => b.VaccineId == id);
            if (batches.Any(b => b.StockQuantity > 0))
            {
                throw new InvalidOperationException("Không thể xoá vắc-xin vẫn còn tồn kho trong các lô.");
            }

            _unitOfWork.Vaccines.Remove(vaccine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteVaccine", $"Xoá vắc-xin ID {id}");
        }

        public async Task<VaccineBatchAdminDto> CreateVaccineBatchAsync(long vaccineId, CreateVaccineBatchDto dto, string currentUserId)
        {
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(vaccineId) ?? throw new KeyNotFoundException("Không tìm thấy vắc-xin.");

            var batch = new VaccineBatch
            {
                VaccineId = vaccineId,
                BatchNumber = dto.BatchNumber,
                ExpirationDate = DateTime.SpecifyKind(dto.ExpirationDate, DateTimeKind.Utc),
                ImportDate = dto.ImportDate.HasValue ? DateTime.SpecifyKind(dto.ImportDate.Value, DateTimeKind.Utc) : DateTime.UtcNow,
                StockQuantity = dto.StockQuantity,
                ImportPrice = dto.ImportPrice,
                SellingPrice = dto.SellingPrice
            };

            await _unitOfWork.VaccineBatches.AddAsync(batch);
            
            // Update aggregate stock
            vaccine.StockQuantity += batch.StockQuantity;
            _unitOfWork.Vaccines.Update(vaccine);

            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateVaccineBatch", $"Nhập lô vắc-xin mới {dto.BatchNumber} cho vắc-xin ID {vaccineId}");
            
            return new VaccineBatchAdminDto
            {
                Id = batch.Id,
                VaccineId = batch.VaccineId,
                BatchNumber = batch.BatchNumber,
                ExpirationDate = batch.ExpirationDate,
                ImportDate = batch.ImportDate,
                StockQuantity = batch.StockQuantity,
                ImportPrice = batch.ImportPrice,
                SellingPrice = batch.SellingPrice
            };
        }

        public async Task DeleteVaccineBatchAsync(long batchId, string currentUserId)
        {
            var batch = await _unitOfWork.VaccineBatches.GetByIdAsync(batchId) ?? throw new KeyNotFoundException("Không tìm thấy lô vắc-xin.");
            
            if (batch.StockQuantity > 0)
            {
                throw new InvalidOperationException("Không thể xoá lô vắc-xin vẫn còn tồn kho. Hãy dùng tính năng huỷ hàng hỏng/hết hạn nếu cần.");
            }

            _unitOfWork.VaccineBatches.Remove(batch);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteVaccineBatch", $"Xoá lô vắc-xin ID {batchId}");
        }

        public async Task<object> GetSchedulesAsync()
        {
            var schedules = await _unitOfWork.DoctorSchedules.FindWithIncludesAsync(s => s.WorkDate >= DateTime.UtcNow.Date.AddDays(-7), s => s.Doctor!);
            return schedules.Select(s => new
            {
                id = s.Id,
                doctorId = s.DoctorId,
                doctorName = s.Doctor?.FullName ?? "Bác sĩ thú y",
                workDate = s.WorkDate.ToString("yyyy-MM-dd"),
                startTime = s.StartTime.ToString(@"hh\:mm"),
                endTime = s.EndTime.ToString(@"hh\:mm"),
                maxAppointments = s.MaxAppointments,
                isAvailable = s.IsAvailable
            });
        }

        public async Task<DoctorScheduleDto> CreateScheduleAsync(CreateScheduleDto dto, string currentUserId)
        {
            var workDate = dto.WorkDate.Date;
            var startTime = TimeSpan.Parse(dto.StartTime);
            var endTime = TimeSpan.Parse(dto.EndTime);

            var existingSchedules = await _unitOfWork.DoctorSchedules.FindAsync(
                s => s.DoctorId == dto.DoctorId && s.WorkDate == workDate && s.IsAvailable
            );

            bool isOverlapping = existingSchedules.Any(s => 
                (startTime >= s.StartTime && startTime < s.EndTime) || 
                (endTime > s.StartTime && endTime <= s.EndTime) ||
                (startTime <= s.StartTime && endTime >= s.EndTime)
            );

            if (isOverlapping)
                throw new InvalidOperationException("Lịch trực của bác sĩ này đang bị trùng lặp thời gian trong ngày.");

            var schedule = new DoctorSchedule
            {
                DoctorId = dto.DoctorId,
                WorkDate = DateTime.SpecifyKind(workDate, DateTimeKind.Utc),
                StartTime = startTime,
                EndTime = endTime,
                MaxAppointments = dto.MaxAppointments,
                IsAvailable = dto.IsAvailable
            };

            await _unitOfWork.DoctorSchedules.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "CreateSchedule", $"Phân ca trực cho bác sĩ {dto.DoctorId} ngày {workDate:dd/MM/yyyy}");
            return new DoctorScheduleDto
            {
                Id = schedule.Id,
                DoctorId = schedule.DoctorId,
                WorkDate = schedule.WorkDate,
                StartTime = schedule.StartTime.ToString(@"hh\:mm"),
                EndTime = schedule.EndTime.ToString(@"hh\:mm"),
                MaxAppointments = schedule.MaxAppointments ?? 0,
                IsAvailable = schedule.IsAvailable
            };
        }

        public async Task<DoctorScheduleDto> UpdateScheduleAsync(long id, CreateScheduleDto dto, string currentUserId)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy lịch trực.");

            var workDate = dto.WorkDate.Date;
            var startTime = TimeSpan.Parse(dto.StartTime);
            var endTime = TimeSpan.Parse(dto.EndTime);

            var existingSchedules = await _unitOfWork.DoctorSchedules.FindAsync(
                s => s.Id != id && s.DoctorId == dto.DoctorId && s.WorkDate == workDate && s.IsAvailable
            );

            bool isOverlapping = existingSchedules.Any(s => 
                (startTime >= s.StartTime && startTime < s.EndTime) || 
                (endTime > s.StartTime && endTime <= s.EndTime) ||
                (startTime <= s.StartTime && endTime >= s.EndTime)
            );

            if (isOverlapping)
                throw new InvalidOperationException("Lịch trực của bác sĩ này đang bị trùng lặp thời gian trong ngày.");

            schedule.WorkDate = DateTime.SpecifyKind(workDate, DateTimeKind.Utc);
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            schedule.MaxAppointments = dto.MaxAppointments;
            schedule.IsAvailable = dto.IsAvailable;

            _unitOfWork.DoctorSchedules.Update(schedule);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "UpdateSchedule", $"Cập nhật lịch trực ID {id}");
            return new DoctorScheduleDto
            {
                Id = schedule.Id,
                DoctorId = schedule.DoctorId,
                WorkDate = schedule.WorkDate,
                StartTime = schedule.StartTime.ToString(@"hh\:mm"),
                EndTime = schedule.EndTime.ToString(@"hh\:mm"),
                MaxAppointments = schedule.MaxAppointments ?? 0,
                IsAvailable = schedule.IsAvailable
            };
        }

        public async Task DeleteScheduleAsync(long id, string currentUserId)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy lịch trực.");

            _unitOfWork.DoctorSchedules.Remove(schedule);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteSchedule", $"Xoá lịch trực ID {id}");
        }

        public SlotConfigModel GetSlotConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new SlotConfigModel
                {
                    StartTime = "08:00:00",
                    EndTime = "17:00:00",
                    DurationMinutes = 30,
                    MaxAppointmentsPerSlot = 3
                };
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<SlotConfigModel>(json) ?? new SlotConfigModel();
            }
            catch
            {
                return new SlotConfigModel();
            }
        }

        public async Task UpdateSlotConfigAsync(SlotConfigModel config, string currentUserId)
        {
            SaveConfig(config);
            await _auditLogService.LogActionAsync(currentUserId, "UpdateSlotConfig", $"Cập nhật cấu hình khung giờ làm việc");
        }

        private static void SaveConfig(SlotConfigModel config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }
}
