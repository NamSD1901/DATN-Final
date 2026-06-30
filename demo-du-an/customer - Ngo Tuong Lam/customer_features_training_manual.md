# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG KHÁCH HÀNG (CUSTOMER FEATURES)

> [!NOTE]
> Tài liệu này được biên soạn bởi Senior Solution Architect, dành riêng cho các thành viên mới gia nhập đội ngũ phát triển MyPetClinic. Mục tiêu là giúp bạn hiểu sâu về Business Logic, kiến trúc Clean Architecture, và luồng dữ liệu của phân hệ Khách Hàng.

---

## 1. Tổng quan chức năng

- **Tên chức năng:** Phân hệ Khách hàng (Customer Portal). Gồm 3 nhóm chính: Quản lý Hồ sơ (Profile), Quản lý Thú cưng (MyPets) và Đặt lịch hẹn (Booking).
- **Mục đích:** Cho phép khách hàng tự quản lý thông tin cá nhân, danh sách thú cưng, xem lịch sử khám chữa bệnh và chủ động đặt lịch hẹn trực tuyến với phòng khám.
- **Người sử dụng:** Khách hàng (End-user) có tài khoản với role `customer`.
- **Vai trò trong toàn hệ thống:** Đây là điểm chạm (touch-point) đầu tiên của khách hàng với hệ thống. Phân hệ này giảm tải khối lượng công việc cho lễ tân (tự động hóa đặt lịch) và tăng trải nghiệm người dùng.
- **Chức năng giải quyết vấn đề gì:** Giải quyết vấn đề khách hàng phải gọi điện/đến trực tiếp để đặt lịch; khó khăn trong việc theo dõi lịch sử bệnh án/tiêm phòng của thú cưng; dễ quên lịch hẹn.

---

## 2. Quy trình nghiệp vụ (Business Flow)

### Luồng 1: Cập nhật hồ sơ & Thêm thú cưng
Khách hàng đăng nhập thành công
↓
Vào trang Hồ sơ cá nhân (Profile)
↓
Cập nhật số điện thoại & thông tin (Bắt buộc phải có SĐT mới được đặt lịch)
↓
Vào trang Thú cưng (My Pets) -> Thêm thú cưng mới
↓
Lưu thông tin thú cưng vào hệ thống
↓
Hoàn thành.

### Luồng 2: Đặt lịch hẹn trực tuyến
Khách hàng chọn chức năng Đặt lịch (Book Appointment)
↓
Chọn thú cưng (từ danh sách MyPets)
↓
Chọn dịch vụ (Khám bệnh, Tiêm phòng, Spa...)
↓
(Nhánh rẽ) Nếu chọn Tiêm phòng -> Hệ thống kiểm tra phác đồ vắc-xin (Validate Vaccine Interval)
↓
Chọn ngày và giờ (Dựa trên Available Slots)
↓
Kiểm tra điều kiện:
- Không phải cấp cứu.
- Cách hiện tại ít nhất 15 phút.
- Nằm trong giờ hành chính (08:00 - 20:00), trừ giờ nghỉ trưa (12:00 - 13:30).
- Không vi phạm luật "Bùng kèo" (No-show < 3 lần trong 30 ngày).
- Không spam (Không đặt 2 lịch cho 1 pet sát nhau dưới 2h).
↓
Ghi nhận Booking vào Database (Trạng thái Pending)
↓
Hoàn thành.

---

## 3. Luồng xử lý trong source code

Thứ tự thực thi tiêu chuẩn áp dụng trong toàn bộ phân hệ:

View (Vue.js / Client)
↓
Call API (HTTP Request với JWT Token)
↓
**Controller** (WebApi): `ProfileController`, `MyPetsController`, `CustomerAppointmentController` (Kiểm tra Model State, Auth)
↓
**Service** (Application): `UserService`, `PetService`, `CustomerAppointmentService` (Xử lý Business Logic, Validation)
↓
**UnitOfWork / Repository** (Application/Infrastructure): `_unitOfWork.Customers`, `_unitOfWork.Pets`, `_unitOfWork.Appointments`
↓
**DbContext** (Infrastructure): EF Core dịch LINQ sang SQL
↓
**Database** (SQL Server): Thực thi câu lệnh
↓
Trả kết quả ngược lại theo luồng (DTOs) -> Controller -> JSON Response -> Client.

