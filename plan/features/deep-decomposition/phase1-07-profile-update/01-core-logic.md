# 🧠 Core Business Logic - Profile Details Update

## 1. Xác thực Định danh & Cơ chế phòng chống IDOR (Identity Extraction)
Để ngăn chặn hoàn toàn lỗi bảo mật **IDOR (Insecure Direct Object References)** - hành vi mà tin tặc thay đổi tham số định danh trên URL hoặc payload để xem/sửa hồ sơ của người dùng khác - hệ thống **MyPetClinic** áp dụng nguyên tắc thiết kế bảo mật chặt chẽ:
*   **Không truyền `userId` từ Client:** API không bao giờ tiếp nhận tham số `userId` thông qua tham số truy vấn (Query Parameter) hoặc thân yêu cầu (Request Body) cho các thao tác trên tài khoản cá nhân.
*   **Giải mã từ Token/Cookie:** Khi một request đi qua bộ lọc xác thực `[Authorize]`, ASP.NET Core Middleware sẽ giải mã JWT Token hoặc Session Cookie và điền thông tin vào thuộc tính `User` (kiểu `ClaimsPrincipal`) của Controller.
*   **Hàm trích xuất định danh chuẩn:**
```csharp
private Guid GetUserId()
{
    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (Guid.TryParse(userIdStr, out Guid userId)) return userId;
    throw new UnauthorizedAccessException("User ID không tồn tại trong token/cookie xác thực.");
}
```

---

## 2. Logic xử lý tại Tầng Application Service

Lớp `UserService` chịu trách nhiệm điều phối luồng nghiệp vụ. Khi có yêu cầu cập nhật hồ sơ, các bước xử lý bao gồm:

1.  **Lấy thực thể từ Database:** Truy vấn thông qua Repository để lấy thông tin Entity của người dùng hiện tại kèm theo trạng thái khóa ngoại (nếu có).
2.  **Ánh xạ (Mapping) dữ liệu:** Cập nhật các trường thông tin cho phép sửa đổi (`FullName`, `Phone`, `Address`, `Gender`, `DateOfBirth`).
3.  **Xử lý ngày múi giờ cho PostgreSQL:** Cơ sở dữ liệu PostgreSQL (qua thư viện Npgsql) yêu cầu kiểu dữ liệu `timestamp with time zone` (timestamptz) phải được cung cấp dưới dạng ngày UTC (`DateTimeKind.Utc`). Việc không thiết lập múi giờ sẽ gây lỗi runtime hệ thống.
4.  **Lưu thay đổi:** Đánh dấu Entity là Modified và gọi SaveChanges thông qua Unit of Work/Repository.

### C# Code Reference: `UserService.cs` (Trích đoạn xử lý Profile)
```csharp
public async Task<bool> UpdateUserProfileAsync(Guid userId, UpdateProfileDto dto)
{
    // 1. Tìm kiếm User theo ID giải mã từ Token
    var user = await _userRepository.GetUserByIdAsync(userId);
    if (user == null) 
    {
        // Trả về false nếu tài khoản không tồn tại hoặc đã bị khóa/xóa mềm
        return false; 
    }

    // 2. Ghi đè dữ liệu mới từ DTO lên Entity
    user.FullName = dto.FullName;
    user.Phone = dto.Phone;
    user.Address = dto.Address;
    user.Gender = dto.Gender;
    
    // 3. Khắc phục lỗi Npgsql DateTime bằng cách ép kiểu UTC
    if (dto.DateOfBirth.HasValue)
    {
        user.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc);
    }
    else
    {
        user.DateOfBirth = null;
    }

    // 4. Lưu thay đổi xuống cơ sở dữ liệu
    await _userRepository.UpdateUserAsync(user);
    await _userRepository.SaveChangesAsync();

    return true;
}
```

---

## 3. Quy trình Kiểm tra nghiệp vụ (Business Rules Validation)

### A. Ràng buộc trường Ngày sinh (Date of Birth)
*   **Quy tắc:** Người dùng không thể sinh ra ở tương lai. Ngày sinh nhập vào phải nhỏ hơn ngày hiện tại (`DateOfBirth < DateTime.UtcNow`).
*   **Độ tuổi tối đa:** Không chấp nhận người dùng nhập năm sinh quá xa trong quá khứ (ví dụ lớn hơn 100 tuổi) nhằm tránh dữ liệu rác.

### B. Ràng buộc trường Họ tên (FullName)
*   **Quy tắc:** Bắt buộc nhập. Độ dài từ 2 đến 100 ký tự.
*   **Ký tự đặc biệt:** Hệ thống chỉ cho phép các ký tự chữ cái Tiếng Việt hợp lệ và dấu khoảng trắng. Tránh các chuỗi script độc hại (XSS prevention) hoặc các ký tự toán học lạ.

### C. Ràng buộc số điện thoại (Phone)
*   **Đầu số nhà mạng:** Phải bắt đầu bằng các đầu số di động chính quy tại Việt Nam: Viettel (03, 098...), Mobifone (07, 090...), VinaPhone (08, 091...), Vietnamobile (05...).
*   **Độ dài:** Đúng 10 chữ số.

---

## 4. Xử lý đồng thời (Concurrency Handling)
*   Do chỉ có duy nhất chính chủ tài khoản mới được quyền sửa đổi hồ sơ cá nhân của mình, xác suất xảy ra xung đột đồng thời (Concurrency Conflict) là cực kỳ thấp. Tuy nhiên, hệ thống vẫn áp dụng cơ chế SaveChanges của Entity Framework Core để theo dõi trạng thái thay đổi của các cột dữ liệu cụ thể và cập nhật đúng các trường đã thay đổi.
