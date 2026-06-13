using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "admin,Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "slot_config.json");

        public AdminController(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        // ================= USER MANAGEMENT =================

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _unitOfWork.Users.FindWithIncludesAsync(u => u.DeletedAt == null, u => u.Role!);
            var result = users.Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                role = u.Role?.Name ?? "customer",
                isActive = u.IsActive
            });
            return Ok(result);
        }

        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateRoleDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == userId)
            {
                return BadRequest(new { message = "Bạn không thể tự thay đổi vai trò của chính mình." });
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(new { message = "Id người dùng không hợp lệ." });
            }

            var users = await _unitOfWork.Users.FindAsync(u => u.Id == userGuid);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            var roles = await _unitOfWork.Roles.FindAsync(r => r.Name.ToLower() == dto.NewRole.ToLower());
            var role = roles.FirstOrDefault();
            if (role == null)
            {
                return BadRequest(new { message = "Vai trò không hợp lệ." });
            }

            user.RoleId = role.Id;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId!, "ChangeRole", $"Đổi vai trò user {userId} thành {dto.NewRole}");
            return Ok(new { message = "Cập nhật vai trò thành công." });
        }

        [HttpPut("users/{userId}/status")]
        public async Task<IActionResult> ToggleUserStatus(string userId, [FromBody] ToggleStatusDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == userId)
            {
                return BadRequest(new { message = "Bạn không thể tự khóa tài khoản của chính mình." });
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(new { message = "Id người dùng không hợp lệ." });
            }

            var users = await _unitOfWork.Users.FindAsync(u => u.Id == userGuid);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            user.IsActive = dto.IsActive;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId!, dto.IsActive ? "ActivateUser" : "SuspendUser", $"Trạng thái hoạt động user {userId} đặt thành {dto.IsActive}");
            return Ok(new { message = "Cập nhật trạng thái người dùng thành công." });
        }

        // ================= SERVICE MANAGEMENT =================

        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _unitOfWork.Services.GetAllAsync();
            return Ok(services);
        }

        [HttpPost("services")]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "CreateService", $"Tạo dịch vụ mới: {dto.Name}");

            return Ok(new { success = true, service });
        }

        [HttpPut("services/{id}")]
        public async Task<IActionResult> UpdateService(long id, [FromBody] CreateServiceDto dto)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id);
            if (service == null)
            {
                return NotFound(new { message = "Không tìm thấy dịch vụ." });
            }

            service.Name = dto.Name;
            service.Price = dto.Price;
            service.CategoryId = dto.CategoryId;
            service.DurationMinutes = dto.DurationMinutes;
            service.Description = dto.Description;

            _unitOfWork.Services.Update(service);
            await _unitOfWork.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "UpdateService", $"Cập nhật dịch vụ ID {id}: {dto.Name}");

            return Ok(new { success = true, service });
        }

        [HttpDelete("services/{id}")]
        public async Task<IActionResult> DeleteService(long id)
        {
            var service = await _unitOfWork.Services.GetByIdAsync(id);
            if (service == null)
            {
                return NotFound(new { message = "Không tìm thấy dịch vụ." });
            }

            service.IsActive = false;
            _unitOfWork.Services.Update(service);
            await _unitOfWork.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "DeleteService", $"Khóa dịch vụ ID {id}");

            return Ok(new { success = true });
        }

        // ================= MEDICINES MANAGEMENT =================

        [HttpGet("medicines")]
        public async Task<IActionResult> GetMedicines()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();
            return Ok(medicines);
        }

        [HttpGet("medicines/warnings")]
        public async Task<IActionResult> GetMedicineWarnings()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();
            var today = DateTime.UtcNow.Date;
            var expireThreshold = today.AddDays(30);

            var lowStock = medicines.Where(m => m.StockQuantity <= 10).ToList();
            var expiring = medicines.Where(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date <= expireThreshold).ToList();

            return Ok(new
            {
                lowStock,
                expiring
            });
        }

        [HttpPost("medicines")]
        public async Task<IActionResult> CreateMedicine([FromBody] CreateMedicineDto dto)
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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "CreateMedicine", $"Tạo thuốc mới: {dto.Name}");

            return Ok(new { success = true, medicine });
        }

        [HttpPut("medicines/{id}")]
        public async Task<IActionResult> UpdateMedicine(long id, [FromBody] CreateMedicineDto dto)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id);
            if (medicine == null)
            {
                return NotFound(new { message = "Không tìm thấy thuốc." });
            }

            medicine.Name = dto.Name;
            medicine.Unit = dto.Unit;
            medicine.StockQuantity = dto.StockQuantity;
            medicine.ImportPrice = dto.ImportPrice;
            medicine.SellPrice = dto.SellPrice;
            medicine.ExpiryDate = dto.ExpiryDate.HasValue ? DateTime.SpecifyKind(dto.ExpiryDate.Value, DateTimeKind.Utc) : null;
            medicine.Description = dto.Description;

            _unitOfWork.Medicines.Update(medicine);
            await _unitOfWork.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "UpdateMedicine", $"Cập nhật thuốc ID {id}: {dto.Name}");

            return Ok(new { success = true, medicine });
        }

        [HttpDelete("medicines/{id}")]
        public async Task<IActionResult> DeleteMedicine(long id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id);
            if (medicine == null)
            {
                return NotFound(new { message = "Không tìm thấy thuốc." });
            }

            _unitOfWork.Medicines.Remove(medicine);
            await _unitOfWork.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "DeleteMedicine", $"Xoá thuốc ID {id}");

            return Ok(new { success = true });
        }

        // ================= SCHEDULES MANAGEMENT =================

        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            var schedules = await _unitOfWork.DoctorSchedules.FindWithIncludesAsync(s => s.WorkDate >= DateTime.UtcNow.Date.AddDays(-7), s => s.Doctor!);
            var result = schedules.Select(s => new
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
            return Ok(result);
        }

        [HttpPost("schedules")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto dto)
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
            {
                return BadRequest(new { message = "Lịch trực của bác sĩ này đang bị trùng lặp thời gian trong ngày." });
            }

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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "CreateSchedule", $"Phân ca trực cho bác sĩ {dto.DoctorId} ngày {workDate:dd/MM/yyyy}");

            return Ok(new { success = true, schedule });
        }

        [HttpPut("schedules/{id}")]
        public async Task<IActionResult> UpdateSchedule(long id, [FromBody] CreateScheduleDto dto)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch trực." });
            }

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
            {
                return BadRequest(new { message = "Lịch trực của bác sĩ này đang bị trùng lặp thời gian trong ngày." });
            }

            schedule.WorkDate = DateTime.SpecifyKind(workDate, DateTimeKind.Utc);
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            schedule.MaxAppointments = dto.MaxAppointments;
            schedule.IsAvailable = dto.IsAvailable;

            _unitOfWork.DoctorSchedules.Update(schedule);
            await _unitOfWork.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "UpdateSchedule", $"Cập nhật lịch trực ID {id}");

            return Ok(new { success = true, schedule });
        }

        [HttpDelete("schedules/{id}")]
        public async Task<IActionResult> DeleteSchedule(long id)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch trực." });
            }

            _unitOfWork.DoctorSchedules.Remove(schedule);
            await _unitOfWork.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditLogService.LogActionAsync(currentUserId!, "DeleteSchedule", $"Xoá lịch trực ID {id}");

            return Ok(new { success = true });
        }

        // ================= SLOT CONFIGURATION =================

        [HttpGet("slots/config")]
        public IActionResult GetSlotConfig()
        {
            var config = LoadConfig();
            return Ok(config);
        }

        [HttpPut("slots/config")]
        public IActionResult UpdateSlotConfig([FromBody] SlotConfigModel config)
        {
            SaveConfig(config);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _auditLogService.LogActionAsync(currentUserId!, "UpdateSlotConfig", $"Cập nhật cấu hình khung giờ làm việc");
            return Ok(new { success = true, config });
        }

        private static SlotConfigModel LoadConfig()
        {
            if (!System.IO.File.Exists(ConfigPath))
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
                var json = System.IO.File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<SlotConfigModel>(json) ?? new SlotConfigModel();
            }
            catch
            {
                return new SlotConfigModel();
            }
        }

        private static void SaveConfig(SlotConfigModel config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }

    public class UpdateRoleDto
    {
        public string NewRole { get; set; } = string.Empty;
    }

    public class ToggleStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class CreateServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public long CategoryId { get; set; }
        public int DurationMinutes { get; set; }
        public string? Description { get; set; }
    }

    public class SlotConfigModel
    {
        public string StartTime { get; set; } = "08:00:00";
        public string EndTime { get; set; } = "17:00:00";
        public int DurationMinutes { get; set; } = 30;
        public int MaxAppointmentsPerSlot { get; set; } = 3;
    }

    public class CreateMedicineDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public int StockQuantity { get; set; }
        public decimal? ImportPrice { get; set; }
        public decimal? SellPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
    }

    public class CreateScheduleDto
    {
        public Guid DoctorId { get; set; }
        public DateTime WorkDate { get; set; }
        public string StartTime { get; set; } = "08:00";
        public string EndTime { get; set; } = "12:00";
        public int MaxAppointments { get; set; } = 10;
        public bool IsAvailable { get; set; } = true;
    }
}
