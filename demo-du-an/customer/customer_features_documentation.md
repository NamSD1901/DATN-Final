# Hướng Dẫn Chi Tiết Các Chức Năng Dành Cho Khách Hàng (Customer Features)

Tài liệu này được soạn thảo để giúp các thành viên trong nhóm hiểu rõ luồng nghiệp vụ, logic xử lý và mã nguồn liên quan đến các chức năng dành cho Khách Hàng (Customer) trong hệ thống MyPetClinic.

---

## 1. Tổng Quan Kiến Trúc (Clean Architecture)

Các chức năng của khách hàng được phân tách rõ ràng theo mô hình Clean Architecture:
- **Domain Entities**: `Customer`, `Pet`, `Appointment`, v.v. (Nằm trong thư mục `MyPetClinic.Domain/Entities`)
- **Application Services**: Chứa toàn bộ logic nghiệp vụ (Nằm trong thư mục `MyPetClinic.Application/Services`)
- **DTOs (Data Transfer Objects)**: Dùng để giao tiếp giữa API (Controller) và Service.

---

## 2. Chi Tiết Các Chức Năng Chính

### A. Quản Lý Hồ Sơ Khách Hàng (Customer Management)
**File xử lý chính:** [CustomerService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/CustomerService.cs)

> [!NOTE]
> Chức năng này bao gồm việc tìm kiếm, xem danh sách, lấy thông tin chi tiết, tạo mới khách hàng (cùng với thú cưng) và xóa mềm (Soft Delete).

**Các nghiệp vụ quan trọng:**
1. **Tạo mới khách hàng kèm thú cưng (`CreateCustomerWithPetsAsync`)**:
   - Hệ thống tự động tạo `CustomerCode` theo định dạng `CUS + yyMMddHHmmss`.
   - Kiểm tra tính duy nhất của Email và Số điện thoại trước khi lưu để tránh trùng lặp dữ liệu.
   - Nếu khách hàng có khai báo thông tin thú cưng ngay lúc đăng ký, hệ thống sẽ lưu luôn danh sách thú cưng đó vào bảng `Pets` cùng lúc (Transactional).
   
2. **Xóa mềm (`SoftDeleteCustomerAsync`)**:
   - Khi xóa, hệ thống không xóa dữ liệu thật khỏi Database (Hard Delete) mà chỉ cập nhật cờ `DeletedAt = DateTime.UtcNow` và `Status = "Inactive"`. Điều này giúp giữ toàn vẹn dữ liệu lịch sử hóa đơn, khám bệnh.

### B. Quản Lý Thú Cưng (Pet Management)
**File xử lý chính:** [PetService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/PetService.cs)

> [!TIP]
> Khách hàng có thể có nhiều thú cưng. Mọi thao tác cập nhật/xóa thú cưng đều phải kiểm tra quyền sở hữu (`CustomerId`) để chống lỗ hổng bảo mật IDOR.

**Các nghiệp vụ quan trọng:**
1. **Bảo mật IDOR trong Update/Delete**:
   - Trong hàm `UpdatePetAsync` và `DeletePetAsync`, hệ thống luôn kiểm tra `if (pet == null || pet.CustomerId != CustomerId)` để đảm bảo user chỉ được phép sửa/xóa thú cưng của chính mình.
2. **Cập nhật trạng thái đặc biệt (`UpdatePetStatusAsync`)**:
   - Nếu thú cưng được đánh dấu là **đã mất (IsDeceased = true)**, hệ thống sẽ TỰ ĐỘNG tìm tất cả các lịch hẹn trong tương lai (trạng thái `pending` hoặc `confirmed`) của thú cưng này và chuyển sang trạng thái `cancelled`.

### C. Đặt Lịch Hẹn Trực Tuyến (Customer Booking)
**File xử lý chính:** [CustomerAppointmentService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/CustomerAppointmentService.cs)

> [!IMPORTANT]
> Đây là chức năng cốt lõi và có nhiều Business Rules (quy tắc nghiệp vụ) nhất để đảm bảo phòng khám hoạt động trơn tru.

**Các quy tắc ràng buộc khi đặt lịch (`BookAppointmentAsync`):**

1. **Khẩn cấp (Emergency):** Nếu user chọn là ca cấp cứu (`IsEmergency = true`), hệ thống sẽ chặn không cho đặt online và yêu cầu mang đến phòng khám ngay.
2. **Số điện thoại:** Bắt buộc khách hàng phải có số điện thoại để phòng khám liên hệ xác nhận.
3. **Thời gian tối thiểu (Lead Time):** Phải đặt trước ít nhất 15 phút so với giờ hiện tại.
4. **Giờ hoạt động:**
   - Chỉ cho phép đặt trong giờ hành chính: **08:00 - 20:00**.
   - Không cho phép đặt vào giờ nghỉ trưa: **12:00 - 13:30**.
5. **Giới hạn uy tín (No-show Limit):** 
   - Nếu trong 30 ngày qua, khách hàng "Bùng kèo" (trạng thái `no_show`) >= 3 lần, hệ thống sẽ **khóa chức năng đặt lịch online** của khách hàng đó.
6. **Chống Spam (Spam Booking Check):** 
   - Không cho phép đặt 2 lịch hẹn cho *cùng 1 thú cưng* cách nhau dưới 2 tiếng đồng hồ trong cùng 1 ngày.
7. **Phân công Bác sĩ Tự động:**
   - Trong quá trình đặt, khách hàng không tự chọn bác sĩ (`DoctorId = Guid.Empty`). Hệ thống/Lễ tân sẽ tự động phân bổ bác sĩ sau dựa trên dịch vụ yêu cầu.

---

## 3. Khuyến Nghị Lập Trình (Best Practices Cho Team)

> [!WARNING]
> Khi thêm mới hoặc sửa đổi code liên quan đến Khách Hàng, các bạn cần chú ý:

- **Bảo mật:** Luôn lấy `customerId` từ `currentUserId` (được giải mã từ JWT Token) thay vì nhận từ request payload (Body/Query) do client gửi lên.
- **Tối ưu LINQ:** Sử dụng `FindWithIncludesAsync` (Eager Loading) khi cần lấy thông tin Khách hàng kèm Account thay vì gọi Database nhiều lần.
- **Validation:** Bất kỳ quy tắc nghiệp vụ (Business Rule) nào cũng phải đặt ở Application Service (như trong `CustomerAppointmentService.cs`) hoặc Domain, KHÔNG đặt logic này ở UI (Frontend) hay API Controller.

---
*Tài liệu này dùng để đào tạo (Onboarding) và tham chiếu kỹ thuật cho team MyPetClinic.*
