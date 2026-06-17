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

        public async Task<IEnumerable<Service>> GetServicesAsync()
        {
            return await _unitOfWork.Services.GetAllAsync();
        }

        public async Task<Service> CreateServiceAsync(CreateServiceDto dto, string currentUserId)
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
            return service;
        }

        public async Task<Service> UpdateServiceAsync(long id, CreateServiceDto dto, string currentUserId)
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
            return service;
        }

        public async Task DeleteServiceAsync(long id, string currentUserId)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy dịch vụ.");

            service.IsActive = false;
            _unitOfWork.Services.Update(service);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteService", $"Khóa dịch vụ ID {id}");
        }

        public async Task<IEnumerable<Medicine>> GetMedicinesAsync()
        {
            return await _unitOfWork.Medicines.GetAllAsync();
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

        public async Task<Medicine> CreateMedicineAsync(CreateMedicineDto dto, string currentUserId)
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
            return medicine;
        }

        public async Task<Medicine> UpdateMedicineAsync(long id, CreateMedicineDto dto, string currentUserId)
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
            return medicine;
        }

        public async Task DeleteMedicineAsync(long id, string currentUserId)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy thuốc.");

            _unitOfWork.Medicines.Remove(medicine);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteMedicine", $"Xoá thuốc ID {id}");
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

        public async Task<DoctorSchedule> CreateScheduleAsync(CreateScheduleDto dto, string currentUserId)
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
            return schedule;
        }

        public async Task<DoctorSchedule> UpdateScheduleAsync(long id, CreateScheduleDto dto, string currentUserId)
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
            return schedule;
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
