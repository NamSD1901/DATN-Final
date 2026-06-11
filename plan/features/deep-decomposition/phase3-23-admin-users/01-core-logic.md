# 01. Core Business Logic Reference - Admin Staff Management

Tài liệu đặc tả các thuật toán xử lý nghiệp vụ cốt lõi, bảo mật mật mã và mã nguồn C# thực thi phía Backend cho phân hệ Quản trị Nhân sự & Phân quyền Admin.

---

## 1. Thuật toán Sinh mật khẩu Tạm thời An toàn (Secure Password Generation)

Mật khẩu mặc định cấp cho nhân viên mới bắt buộc phải tuân thủ nghiêm ngặt chính sách mật khẩu mạnh của phòng khám nhằm chống tấn công Brute-force.
Sử dụng lớp `RandomNumberGenerator` trong C# để tạo chuỗi ngẫu nhiên có độ tin cậy mật mã học:

```csharp
using System;
using System.Security.Cryptography;
using System.Text;

public static class PasswordGenerator
{
    private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Specials = "!@#$%^&*()_+=-[]{}|;:,.<>?";

    public static string GenerateSecurePassword(int length = 12)
    {
        if (length < 8) throw new ArgumentException("Mật khẩu phải dài tối thiểu 8 ký tự.");

        var res = new StringBuilder();
        using (var rng = RandomNumberGenerator.Create())
        {
            // Bảo đảm chứa ít nhất một ký tự của từng nhóm
            res.Append(GetRandomChar(UpperCase, rng));
            res.Append(GetRandomChar(LowerCase, rng));
            res.Append(GetRandomChar(Digits, rng));
            res.Append(GetRandomChar(Specials, rng));

            string allChars = UpperCase + LowerCase + Digits + Specials;
            for (int i = 4; i < length; i++)
            {
                res.Append(GetRandomChar(allChars, rng));
            }
        }

        // Trộn ngẫu nhiên chuỗi kết quả để không bị cố định vị trí nhóm ký tự
        return ShuffleString(res.ToString());
    }

    private static char GetRandomChar(string charSet, RandomNumberGenerator rng)
    {
        byte[] bytes = new byte[4];
        rng.GetBytes(bytes);
        uint value = BitConverter.ToUInt32(bytes, 0);
        return charSet[(int)(value % (uint)charSet.Length)];
    }

    private static string ShuffleString(string input)
    {
        char[] array = input.ToCharArray();
        using (var rng = RandomNumberGenerator.Create())
        {
            int n = array.Length;
            while (n > 1)
            {
                byte[] box = new byte[4];
                rng.GetBytes(box);
                int k = (int)(BitConverter.ToUInt32(box, 0) % (uint)n);
                n--;
                char value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
        }
        return new string(array);
    }
}
```

---

## 2. Quy tắc Nghiệp vụ Chặn Tự tác động tài khoản Admin (Self-Action Prevention)

Nhằm duy trì tính ổn định của hệ thống quản trị, phòng khám áp dụng 2 quy tắc sắt:
1. **Chặn tự hạ quyền (Self-Demotion Block):** Admin đang đăng nhập không được phép gọi API tự đổi vai trò (`Role`) của mình sang vai trò thấp hơn (`doctor`, `receptionist`...).
2. **Chặn tự khóa tài khoản (Self-Suspension Block):** Admin đang đăng nhập không được tự khóa trạng thái tài khoản của mình sang `Suspended` hoặc `Deactivated`.

### Logic C# Kiểm tra trong Service:
```csharp
if (targetUserId == currentAdminId)
{
    throw new InvalidOperationException("Admin không được phép tự thay đổi vai trò hoặc tự khóa tài khoản của chính mình.");
}
```

---

## 3. Mã nguồn C# Service Thực thi Logic Quản trị Nhân sự

Dưới đây là cài đặt chi tiết của lớp `AdminStaffService` chịu trách nhiệm xử lý nghiệp vụ tại tầng `Application`.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.DTOs.Admin;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Data;

namespace MyPetClinic.Application.Services
{
    public interface IAdminStaffService
    {
        Task<StaffDto> CreateStaffAccountAsync(CreateStaffRequest request, Guid adminId, string ipAddress);
        Task<StaffDto> ChangeStaffRoleAsync(Guid staffId, ChangeRoleRequest request, Guid adminId, string ipAddress);
        Task<StaffDto> ToggleStaffStatusAsync(Guid staffId, Guid adminId, string ipAddress);
        Task<List<StaffDto>> GetAllStaffAccountsAsync();
    }

    public class AdminStaffService : IAdminStaffService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminStaffService> _logger;
        private readonly IEmailService _emailService; // Dịch vụ gửi thư