---

## 4. Các file liên quan

**Controllers:**
- `WebApi/Controllers/ProfileController.cs`
- `WebApi/Controllers/MyPetsController.cs`
- `WebApi/Controllers/CustomerAppointmentController.cs`

**Services (Application Layer):**
- `MyPetClinic.Application/Services/UserService.cs`
- `MyPetClinic.Application/Services/CustomerService.cs`
- `MyPetClinic.Application/Services/PetService.cs`
- `MyPetClinic.Application/Services/CustomerAppointmentService.cs`
- `MyPetClinic.Application/Services/AppointmentService.cs`

**DTOs:**
- `UpdateProfileDto`, `CreatePetDto`, `UpdatePetDto`, `CustomerBookingDto`, `ValidateVaccineDto` (Nằm trong `MyPetClinic.Application.DTOs`)

**Entities (Domain Layer):**
- `User`, `Customer`, `Pet`, `Appointment`, `Vaccine`, `VaccinationRecord`, `MedicalRecord`

**Middleware/Filters:**
- `MyPetClinic.WebApi.Filters.AuthorizeOwnerAttribute` (Xử lý chặn IDOR)

---

## 5. Phân tích Controller

### A. CustomerAppointmentController
- **`GET /api/my-appointments`**: 
  - *Mục đích*: Lấy danh sách lịch hẹn của tôi.
  - *Business Logic*: Gọi `GetCurrentCustomerIdAsync()` để lấy ID. Phân trang và lọc theo trạng thái.
- **`POST /api/my-appointments` (BookAppointment)**: 
  - *Mục đích*: Tạo lịch hẹn.
  - *Input*: `CustomerBookingDto`.
  - *Business Logic*: Bắt userId từ Token. Chuyển xuống `CustomerAppointmentService.BookAppointmentAsync` để validate luật.
  - *Exception*: Trả về 400 BadRequest kèm theo thông báo lỗi cụ thể nếu vi phạm rule.
- **`PUT /api/my-appointments/{id}/cancel`**:
  - *Mục đích*: Hủy lịch.
  - *Business Logic*: Hiện tại bị chặn cứng (hardcoded trả về BadRequest) theo PRD mới, yêu cầu khách gọi lễ tân để hủy.

### B. MyPetsController
- **`GET /api/mypets`**: Trả về danh sách thú cưng của customer đang đăng nhập.
- **`POST /api/mypets`**: 
  - *Input*: `CreatePetDto`. 
  - *Validate*: Yêu cầu CustomerId phải khác null (nghĩa là user phải cập nhật hồ sơ trước).
- **`GET /api/mypets/{id}/medical-records`**:
  - *Mục đích*: Xem bệnh án.
  - *Security*: Sử dụng filter `[AuthorizeOwner]` hoặc code kiểm tra quyền `pet.CustomerId == customerId.Value` để đảm bảo không xem trộm bệnh án pet người khác (Chống IDOR).

---

## 6. Phân tích Service

### `CustomerAppointmentService.BookAppointmentAsync`
- **Ý nghĩa**: Core logic xử lý đặt lịch hẹn online.
- **Input**: `CustomerBookingDto`, `customerId`, `userId`.
- **Output**: `long appointmentId`.
- **Logic**:
  1. Từ chối cấp cứu: `if (dto.IsEmergency) throw Exception...`
  2. Yêu cầu SĐT: `if (string.IsNullOrWhiteSpace(customer.Phone)) throw...`
  3. Lead Time: So sánh `appointmentDate < DateTime.Now.AddMinutes(15)`.
  4. Giờ hoạt động: Khai thác `appointmentDate.Hour` để chặn giờ tối và nghỉ trưa.
  5. No-Show limit: Đếm số lịch `no_show` trong 30 ngày qua bằng LINQ.
  6. Spam check: Tìm lịch hẹn cùng ngày của cùng 1 pet, so sánh `Math.Abs((a.AppointmentDate - appointmentDate).TotalHours) < 2`.
  7. Ủy thác cho `_appointmentService.CreateAppointmentAsync` để ghi vào DB.

