# 🌐 Infrastructure & Security - Admin Staff Management

## 🔗 Skills Liên Quan
- **BE-A03 (RBAC):** `[Authorize(Roles = "admin")]` tại lớp Controller.
- **BE-C01 (Identity Configuration):** Cấu hình khóa tài khoản tự động (MaxFailedAccessAttempts) và thời hạn khóa trong cấu hình Startup để tăng cường độ bảo mật tổng thể.

---

## 1. Cấu hình Identity Security (Program.cs)

Đăng ký cấu hình bảo mật mật khẩu và cơ chế khóa tài khoản tự động khi nhập sai mật khẩu nhiều lần:

```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    // Cấu hình Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5; // Khóa sau 5 lần gõ sai mật khẩu
    options.Lockout.AllowedForNewUsers = true;

    // Yêu cầu mật khẩu mạnh
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
});
```
---

## 2. Phòng chống tấn công leo thang đặc quyền (Privilege Escalation Protection)
Khi Admin thay đổi Role của một tài khoản khác lên `admin`, hệ thống kiểm tra số lượng tài khoản Admin hiện hữu trong DB, cấm xóa hoặc hạ quyền của tài khoản Admin gốc (Super Admin) để tránh việc mất quyền quản trị hệ thống hoàn toàn.
