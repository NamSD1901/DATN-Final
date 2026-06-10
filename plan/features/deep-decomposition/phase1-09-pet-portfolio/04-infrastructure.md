# 🌐 Infrastructure & Security - Pet Portfolio Management

## 1. Phân quyền vai trò và Xác thực (Access Control & RBAC)

Hồ sơ thú cưng là thông tin riêng tư của chủ nuôi. Do đó, hạ tầng Web API thiết lập các rào chắn truy cập nghiêm ngặt:

*   **Ràng buộc vai trò (Role Restrict):**
    *   Endpoint `MyPetsController` được cấu hình thuộc tính `[Authorize(Roles = "customer")]`.
    *   Chỉ những tài khoản được phân vai trò (Role) là **Customer (Khách hàng)** mới được phép gọi các API CRUD thú cưng cá nhân này.
    *   Nhân viên phòng khám (Lễ tân, Bác sĩ) sử dụng các API chuyên biệt khác (trong các Phase sau) để truy cập thông tin thú cưng của khách hàng, đảm bảo nguyên tắc đặc quyền tối thiểu (Least Privilege).
*   **Token Claims Verification:** Mọi request bắt buộc gửi kèm JWT hợp lệ để trích xuất `ClaimTypes.NameIdentifier` (UserId).

---

## 2. Phòng chống Tấn công Bảo mật IDOR (Insecure Direct Object Reference)

*   **Rủi ro:** Kẻ tấn công sử dụng tài khoản Khách hàng A, đoán số ID thú cưng tự tăng (ví dụ: `12`, `13`) và gửi request `GET /api/mypets/13` hoặc `DELETE /api/mypets/13` để xem lén hoặc xóa phá hoại thú cưng của Khách hàng B.
*   **Giải pháp hạ tầng & mã nguồn đối phó:**
    1.  **Không tin tưởng ID đầu vào:** Mọi API thao tác trên một thú cưng cụ thể (`GET`, `PUT`, `DELETE` với `{id}`) đều yêu cầu truyền kèm tham số `Guid ownerId` giải mã từ Token JWT.
    2.  **Truy vấn kèm điều kiện chủ sở hữu:** Trước khi thực thi bất kỳ thay đổi nào xuống Database, lớp Service phải truy vấn kiểm tra quyền sở hữu:
        ```csharp
        var pet = await _petRepository.GetPetByIdAsync(id);
        if (pet == null || pet.OwnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Không tìm thấy thú cưng hoặc bạn không có quyền thao tác.");
        }
        ```
    3.  **Che giấu thông tin lỗi nhạy cảm:** Nếu phát hiện hành vi truy cập sai chủ sở hữu (IDOR), API có thể trả về `404 Not Found` hoặc `403 Forbidden` thay vì thông báo lỗi chi tiết, nhằm tránh việc tin tặc dò quét sự tồn tại của ID thú cưng trong DB (User Enumeration Prevention).

---

## 3. Tối ưu hóa Database (PostgreSQL Indexing)

Khi phòng khám mở rộng quy mô với hàng ngàn khách hàng và thú cưng, câu lệnh truy xuất danh sách thú cưng của một khách hàng có thể bị chậm đi nếu quét toàn bộ bảng (Table Scan).

*   **Giải pháp tối ưu hóa:** Tạo **Non-Clustered Index** trên cột ngoại khóa `OwnerId` và cờ trạng thái `IsDeleted` trong bảng `Pets`.
*   **Câu lệnh SQL khởi chạy Index:**
```sql
CREATE INDEX IX_Pets_OwnerId_IsDeleted 
ON "Pets" ("OwnerId", "IsDeleted")
INCLUDE ("Name", "Species", "Breed");
```
*   **Lợi ích:** Cơ sở dữ liệu PostgreSQL sẽ tìm kiếm trực tiếp bằng cây chỉ mục (Index Seek) cực nhanh, giảm thời gian thực thi truy vấn xuống dưới 10ms đối với dữ liệu lớn.

---

## 4. Rate Limiting cho API CRUD Thú Cưng

Để ngăn chặn việc tạo tài khoản ảo và spam tạo hàng loạt hồ sơ thú cưng làm rác database, hệ thống áp dụng giới hạn tần suất gọi API:

*   **POST /api/mypets (Thêm mới):** Tối đa 5 lần tạo / phút cho mỗi User ID.
*   **GET /api/mypets (Xem danh sách):** Tối đa 60 lần / phút cho mỗi User ID.
*   **Chính sách Rate Limiting (.NET 8+):**
```csharp
options.AddFixedWindowLimiter(policyName: "PetCrudPolicy", limitOptions =>
{
    limitOptions.PermitLimit = 15; // Trung bình tối đa 15 request hỗn hợp
    limitOptions.Window = TimeSpan.FromMinutes(1);
    limitOptions.QueueLimit = 1;
});
```
