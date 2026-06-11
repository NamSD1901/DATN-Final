# 04. Infrastructure & Security - Admin Staff Management

Tài liệu thiết kế hạ tầng, cơ chế phân quyền (RBAC), phòng chống leo thang đặc quyền (Privilege Escalation) và giải pháp lưu vết hoạt động (Audit Logging).

---

## 1. Phòng chống Leo thang Đặc quyền (Privilege Escalation Prevention)

Lỗi leo thang đặc quyền xảy ra khi người dùng có vai trò thấp hơn (như Bác sĩ, Lễ tân hoặc khách hàng) tìm cách gửi các request trực tiếp đến cổng API quản trị nhân viên để tự thăng chức hoặc đổi quyền của người khác.

### Các biện pháp phòng chống ở tầng API Gateway & Backend:
1. **Kiểm tra JWT Role nghiêm ngặt:** Sử dụng thuộc tính `[Authorize(Roles = "admin")]` ở cấp độ Class Controller. Mọi request không chứa Claim `role` có giá trị `admin` đều bị chặn ngay lập tức tại Middleware Authorization và trả về mã lỗi `403 Forbidden` trước khi chạm tới tầng Business Logic.
2. **Kiểm tra vai trò đích:** Khi Admin tạo tài khoản hoặc đổi vai trò của người dùng khác, Backend không lấy thông tin vai trò từ Client gửi lên mà tự động đối soát chéo với các Role định nghĩa sẵn trong hệ thống.
3. **Mã hóa Token an toàn:** Khóa bí mật dùng để ký token JWT (HMAC-SHA256) được lưu trữ an toàn trong biến môi trường của hệ thống sản xuất (Production Environment Variables) hoặc Azure Key Vault, ngăn chặn hành vi giả mạo token cục bộ để tự gán quyền admin.

---

## 2. Hệ thống Nhật ký Kiểm toán (Security Audit Logging)

Mọi thao tác quản lý nhân sự nhạy cảm của Admin bắt buộc phải được lưu vết tự động vào cơ sở dữ liệu để làm căn cứ đối soát bảo mật. Nhật ký kiểm toán là tài sản chỉ-đọc (`Read-Only`), không được phép chỉnh sửa hoặc xóa bởi bất kỳ người dùng nào (kể cả các Admin khác).

### Cấu trúc Lưu trữ Audit Logs trong C#:
```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid ActorId { get; set; } // ID người thực hiện (Admin)
    public string ActionName { get; set; } = null!; // Tên hành động: CREATE_STAFF, CHANGE_ROLE, SUSPEND
    public Guid TargetUserId { get; set; } // Nhân viên chịu tác động
    public string Description { get; set; } = null!; // Mô tả chi tiết
    public DateTime Timestamp { get; set; } // Thời gian thực thi
    public string? IpAddress { get; set; } // Địa chỉ IP gửi request
}
```

### Cơ chế tự động ghi nhận nhật ký qua EF Core Interceptor:
Hệ thống sử dụng một Service trung gian để tự động lưu vết mỗi khi gọi API quản trị:

```csharp
public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;

    public AuditLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(Guid actorId, string actionName, Guid targetUserId, string description, string ipAddress)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            ActionName = actionName,
            TargetUserId = targetUserId,
            Description = description,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
```

---

## 3. Rate Limiting Giới hạn Tần suất Yêu cầu

Nhằm ngăn chặn hành vi brute-force dò tìm mật khẩu tài khoản Admin hoặc spam API thêm mới nhân sự:
- **Tạo tài khoản mới:** Giới hạn tối đa **5 requests / phút** trên toàn bộ hệ thống để tránh spam gửi mail hàng loạt.
- **Đổi vai trò & Khóa tài khoản:** Giới hạn tối đa **10 requests / phút** trên một IP Admin.
- **Xem nhật ký Audit Logs:** Giới hạn tối đa **30 requests / phút** trên một tài khoản Admin.

---

## 4. Tích hợp Dịch vụ Email (SMTP Mailer)

Hệ thống sử dụng thư viện **MailKit** để gửi email tự động chứa thông tin tài khoản và mật khẩu tạm thời cho nhân viên mới.
- **Cấu hình SMTP Secure Connection:** Bắt buộc sử dụng giao thức **STARTTLS** (Cổng 587) hoặc **SSL/TLS** (Cổng 465) để mã hóa toàn bộ lưu lượng email truyền đi, ngăn chặn rò rỉ mật khẩu tạm thời trên đường truyền mạng.
