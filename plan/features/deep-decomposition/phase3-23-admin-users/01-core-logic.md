# 🧠 Core Business Logic - Admin Staff Management

## 🔗 Skills Liên Quan
- **BE-F01 (C# Fundamentals):** Kiểm duyệt tránh lỗi logic (ví dụ: Chặn Admin hiện tại tự gửi request khóa tài khoản của chính mình).
- **BE-F03 (Async/Await):** Sử dụng các thư viện tích hợp Identity (`UserManager<ApplicationUser>`) bất đồng bộ.

---

## 1. C# Logic: Xử lý quản trị nhân sự an toàn (Identity Staff Service)

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class AdminStaffService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminStaffService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> LockUserAccountAsync(Guid targetUserId, Guid currentAdminUserId)
    {
        // Chặn Admin tự khóa tài khoản chính mình
        if (targetUserId == currentAdminUserId)
        {
            return (false, "Bạn không thể tự khóa tài khoản của chính mình.");
        }

        var user = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (user == null)
        {
            return (false, "Người dùng không tồn tại.");
        }

        // Khóa tài khoản vĩnh viễn (thiết lập lockout end đến tương lai xa)
        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!result.Succeeded)
        {
            return (false, "Không thể thiết lập trạng thái khóa tài khoản.");
        }

        return (true, null);
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> UnlockUserAccountAsync(Guid targetUserId)
    {
        var user = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (user == null)
        {
            return (false, "Người dùng không tồn tại.");
        }

        // Mở khóa tài khoản (thiết lập lockout end về null hoặc thời gian trong quá khứ)
        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        if (!result.Succeeded)
        {
            return (false, "Không thể mở khóa tài khoản.");
        }

        return (true, null);
    }
}
```