### `PetService.AddPetAsync`
- **Ý nghĩa**: Khởi tạo thú cưng mới.
- **Logic**: Map `CreatePetDto` sang Domain Entity `Pet`. Gán `CustomerId`. Gọi `_petRepository.CreatePetAsync(pet)`.

---

## 7. Phân tích Entity

### `Customer`
- `Id (Guid)`: PK.
- `CustomerCode (string)`: Mã định danh dễ đọc (CUS...).
- `Phone (string)`: Rất quan trọng, bắt buộc phải có để đặt lịch.
- `DeletedAt (DateTime?)`: Dùng cho Soft Delete.

### `Pet`
- `Id (long)`: PK, auto increment.
- `CustomerId (Guid)`: Foreign Key liên kết với chủ (Customer). Bắt buộc để phân quyền (Owner).
- `IsDeceased (bool)`: Cờ đánh dấu đã mất. Nếu True, tự động hủy các lịch hẹn tương lai.
- `Species`, `Breed`: Phân loại để bác sĩ biết trước.

### `Appointment`
- `Status (string)`: pending, confirmed, cancelled, no_show, completed.
- `AppointmentDate (DateTime)`: Thời gian khám.
- `DoctorId (Guid?)`: Allow null lúc đặt online, lễ tân sẽ gán sau.

---

## 8. Phân tích Database

- **Bảng**: `Users`, `Customers`, `Pets`, `Appointments`, `Vaccines`, `MedicalRecords`.
- **Quan hệ**: 
  - `Users` 1-1 `Customers` (Một tài khoản login gắn với 1 profile khách hàng).
  - `Customers` 1-N `Pets` (Một chủ có nhiều thú cưng).
  - `Pets` 1-N `Appointments` (Một thú cưng có nhiều lịch khám).
- **Soft Delete**: Các bảng master (Customer, Pet) thường áp dụng Soft Delete để giữ nguyên vẹn dữ liệu kế toán/hóa đơn. Thể hiện qua cột `DeletedAt`.

---

## 9. Phân tích API

**Endpoint Đặt lịch (Book Appointment)**
- **URL:** `/api/my-appointments`
- **Method:** `POST`
- **Authorization:** `Bearer Token` (Role: customer)
- **Request Body JSON:**
```json
{
  "petId": 1,
  "serviceId": 2,
  "appointmentDate": "2026-06-31T09:00:00Z",
  "isEmergency": false,
  "symptom": "Bé bị nôn mửa",
  "vaccineId": null
}
```
- **Response Success (200 OK):**
```json
{
  "success": true,
  "message": "Đặt lịch hẹn thành công! Chúng tôi sẽ xác nhận sớm.",
  "id": 105
}
```
- **Response Error (400 Bad Request):**
```json
{
  "success": false,
  "message": "Đặt lịch thất bại: Vui lòng đặt lịch trước ít nhất 15 phút..."
}
```

---

## 10. Business Rules

> [!IMPORTANT]
> Đây là các luật cốt lõi bạn PHẢI thuộc lòng khi fix bug hoặc thêm tính năng:

