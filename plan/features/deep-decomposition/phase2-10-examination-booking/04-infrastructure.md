# 🌐 Infrastructure & Security - Online Examination Booking

## 1. Phân quyền API và Bảo mật vai trò (RBAC)
Tính năng đặt lịch khám y tế trực tuyến chỉ dành riêng cho phân hệ Khách hàng (Customer):
*   **Ràng buộc Endpoint:** Endpoint `CustomerAppointmentController` bắt buộc cấu hình `[Authorize(Roles = "customer")]`.
*   **Hạn chế quyền hạn:** Nhân viên hay bác sĩ không sử dụng endpoint này (họ sử dụng receptionist/doctor portal riêng để lên lịch walk-in trực tiếp), đảm bảo kiến trúc cách ly bảo mật dữ liệu khách hàng.
*   **Trích xuất UserId:** ID khách hàng được trích xuất hoàn toàn từ token claims của JWT đã qua xác thực ký số, triệt tiêu nguy cơ giả danh ID.

---

## 2. Phòng chống Tấn công IDOR chéo (Pet Ownership Verification)
Một khách hàng độc hại có thể cố gắng đặt lịch hẹn y tế và gán ID thú cưng của một người dùng khác (đoán ID ngẫu nhiên) nhằm xem lén thông tin thú cưng hoặc gán lịch ảo làm rối hệ thống.

*   **Cơ chế phòng thủ:**
    *   Trước khi lưu lịch hẹn y tế, API Controller bắt buộc gọi `_petService.GetPetByIdAsync(dto.PetId, customerId)`.
    *   Hàm này thực thi truy vấn kiểm tra quyền sở hữu trong DB:
        `SELECT Id FROM Pets WHERE Id = @PetId AND OwnerId = @CustomerId`
    *   Nếu trả về null (thú cưng không tồn tại hoặc thuộc người khác), API ngay lập tức dừng xử lý và trả về `400 Bad Request` kèm thông báo *"Thú cưng không hợp lệ hoặc không thuộc về bạn."*.
*   **Chặn IDOR Bác sĩ:** Hệ thống kiểm tra ID bác sĩ yêu cầu (`dto.DoctorId`) để bảo đảm bác sĩ đó đang hoạt động (`IsActive = true`) và thuộc nhóm Role `doctor` trong DB.

---

## 3. Thiết kế Index Cơ sở dữ liệu để chống trùng lịch
Hoạt động kiểm tra trùng lịch khám của Bác sĩ diễn ra liên tục mỗi khi khách hàng chọn giờ khám. Để tránh tình trạng nghẽn cơ sở dữ liệu khi nhiều người dùng thao tác đồng thời:

*   **Index tối ưu:** Thiết lập **Composite Index Filtered** trên bảng `Appointments` để phục vụ nhanh câu lệnh `AnyAsync()`.
*   **SQL DDL Script:**
```sql
CREATE INDEX IX_Appointments_DoubleBookingCheck 
ON "Appointments" ("DoctorId", "AppointmentDate") 
INCLUDE ("Status")
WHERE "Status" != 'cancelled';
```
*   **Ý nghĩa:** CSDL PostgreSQL sẽ lưu trữ chỉ mục các khung giờ của từng bác sĩ điều trị ở trạng thái hoạt động (bỏ qua lịch đã hủy). Khi có request đặt lịch mới, CSDL thực thi truy vấn so khớp phạm vi (Range Search) $\pm30$ phút bằng cơ chế Index Seek nhanh chóng dưới 2ms, giảm thiểu nghẽn I/O đĩa ghi.

---

## 4. Giới hạn tần suất gọi API (Rate Limiting)
Hoạt động đặt lịch hẹn sinh ra luồng ghi cơ sở dữ liệu phức tạp (Transaction) và có thể gây nghẽn phòng khám nếu có hàng loạt lịch ảo được tạo liên tục:

*   **Chính sách Rate Limiting (.NET 8+):**
    *   Tối đa **3 lần đặt lịch / 1 phút / 1 tài khoản**.
    *   Tối đa **20 lần tải danh sách dịch vụ/bác sĩ / 1 phút / 1 tài khoản**.
*   **Mã cấu hình C# Web API:**
```csharp
options.AddFixedWindowLimiter(policyName: "BookingRateLimitPolicy", limitOptions =>
{
    limitOptions.PermitLimit = 3;
    limitOptions.Window = TimeSpan.FromMinutes(1);
    limitOptions.QueueLimit = 0; // Từ chối ngay lập tức
});
```
