# 🌐 Infrastructure & Security - Profile Avatar Upload

## 1. Cấu hình Hạ tầng Máy chủ & Tệp tĩnh (Static Files Middleware)

Để ứng dụng Client (Vue SPA) hoặc người dùng bên ngoài có thể truy cập trực tiếp các tệp hình ảnh đại diện đã tải lên thông qua đường dẫn URL tương đối (ví dụ: `https://localhost:5001/uploads/avatars/abc-123.jpg`), hạ tầng máy chủ ASP.NET Core cần được thiết lập đúng cơ chế phục vụ tệp tĩnh:

*   **Middleware kích hoạt:** Bắt buộc đăng ký `app.UseStaticFiles()` trong luồng HTTP Pipeline của `Program.cs`.
*   **Phân quyền Thư mục (File Permissions):**
    *   Thư mục vật lý `wwwroot/uploads/avatars` trên hệ điều hành máy chủ phải được cấp quyền **Write (Ghi)** và **Read (Đọc)** cho tài khoản chạy tiến trình Web API (ví dụ: `IIS_IUSRS` trên Windows Server/IIS hoặc tài khoản dịch vụ `www-data` trên Linux/Nginx).
    *   Không cấp quyền **Execute (Thực thi)** trên thư mục `uploads` để chặn đứng nguy cơ kẻ tấn công tải lên file mã độc rồi kích hoạt trực tiếp từ trình duyệt.

---

## 2. Các Rào cản Bảo mật và Chính sách Bảo vệ Máy chủ (Upload Security Policy)

Tải tệp tin là con đường ngắn nhất để hacker tấn công chiếm quyền điều khiển máy chủ (Remote Code Execution - RCE). Do đó, hệ thống **MyPetClinic** áp dụng 4 lớp phòng vệ vững chắc:

### A. Giới hạn Kích thước Tệp tối đa tại Kestrel / IIS
*   Mặc dù API Controller đã kiểm tra `.Length > 2MB`, kẻ tấn công vẫn có thể gửi một file nặng 500MB lên làm nghẽn băng thông hệ thống trước khi Controller kịp xử lý.
*   **Giải pháp:** Đặt cấu hình giới hạn kích thước tối đa cho toàn hệ thống ở tầng Middleware.
    *   **Kestrel server config (C#):**
        ```csharp
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 2 * 1024 * 1024; // 2MB toàn hệ thống cho request
        });
        ```
    *   **IIS Web.config settings:**
        ```xml
        <system.webServer>
          <security>
            <requestFiltering>
              <!-- 2MB = 2097152 Bytes -->
              <requestLimits maxAllowedContentLength="2097152" />
            </requestFiltering>
          </security>
        </system.webServer>
        ```

### B. Ngăn chặn Path Traversal bằng UUID Renaming
*   Hệ thống loại bỏ hoàn toàn tên file do Client gửi lên để sinh tên ngẫu nhiên:
    `string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);`
*   Việc này cũng triệt tiêu nguy cơ trùng lặp tên tệp giữa các người dùng khác nhau và ngăn chặn lỗi hiển thị Unicode khi lưu file trên hệ điều hành Linux/ext4.

### C. Danh sách Định dạng được Phép (White-list Validation)
*   Hệ thống chỉ cho phép tải lên các định dạng ảnh an toàn: `.jpg`, `.jpeg`, `.png`, `.gif`.
*   Tất cả các định dạng khả nghi như `.svg` (có thể chứa mã độc XML/XSS), `.html`, `.exe`, `.dll`, `.bat` đều bị từ chối thẳng.

---

## 3. Chính sách giới hạn tần suất API (Rate Limiting)

Việc tải file ngốn băng thông mạng và hiệu năng ổ đĩa I/O của máy chủ. Cần giới hạn tần suất tải ảnh để chống brute-force làm sập máy chủ lưu trữ.

*   **Chính sách Rate Limiting:**
    *   Tối đa **5 lần tải ảnh đại diện / phút** cho mỗi tài khoản đã đăng nhập.
    *   Nếu vượt quá hạn mức, hệ thống trả về HTTP Status `429 Too Many Requests`.
    *   Hàng đợi (Queue) cấu hình bằng 0 để từ chối ngay lập tức các request spam.

```csharp
options.AddFixedWindowLimiter(policyName: "AvatarUploadPolicy", limitOptions =>
{
    limitOptions.PermitLimit = 5;
    limitOptions.Window = TimeSpan.FromMinutes(1);
    limitOptions.QueueLimit = 0;
});
```