1. **Không cho phép cấp cứu online:** Các ca cấp cứu phải gọi hotline, không đặt qua web.
2. **Bắt buộc có SĐT:** Hồ sơ Customer phải có SĐT mới cho phép đặt lịch.
3. **Lead Time 15 phút:** `AppointmentDate` phải >= Thời gian hiện tại + 15 phút.
4. **Giờ hành chính:** Chỉ được đặt trong khung 08:00 - 12:00 và 13:30 - 20:00.
5. **Chống bùng kèo:** Khách hàng có >= 3 lịch hẹn bị đánh dấu `no_show` trong 30 ngày qua sẽ bị chặn đặt lịch.
6. **Chống Spam:** Một thú cưng không được đặt 2 lịch hẹn cách nhau dưới 2 tiếng trong cùng 1 ngày.
7. **Khách hàng không tự hủy lịch:** Chức năng Cancel API đã bị chặn, phải gọi điện cho lễ tân (Rule mới).
8. **IDOR Protection:** Khách hàng chỉ được xem/sửa Pet và Appointment của chính mình (`CustomerId` map với `currentUserId`).
9. **Thú cưng qua đời:** Khi update status thú cưng là `IsDeceased = true`, tự động hủy toàn bộ lịch hẹn `pending/confirmed` của thú cưng đó trong tương lai.

---

## 11. Validation

- **Data Annotation (DTO):** Các trường `Required`, `MaxLength` được validate ở mức độ Controller (`ModelState.IsValid`).
- **Business Validation:** 
  - Khách hàng không tồn tại.
  - Pet không thuộc về Khách hàng.
  - Vaccine không tồn tại hoặc hết hàng.
- Kiểm tra trực tiếp trong logic của Service và quăng ra `InvalidOperationException` hoặc `KeyNotFoundException`.

---

## 12. Phân tích Security

- **Authentication:** Sử dụng JWT Bearer Token. Lấy UserId bằng `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
- **Authorization:** Controller được gắn attribute `[Authorize(Roles = "customer")]`.
- **Insecure Direct Object Reference (IDOR):** 
  - Đây là lỗ hổng bảo mật phổ biến nhất. Đã được phòng ngừa triệt để. Ví dụ trong `MyPetsController`, phương thức `UpdatePet` luôn kiểm tra:
  ```csharp
  var pet = await _petService.GetPetByIdAsync(dto.Id, customerId.Value); 
  // GetPetByIdAsync bắt buộc kiểm tra pet.CustomerId == CustomerId thì mới trả về dữ liệu.
  ```
  - Có sử dụng filter custom `[MyPetClinic.WebApi.Filters.AuthorizeOwner]`.

---

## 13. Phân tích Exception

- **Bắt lỗi (Try-Catch):** Hầu hết các action trong Controller đều bọc bởi `try-catch`.
- **Trích xuất lỗi:** 
  ```csharp
  catch (Exception ex) {
      return BadRequest(new { message = ex.Message });
  }
  ```
  Các Exception nghiệp vụ (`InvalidOperationException`) được ném từ Service sẽ được catch ở Controller và trả về HTTP 400 kèm message thân thiện với người dùng. (Nên cân nhắc đưa vào Global Exception Middleware để code clean hơn).

---

## 14. Phân tích Front-end

- **Giao diện:** Viết bằng Vue 3 (Composition API) + TailwindCSS (Glassmorphism design).
- **Call API:** Dùng `fetch` hoặc `axios` (cấu hình interceptor để nhét JWT Token vào header `Authorization: Bearer <token>`).
- **Quản lý state:** Pinia Store để lưu thông tin User profile, giỏ hàng/thú cưng.
- **Validation Form:** Ràng buộc nhập liệu (Vuelidate) trùng khớp với Business Logic (ví dụ chọn giờ bôi xám các khung giờ nghỉ trưa).

---

## 15. Luồng dữ liệu

User (Browser)
↓
Vue Component (Gửi Form Data POST /api/my-appointments)
↓
`CustomerAppointmentController.BookAppointment(CustomerBookingDto)`
↓
`CustomerAppointmentService.BookAppointmentAsync(dto, customerId)`
↓
`UnitOfWork.Appointments.FindAsync` (Lấy dữ liệu check spam, no-show)
↓
SQL Server (Thực thi SELECT)
↓
`AppointmentService.CreateAppointmentAsync()` (Tạo entity)
↓
`UnitOfWork.SaveChangesAsync()` (Commit Transaction)
↓
SQL Server (Thực thi INSERT)
↓
Controller trả về JSON `{ success: true, id: 105 }`
↓
Vue Component chuyển hướng tới trang "Lịch sử hẹn".

---

## 16. Sequence Diagram dạng text

```text
Customer (Vue App)
  |
  |-- [POST /api/my-appointments] --> CustomerAppointmentController
                                        |
                                        |-- GetCurrentCustomerId()
                                        |
                                        |-- BookAppointmentAsync() --> CustomerAppointmentService
                                                                           |
                                                                           |-- [Check 6 Business Rules] 
                                                                           |     (Emergency, Time, Spam, No-show...)
                                                                           |
                                                                           |-- CreateAppointmentAsync() --> AppointmentService
                                                                                                               |
                                                                                                               |-- SaveChangesAsync() --> Database (SQL)
                                                                           | <-- Return AppointmentId --
                                        | <-- Return JSON (200 OK) --
  |<-- Display Success Message --
