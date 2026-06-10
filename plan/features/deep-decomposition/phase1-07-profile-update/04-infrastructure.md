# 🌐 Infrastructure & Security - Profile Details Update

## 1. Cơ chế bảo mật và Ủy quyền (Authentication & Authorization)
Mọi tài nguyên và hành động trong phân hệ quản lý hồ sơ cá nhân đều thuộc phạm vi bảo vệ nghiêm ngặt của hệ thống xác thực.

### A. Middleware Xác thực JWT
*   Tất cả các endpoint trong `ProfileController` được khai báo bộ lọc `[Authorize]`.
*   Yêu cầu Client phải cung cấp một chuỗi JWT (JSON Web Token) hợp lệ trong Header: `Authorization: Bearer <JWT_TOKEN>` hoặc thông qua Secure HttpOnly Cookie `X-Access-Token` (tùy thuộc vào cấu hình của từng phân hệ).
*   Nếu Token không hợp lệ, bị sửa đổi chữ ký, hoặc hết hạn, ASP.NET Core Authentication Middleware sẽ tự động phản hồi mã trạng thái `401 Unauthorized` tại tầng Kestrel mà không chuyển request vào Pipeline xử lý của Controller, giúp bảo vệ tài nguyên hệ thống tối đa.

### B. Chống tấn công IDOR tuyệt đối
*   **IDOR (Insecure Direct Object Reference):** Lỗi bảo mật xảy ra khi hệ thống cho phép tin tặc sửa đổi giá trị định danh người dùng trong URL hoặc Request Body để truy cập/sửa đổi trái phép dữ liệu của người khác (Ví dụ: `PUT /api/profile?userId=999`).
*   **Giải pháp xử lý:**
    1.  Không sử dụng tham số ID đầu vào từ Client.
    2.  Hệ thống trích xuất định danh trực tiếp từ trường `ClaimTypes.NameIdentifier` nằm trong Payload được ký điện tử bảo mật của JWT. Vì Client không thể giả mạo chữ ký của JWT (chỉ Server có Private Key mới ký được), User ID trích xuất ra là hoàn toàn tin cậy và chính xác là của tài khoản đang đăng nhập.
    3.  Thực hiện đối chiếu logic nội bộ tại Repository để chỉ cập nhật bản ghi khớp với ID này.

---

## 2. Giới hạn tần suất gọi API (Rate Limiting)

Để ngăn chặn việc người dùng cố tình viết script tự động gửi hàng nghìn request cập nhật liên tục làm cạn kiệt tài nguyên cơ sở dữ liệu (tấn công từ chối dịch vụ DoS/DDoS), hệ thống áp dụng chính sách giới hạn tần suất cụ thể cho Endpoint Profile:

*   **Chính sách giới hạn (Rate Limit Policy):** Sử dụng thư viện `Microsoft.AspNetCore.RateLimiting` được tích hợp sẵn trong .NET 8+.
*   **Thuật toán áp dụng:** Thuật toán **Fixed Window** (Cửa sổ cố định).
*   **Cấu hình chi tiết:**
    *   **GET /api/profile (Đọc thông tin):** Tối đa 60 request trong vòng 1 phút cho mỗi địa chỉ IP / User ID. Nếu vượt quá, trả về mã lỗi `429 Too Many Requests`.
    *   **PUT /api/profile (Cập nhật thông tin):** Tối đa 10 request trong vòng 1 phút cho mỗi User ID. Đây là hoạt động ghi xuống Database (Write-heavy), cần giới hạn chặt hơn để bảo vệ đĩa ghi. Nếu vượt quá, trả về mã lỗi `429 Too Many Requests`.

### Mã nguồn cấu hình C# Web API (`Program.cs`):
```csharp
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "ProfileUpdatePolicy", limitOptions =>
    {
        limitOptions.PermitLimit = 10; // Tối đa 10 lượt cập nhật
        limitOptions.Window = TimeSpan.FromMinutes(1); // Trong vòng 1 phút
        limitOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limitOptions.QueueLimit = 2; // Hàng đợi tối đa 2 request chờ xử lý
    });
});
```

---

## 3. Bảo vệ dữ liệu cá nhân & Tuân thủ GDPR (Data Privacy)

Hồ sơ cá nhân chứa các dữ liệu định danh trực tiếp (PII - Personally Identifiable Information) như Họ tên, Số điện thoại, Ngày sinh và Địa chỉ thường trú. Do đó, việc lưu trữ và vận hành hạ tầng tuân thủ các quy tắc bảo mật thông tin khách hàng:

*   **HTTPS Only:** Ép buộc sử dụng kết nối mã hóa TLS 1.3 trên toàn bộ các kênh truyền thông. Mọi request HTTP thông thường sẽ tự động chuyển hướng (Redirect 301) sang HTTPS.
*   **Lọc trường nhạy cảm trong Log:** Toàn bộ hệ thống Log (như Serilog hoặc NLog) phải được cấu hình bộ lọc để ẩn hoặc mã hóa các thông tin nhạy cảm như `Phone`, `Address` trong console log/tệp log để tránh rò rỉ khi quản trị viên hệ thống truy cập file log.
*   **Xóa mềm (Soft Delete):** Khi người dùng yêu cầu xóa tài khoản, hệ thống không xóa vật lý bản ghi ngay lập tức mà cập nhật cột `DeletedAt = DateTime.UtcNow` và ẩn toàn bộ thông tin cá nhân khỏi các danh sách hoạt động, đồng thời mã hóa một phần số điện thoại trong DB (ví dụ: `098***1234`) để giải phóng số điện thoại cho người dùng mới đăng ký nhưng vẫn giữ lịch sử hóa đơn kế toán phòng khám.