        public AdminStaffService(AppDbContext context, ILogger<AdminStaffService> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<StaffDto> CreateStaffAccountAsync(CreateStaffRequest request, Guid adminId, string ipAddress)
        {
            // 1. Kiểm tra trùng email
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (emailExists)
            {
                throw new InvalidOperationException($"Địa chỉ Email {request.Email} đã được đăng ký trong hệ thống.");
            }

            // 2. Sinh mật khẩu tạm thời an toàn
            string tempPassword = PasswordGenerator.GenerateSecurePassword(14);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword, workFactor: 11);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Khởi tạo thực thể User
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = hashedPassword,
                    Role = request.Role,
                    Status = "PendingActivation",
                    RequirePasswordChange = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);

                // 4. Tạo ghi nhật ký bảo mật Audit Log
                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorId = adminId,
                    ActionName = "CREATE_STAFF",
                    TargetUserId = newUser.Id,
                    Description = $"Tạo mới nhân viên {request.FullName} với vai trò {request.Role}",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                };
                _context.AuditLogs.Add(log);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 5. Gửi thư chào mừng bất đồng bộ chứa mật khẩu tạm thời
                try
                {
                    await _emailService.SendEmailAsync(
                        newUser.Email,
                        "Chào mừng bạn đến với MyPetClinic - Thông tin tài khoản nhân viên",
                        $"Chào {newUser.FullName}, tài khoản làm việc của bạn đã được khởi tạo.\n\n" +
                        $"Tài khoản đăng nhập: {newUser.Email}\n" +
                        $"Mật khẩu tạm thời: {tempPassword}\n\n" +
                        $"Vui lòng đổi mật khẩu ở lần đăng nhập đầu tiên.");
                }
                catch (Exception mailEx)
                {
                    _logger.LogError(mailEx, $"Không thể gửi email thông tin tài khoản cho {newUser.Email}");
                    // Vẫn cho phép thành công tạo tài khoản, Admin sẽ cấp lại mật khẩu thủ công nếu mail lỗi
                }

                return MapToDto(newUser);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi xảy ra trong giao dịch tạo tài khoản nhân viên.");
                throw;
            }
        }

        public async Task<StaffDto> ChangeStaffRoleAsync(Guid staffId, ChangeRoleRequest request, Guid adminId, string ipAddress)
        {
            if (staffId == adminId)
            {
                throw new InvalidOperationException("Không thể tự thay đổi vai trò của chính mình.");
            }

            var staff = await _context.Users.FindAsync(staffId);
            if (staff == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy tài khoản nhân viên với ID {staffId}");
            }

            string oldRole = staff.Role;
            staff.Role = request.NewRole;

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorId = adminId,
                ActionName = "CHANGE_ROLE",
                TargetUserId = staffId,
                Description = $"Thay đổi vai trò của {staff.FullName} từ {oldRole} sang {request.NewRole}",
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Admin {adminId} thay đổi vai trò của nhân viên {staffId} từ {oldRole} thành {request.NewRole}");
            return MapToDto(staff);
        }

        public async Task<StaffDto> ToggleStaffStatusAsync(Guid staffId, Guid adminId, string ipAddress)
        {
            if (staffId == adminId)
            {
                throw new InvalidOperationException("Không thể tự khóa tài khoản của chính mình.");
            }

            var staff = await _context.Users.FindAsync(staffId);
            if (staff == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy tài khoản nhân viên với ID {staffId}");
            }

            string oldStatus = staff.Status;
            string newStatus = oldStatus == "Active" ? "Suspended" : "Active";
            staff.Status = newStatus;

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorId = adminId,
                ActionName = newStatus == "Suspended" ? "SUSPEND_USER" : "ACTIVATE_USER",
                TargetUserId = staffId,
                Description = $"Thay đổi trạng thái tài khoản của {staff.FullName} từ {oldStatus} sang {newStatus}",
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();

            _logger.LogWarning($"Admin {adminId} đã thay đổi trạng thái nhân viên {staffId} thành {newStatus}");
            return MapToDto(staff);
        }

        public async Task<List<StaffDto>> GetAllStaffAccountsAsync()
        {
            return await _context.Users
                .Where(u => u.Role != "customer")
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new StaffDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    Role = u.Role,
                    Status = u.Status,
                    RequirePasswordChange = u.RequirePasswordChange,
                    CreatedAt = u.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();
        }

        private StaffDto MapToDto(User user)
        {
            return new StaffDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = user.Role,
                Status = user.Status,
                RequirePasswordChange = user.RequirePasswordChange,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
```