```

---

## 17. Phân tích từng đoạn code quan trọng

### Method: `BookAppointmentAsync` (trong CustomerAppointmentService.cs)

```csharp
// Kiểm tra nếu là ca cấp cứu thì từ chối đặt online
if (dto.IsEmergency)
{
    throw new InvalidOperationException("TRƯỜNG HỢP CẤP CỨU: Vui lòng KHÔNG đặt lịch online. Hãy đưa bé đến phòng khám ngay lập tức hoặc gọi Hotline khẩn cấp.");
}
```
> **Giải thích:** Khóa vòi cấp cứu ngay từ dòng đầu tiên. Không cần truy vấn DB, tiết kiệm tài nguyên.

```csharp
// Đếm số lần bùng kèo (no_show) và hủy (cancelled) trong 30 ngày qua
var thirtyDaysAgo = DateTime.Now.AddDays(-30);
var recentAppointments = await _unitOfWork.Appointments.FindAsync(
    a => a.CustomerId == customerId && a.AppointmentDate >= thirtyDaysAgo);

var noShowCount = recentAppointments.Count(a => a.Status.ToLower() == "no_show");

if (noShowCount >= 3)
{
    throw new InvalidOperationException("Tài khoản của bạn tạm thời bị hạn chế đặt lịch online do lịch sử vắng mặt nhiều lần. Vui lòng gọi trực tiếp Hotline để được hỗ trợ.");
}
```
> **Giải thích:** 
> - Xác định mốc thời gian 30 ngày trước.
> - LINQ `FindAsync`: Truy vấn database để lấy các lịch hẹn của customer này từ 30 ngày trước đến nay.
> - `.Count(a => a.Status.ToLower() == "no_show")`: Đếm trên memory (Lưu ý: Nơi này có thể TỐI ƯU HÓA bằng cách đẩy .Count() xuống thẳng database thay vì kéo data list về bộ nhớ).
> - Nếu `>= 3` thì ném lỗi, chặn luồng thực thi.

```csharp
// Chống Spam đặt lịch liên tiếp
var todayAppointments = await _unitOfWork.Appointments.FindAsync(
    a => a.CustomerId == customerId && a.PetId == dto.PetId && a.AppointmentDate.Date == appointmentDate.Date && a.Status != "cancelled" && a.Status != "no_show");

