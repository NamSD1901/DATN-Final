# 🛠️ Technical Specification - Profile Avatar Upload

## 1. Cơ chế tải tệp nhị phân (Upload Sequence Diagram)

Sơ đồ dưới đây đặc tả luồng gửi tệp nhị phân định dạng `multipart/form-data` từ Vue 3 Client lên .NET Web API, lưu trữ vật lý trên Server và cập nhật cơ sở dữ liệu:

```mermaid
sequenceDiagram
    autonumber
    actor User as Client (Vue 3)
    participant API as WebApi (ProfileController)
    participant Auth as JwtMiddleware / Authorize
    participant OS as Server OS File System
    participant Service as UserService
    participant Repo as UserRepository
    participant DB as PostgreSQL Database

    User->>API: POST /api/profile/avatar (multipart/form-data with avatarFile)
    Note right of User: Gửi tệp nhị phân kèm JWT Token
    API->>Auth: Xác thực token & giải mã Claims
    alt Token không hợp lệ
        Auth-->>User: 401 Unauthorized
    else Token hợp lệ
        Auth-->>API: Trích xuất NameIdentifier (userId)
        API->>API: Kiểm tra tệp tin (avatarFile != null và length > 0)
        alt Tệp tin trống rỗng
            API-->>User: 400 Bad Request ("Vui lòng chọn một file ảnh hợp lệ.")
        else Tệp tin hợp lệ
            API->>API: Kiểm tra định dạng đuôi (.jpg, .jpeg, .png, .gif)
            alt Đuôi tệp không hợp lệ
                API-->>User: 400 Bad Request ("Chỉ chấp nhận các file ảnh...")
            else Đuôi hợp lệ
                API->>API: Kiểm tra kích thước (Length > 2 * 1024 * 1024)
                alt Kích thước > 2MB
                    API-->>User: 400 Bad Request ("Kích thước ảnh không được vượt quá 2MB.")
                else Kích thước hợp lệ
                    API->>API: Sinh tên tệp duy nhất: {Guid}_{FileName}
                    API->>OS: Tạo thư mục wwwroot/uploads/avatars nếu chưa có
                    API->>OS: Ghi luồng dữ liệu (FileStream.CopyToAsync)
                    OS-->>API: Lưu file vật lý thành công
                    API->>Service: UpdateAvatarAsync(userId, "/uploads/avatars/{UniqueName}")
                    Service->>Repo: GetUserByIdAsync(userId)
                    Repo-->>Service: User Entity
                    Service->>Service: Cập nhật User.Avatar = avatarUrl
                    Service->>Repo: UpdateUserAsync(user)
                    Repo->>DB: UPDATE Users SET Avatar = @AvatarUrl WHERE Id = @userId
                    DB-->>Repo: Row affected
                    Repo-->>Service: Success (true)
                    Service-->>API: Success (true)
                    API-->>User: 200 OK (JSON success, avatarUrl)
                end
            end
        end
    end
```

---

## 2. Đặc tả Cấu trúc Lưu trữ & Hệ thống Tệp tin (Filesystem Structure)

*   **Đường dẫn thư mục gốc (Root path):** `wwwroot/uploads/avatars/`. Thư mục này nằm trong thư mục gốc của Web API để IIS/Kestrel có thể trực tiếp phục vụ tệp tĩnh (Static Files Serving).
*   **Quy tắc đặt tên tệp tin:**
    *   Tên tệp tin tải lên từ máy người dùng có thể trùng lặp hoặc chứa các ký tự đặc biệt gây lỗi đường dẫn URL. Do đó, hệ thống bắt buộc đặt lại tên theo công thức:
        `string uniqueFileName = $"{Guid.NewGuid()}_{avatarFile.FileName}";`
    *   Ví dụ: `a3b2c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d_my-cat.png`
*   **Xử lý thư mục chưa tồn tại:** Đoạn mã bắt buộc kiểm tra sự tồn tại của thư mục lưu trữ trước khi ghi file nhằm tránh lỗi thư mục ảo:
    ```csharp
    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
    if (!Directory.Exists(uploadsFolder))
    {
        Directory.CreateDirectory(uploadsFolder);
    }
    ```

---

## 3. Đặc tả Cổng API & Ràng buộc Input (API Contract & Inputs Validation)

### A. Phương thức giao tiếp (HTTP Method & Route)
*   **HTTP Method:** `POST`
*   **Endpoint Route:** `/api/profile/avatar`
*   **Content-Type:** `multipart/form-data`
*   **Request Parameter:**
    *   `avatarFile` (Kiểu dữ liệu: `IFormFile`, bắt buộc gửi dạng Binary).

### B. Validation tại API Controller
Đoạn mã xử lý kiểm tra đầu vào nghiêm ngặt trước khi tương tác hệ thống:
```csharp
// Kiểm tra đuôi file mở rộng (White-list Extension)
var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
if (!allowedExtensions.Contains(extension))
{
    return BadRequest(new { message = "Chỉ chấp nhận các file ảnh định dạng: .jpg, .jpeg, .png, .gif" });
}

// Giới hạn kích thước tối đa 2MB
if (avatarFile.Length > 2 * 1024 * 1024)
{
    return BadRequest(new { message = "Kích thước ảnh không được vượt quá 2MB." });
}
```
> [!IMPORTANT]
> Toàn bộ quá trình ghi tệp tin phải được đặt trong khối `try-catch` và sử dụng lệnh `using` đối với `FileStream` để giải phóng con trỏ tệp tin ngay sau khi ghi xong, tránh gây khóa tệp (file lock) hoặc rò rỉ tài nguyên bộ nhớ hệ thống.