if (todayAppointments.Any(a => Math.Abs((a.AppointmentDate - appointmentDate).TotalHours) < 2))
{
    throw new InvalidOperationException("Bé cưng đã có lịch hẹn quá sát với thời gian này. Bạn không thể đặt thêm lịch liên tiếp (chống spam).");
}
```
> **Giải thích:** Lấy các lịch của CÙNG 1 thú cưng trong CÙNG 1 ngày. Lặp qua list (sử dụng `.Any()`). Nếu có bất kỳ lịch nào cách lịch đang định đặt `< 2` tiếng (`Math.Abs(...) < 2`), thì chặn lại. Logic này chống bot spam slot khám.

---

## 18. Những điểm cần lưu ý khi bảo trì

- **Có thể phát sinh bug ở đâu?** Ở việc xử lý Timezone. `DateTime.Now` đang được sử dụng ở backend. Nếu server host ở múi giờ khác Việt Nam (UTC chẳng hạn), thì giờ hành chính 08:00 - 20:00 sẽ bị sai bét. **Nên refactor** sử dụng `TimeZoneInfo` của VN (SE Asia Standard Time) hoặc đồng nhất dùng `DateTime.UtcNow`.
- **Thêm tính năng thì sửa file nào?** Nếu thêm Dịch vụ (Ví dụ: Spa), bạn không cần code thêm, vì hệ thống đang load tự động list dịch vụ. Nhưng nếu thêm logic "Spa phải đặt trước 2 tiếng", thì phải vào `CustomerAppointmentService.cs` thêm nhánh `if (dto.ServiceId == X)`.
- **Ảnh hưởng API:** Bất kỳ thay đổi nào trong Exception Message cũng sẽ được Vue.js hiển thị trực tiếp cho user. Hãy viết câu văn thân thiện.

---

## 19. Best Practices đang áp dụng

- **Clean Architecture:** Rất tốt. UI (Controller) không chứa logic nghiệp vụ. Database logic nằm trong Repository (`UnitOfWork`). Core business nằm trong Service.
- **DTOs:** Sử dụng DTO chuẩn chỉ, không bao giờ lộ Entity thật (như Entity chứa `DeletedAt` hay info nhạy cảm) ra ngoài API.
- **Dependency Injection (DI):** Khởi tạo Service qua Constructor Injection.
- **Điểm chưa áp dụng (Nên làm):** 
  - Khá nhiều khối `try/catch` lặp lại trong Controller. Nên sử dụng **Global Exception Handling Middleware** để bắt các lỗi `InvalidOperationException` và tự mapping ra Status 400.
  - LINQ đếm (Count) đang kéo list object về memory thay vì đếm tại SQL.

---

## 20. Đánh giá chất lượng chức năng

- **Ưu điểm:** Logic bảo mật (chống IDOR) rất chặt chẽ. Đã bao trùm được rất nhiều kịch bản thực tế của phòng khám thú y (chống spam, chống bùng kèo, nghỉ trưa, báo tử tự hủy lịch).
- **Nhược điểm:** Xử lý `DateTime.Now` có rủi ro nếu deploy server quốc tế. 
- **Điểm nên refactor:** 
  Đoạn code: `var recentAppointments = await _unitOfWork.Appointments.FindAsync(...)` theo sau là `.Count()` trên bộ nhớ RAM. Nên tạo 1 hàm ở Repository: `await _appointmentRepository.CountNoShowAsync(customerId, thirtyDaysAgo)`. Tính toán tổng hợp (Aggregate) trực tiếp tại Database.

---

## 21. Bảng Tóm Tắt (Dành cho Member mới)

| Hạng mục | Chi tiết |
| :--- | :--- |
| **Chức năng** | Customer Profile, Pet Management, Online Booking |
| **File Logic Chính** | `CustomerAppointmentService.cs`, `PetService.cs` |
| **API Chính** | `POST /api/my-appointments` (Đặt lịch) |
| **Database Liên Quan**| Bảng `Customers`, `Pets`, `Appointments` |
| **Business Rules** | Cấm cấp cứu; Bắt buộc có SĐT; Lead time >= 15p; Nằm trong giờ hành chính; Không bùng kèo (No-show < 3); Không spam pet (< 2h). |
| **Bảo mật quan trọng**| Chống IDOR (luôn so sánh `CustomerId` của Pet/Appt với User đăng nhập). |
| **Quy trình xử lý** | Auth Token -> Controller -> Service Check Rule -> Repo Save -> Return JSON |
| **Điểm Cần Nhớ** | Không cho User tự hủy lịch qua API (`PUT /cancel` đã bị disable). |
| **Điểm Dễ Gây Lỗi** | Trùng múi giờ khi thao tác `DateTime.Now` và LINQ Count kéo data về RAM (Cần tối ưu). |

---
*(Hết tài liệu đào tạo - Version 1.0)*
