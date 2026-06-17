# 🐞 Nhật Ký Sửa Lỗi - Debugging & Error Log

Dưới đây là nhật ký ghi lại các sự cố, nguyên nhân và giải pháp khắc phục trong quá trình phát triển và kiểm thử dự án MyPetClinic.

---

## 📅 Lỗi 1: Vi phạm Khóa Ngoại doctor_id khi đặt lịch hẹn (DbUpdateException)

- **Trạng thái:** `✅ FIXED`
- **Thời gian phát hiện:** 10-06-2026

### 1. Mô tả lỗi (Symptom)
Khi thực hiện tạo lịch hẹn mới từ giao diện tiếp nhận của Lễ tân, hệ thống ném ra ngoại lệ:
```
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. 
---> Npgsql.PostgresException (0x80004005): 23503: insert or update on table "appointments" violates foreign key constraint "appointments_doctor_id_fkey"
```

### 2. Nguyên nhân (Root Cause)
- Giao diện tiếp nhận gọi API `GET /api/receptionist/doctors` để hiển thị danh sách bác sĩ lên dropdown, nhưng danh sách trả về bị rỗng `[]`.
- Do dropdown rỗng, Lễ tân gửi payload với `doctorId = ""` (chuỗi rỗng), dẫn tới Model Binder của .NET tự động gán giá trị mặc định là `Guid.Empty` (`00000000-0000-0000-0000-000000000000`). Khi Entity Framework Core lưu bản ghi vào bảng `appointments`, PostgreSQL kiểm tra và thấy không có tài khoản bác sĩ nào có ID này trong bảng `users` nên trả về lỗi vi phạm ràng buộc FK.
- Nguyên nhân khiến danh sách bác sĩ rỗng là do sự không đồng bộ về chữ hoa/thường (Case-sensitivity): Database seeder khởi tạo vai trò ở dạng chữ thường (`"doctor"`, `"customer"`), trong khi code truy vấn trong `ReceptionistService.cs` so sánh bằng chuỗi chính xác có phân biệt hoa thường (`u.Role.Name == "Doctor"` và `u.Role.Name == "Customer"`).

### 3. Giải pháp khắc phục (Fix)
- Cập nhật toàn bộ các câu lệnh so sánh vai trò trong [ReceptionistService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/ReceptionistService.cs) sang so sánh không phân biệt hoa thường bằng phương thức `.ToLower()`:
  - `u.Role.Name.ToLower() == "doctor"`
  - `u.Role.Name.ToLower() == "customer"`
- Chạy kiểm thử tự động `dotnet test` thành công và xác nhận sửa lỗi triệt để.

---

## 📅 Lỗi 2: Đặt lịch khám trực tuyến thất bại & hiển thị "Invalid Date" trên Client Portal

- **Trạng thái:** `✅ FIXED`
- **Thời gian phát hiện:** 10-06-2026

### 1. Mô tả lỗi (Symptom)
- Khách hàng thực hiện đặt lịch khám online qua màn hình "Lịch hẹn của tôi". Tại bước 3 (Xác nhận), trường Thời gian hiển thị chữ `"Invalid Date"`.
- Khi nhấn nút "Xác nhận đặt lịch", hệ thống báo lỗi `"Đặt lịch thất bại. Vui lòng thử lại."` màu đỏ.

### 2. Nguyên nhân (Root Cause)
- **Lỗi đặt lịch thất bại:** Do khách hàng đặt lịch trực tuyến không có bước chọn Bác sĩ (hệ thống sẽ phân bổ sau). API của khách hàng `POST /api/my-appointments` nhận DTO với `DoctorId` trống (`Guid.Empty`). Ở Backend, hàm `CreateAppointmentAsync` của `AppointmentService` không tự động phân phối bác sĩ khi `DoctorId` rỗng mà cố gắng insert trực tiếp `Guid.Empty` xuống DB, vi phạm khóa ngoại `appointments_doctor_id_fkey` giống Lỗi 1.
- **Lỗi hiển thị "Invalid Date":** Hàm `formatDatetimeLocal` trong frontend sử dụng lệnh `new Date(val)` trực tiếp trên chuỗi `datetime-local` (định dạng `YYYY-MM-DDTHH:mm`). Trong một số môi trường trình duyệt (như Safari/WebKit), việc thiếu timezone khiến hàm khởi tạo Date trả về `Invalid Date`.

### 3. Giải pháp khắc phục (Fix)
- **Backend:** Bổ sung logic tự động phân bổ bác sĩ đang hoạt động đầu tiên trong hệ thống vào hàm `CreateAppointmentAsync` (nằm tại [AppointmentService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AppointmentService.cs)) khi `dto.DoctorId` gửi lên là `Guid.Empty`.
- **Frontend:** Cải tiến hàm `formatDatetimeLocal` trong [MyAppointmentsTab.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/dashboard/MyAppointmentsTab.vue) để chuyển đổi kí tự `T` thành khoảng trắng nhằm tăng tính tương thích khi parse ngày trên WebKit/Safari.

---

## 📅 Lỗi 3: EF Core InMemory Database Query Translation trả về 0 kết quả trong Unit Test

- **Trạng thái:** `✅ FIXED`
- **Thời gian phát hiện:** 13-06-2026

### 1. Mô tả lỗi (Symptom)
- Khi chạy unit test `GetCustomerAppointmentsPaginated_ShouldReturnPaginatedResults` trong `AppointmentServiceTests.cs`, kết quả `Items` trả về rỗng (0 dòng) mặc dù `TotalCount` trả về đúng 3 dòng.
- Lỗi xảy ra cả khi đặt `.Select()` trước hay sau các phương thức phân trang `.Skip().Take().ToList()`.

### 2. Nguyên nhân (Root Cause)
- Nhà cung cấp cơ sở dữ liệu EF Core InMemory (`UseInMemoryDatabase`) gặp hạn chế khi dịch truy vấn LINQ phức tạp chiếu sang kiểu vô danh (anonymous projection) chứa các thuộc tính liên kết (`a.Pet`, `a.Customer`, `a.Doctor`, `a.Service`) nhưng các bảng liên quan không được seed dữ liệu hoặc có khoá ngoại không hợp lệ.
- Khi đánh giá biểu thức chiếu dạng `PetName = a.Pet != null ? a.Pet.Name : null` trên đối tượng không tồn tại ở cơ sở dữ liệu in-memory, EF Core InMemory sẽ không tạo các liên kết LEFT JOIN giống SQL thông thường mà bỏ qua dòng dữ liệu đó hoặc không dịch được chính xác dẫn tới trả về rỗng.

### 3. Giải pháp khắc phục (Fix)
- Khôi phục thứ tự tối ưu `.Select()` trước `.Skip().Take()` trong [AppointmentService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AppointmentService.cs) để đảm bảo tối ưu hoá câu truy vấn khi chạy trên DB thật.
- Cập nhật hàm test `GetCustomerAppointmentsPaginated_ShouldReturnPaginatedResults` trong [AppointmentServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/AppointmentServiceTests.cs): Seed đầy đủ các thực thể liên quan bao gồm `User` (Customer & Doctor) và `Service` khớp với khoá ngoại để EF Core InMemory ánh xạ và chiếu dữ liệu thành công.
- Sau khi sửa đổi, toàn bộ 22/22 unit tests đều vượt qua thành công (`Passed!`).

---

## 📅 Lỗi 4: Lệch ngày giờ sảnh chờ (Timezone Offset Bug) & Truy vấn phi tối ưu (Non-SARGable Query)

- **Trạng thái:** `✅ FIXED`
- **Thời gian phát hiện:** 13-06-2026

### 1. Mô tả lỗi (Symptom)
- Khách hàng check-in hoặc đăng ký Walk-in vào đầu ngày (từ 00:00 đến 07:00 sáng theo giờ Việt Nam) bị hệ thống từ chối hoặc xếp nhầm vào hàng đợi của ngày hôm trước.
- API lấy hàng đợi `/api/receptionist/queue` hiển thị sai danh sách hoặc bị rỗng vào các khung giờ sáng sớm do lệch múi giờ giữa máy chủ (UTC) và phòng khám (UTC+7).

### 2. Nguyên nhân (Root Cause)
- **Timezone mismatch:** Code gốc lấy mốc hôm nay sử dụng `DateTime.UtcNow.Date` trực tiếp trên database. Ở Việt Nam lúc 05:00 sáng (UTC+7 ngày 13/06) thì UTC vẫn là 22:00 đêm ngày 12/06. Do đó, hệ thống tìm các lịch hẹn của ngày 12/06 thay vì 13/06.
- **Non-SARGable query:** Biểu thức `a.AppointmentDate.Date == today` ép cơ sở dữ liệu phải chạy qua từng bản ghi để trích xuất phần ngày (chạy Table Scan/Index Scan), làm chậm nghiêm trọng hệ thống khi số lượng lịch hẹn tăng cao.

### 3. Giải pháp khắc phục (Fix)
- Viết hàm helper `GetVietnamTodayUtcRange` trong [ReceptionistService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/ReceptionistService.cs) để tính toán chính xác khoảng thời gian của ngày hiện tại theo giờ Việt Nam (UTC+7) quy đổi sang UTC: `[StartLocalUtc, EndLocalUtc)`.
- Thay thế hoàn toàn so sánh `.Date` bằng so sánh khoảng thời gian: `a.AppointmentDate >= startUtc && a.AppointmentDate < endUtc` giúp tối ưu hiệu năng (SARGable query) sử dụng được Index trên DB.
- Cập nhật và bổ sung bộ unit test trong [ReceptionistServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/ReceptionistServiceTests.cs), hoàn thành 28/28 tests passed.

---

## 📅 Lỗi 5: Chặn đặt lịch khám liền kề (Back-to-Back Scheduling Block) do so sánh không loại trừ

- **Trạng thái:** `✅ FIXED`
- **Thời gian phát hiện:** 13-06-2026

### 1. Mô tả lỗi (Symptom)
- Khách hàng không thể đặt lịch khám vào các khung giờ nối tiếp nhau (ví dụ: ca 1 đã đặt lúc 09:00 - 09:30, ca tiếp theo định đặt lúc 09:30 - 10:00 của cùng bác sĩ sẽ bị báo lỗi "Bác sĩ đã có lịch hẹn trong khoảng thời gian này.").
- Việc này làm giảm công suất phục vụ của phòng khám, tạo các khoảng trống chết 30 phút giữa các ca khám không đáng có.

### 2. Nguyên nhân (Root Cause)
- Trong logic kiểm tra trùng lịch (double-booking check) của [AppointmentService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AppointmentService.cs) ở cả 3 luồng (Đặt lịch mới, Đăng ký Walk-in, và Dời lịch), biểu thức so sánh thời gian sử dụng toán tử không loại trừ `>=` và `<=`:
  `a.AppointmentDate >= appointmentDate.AddMinutes(-30) && a.AppointmentDate <= appointmentDate.AddMinutes(30)`.
- Khi ca 1 lúc 09:00 kết thúc lúc 09:30, ca 2 muốn đặt lúc 09:30 sẽ đối chiếu và thấy ca 1 (09:00) nằm trong dải `[09:30 - 30, 09:30 + 30]` (tức `[09:00, 10:00]`). Do toán tử so sánh lấy cả dấu `=`, hệ thống nhận định ca 2 bị trùng với ca 1 và báo lỗi.

### 3. Giải pháp khắc phục (Fix)
- Thay đổi toán tử so sánh từ không loại trừ (`>=` và `<=`) sang loại trừ nghiêm ngặt (`>` và `<`) trong cả 3 hàm của [AppointmentService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AppointmentService.cs):
  `a.AppointmentDate > appointmentDate.AddMinutes(-30) && a.AppointmentDate < appointmentDate.AddMinutes(30)`.
- Việc đổi sang so sánh loại trừ khớp hoàn toàn với logic tính toán slot trống trong [SlotCalculationHelper.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Helpers/SlotCalculationHelper.cs), cho phép đặt các ca liền kề nhau hoàn hảo (back-to-back) mà vẫn chặn đứng 100% các ca thực sự bị chồng chéo thời gian.
- Chạy lại toàn bộ bộ kiểm thử tự động, 28/28 tests passed thành công.

---

## 📅 Lỗi 6: Tất cả lịch hẹn hiển thị 00:00 và lỗi "không trùng lịch mà đã đặt" (Double-Booking Bypass)

- **Trạng thái:** `✅ FIXED`
- **Thời gian phát hiện:** 16-06-2026

### 1. Mô tả lỗi (Symptom)
- Ở giao diện Dashboard khách hàng (MyAppointmentsTab) và Lễ tân, thời gian đặt lịch của tất cả các lịch hẹn luôn hiển thị là `00:00` thay vì giờ thực tế.
- Khách hàng than phiền "không trùng lịch mà đã đặt", nghĩa là chức năng chống đặt trùng (Double-Booking Check) trên UI không hoạt động. Các slot đã được đặt bởi người khác vẫn hiển thị là "Trống" (Available) cho người tiếp theo, dẫn đến tình trạng hai người cùng đặt thành công vào một slot giờ.

### 2. Nguyên nhân (Root Cause)
- **Kiến trúc DB:** Entity `Appointment` tách biệt phần thời gian ra thành 2 thuộc tính: `AppointmentDate` (chỉ lưu phần ngày) và `StartTime` (lưu phần giờ). 
- **Lỗi logic khi Tạo Lịch:** Trong hàm `CreateAppointmentAsync` ở `AppointmentService.cs`, code cũ chỉ gán `AppointmentDate = appointmentDate`, nhưng **quên gán** thuộc tính `StartTime`. Kết quả là `StartTime` nhận giá trị mặc định `TimeSpan.Zero` (tức `00:00:00`). Do backend đang bật `EnableLegacyTimestampBehavior`, EF Core Npgsql cắt bỏ múi giờ nhưng PostgreSQL vẫn chỉ lưu phần thời gian là 00:00:00 nếu ta chỉ ánh xạ `DateTime` sang dạng Date thuần.
- **Lỗi mapping DTO:** Khi lấy dữ liệu lịch hẹn (`GetCustomerAppointmentsPaginatedAsync`), logic `.ToString("yyyy-MM-ddTHH:mm:ss")` được gọi thẳng trên `AppointmentDate` thay vì cộng gộp với `StartTime`. Kết quả là frontend luôn nhận chuỗi thời gian kết thúc bằng `T00:00:00`.
- **Hệ lụy Double-Booking:** Hàm sinh slot trống `SlotCalculationHelper.GetAvailableSlots` so sánh `slot` (chứa giờ cụ thể, ví dụ 14:30) với `appt.AppointmentDate` (bị reset về 00:00:00). Kết quả so sánh khoảng cách lệch nhau vài trăm phút, do đó logic luôn coi là "không trùng lặp", hiển thị slot đó thành `Trống`.

### 3. Giải pháp khắc phục (Fix)
- Cập nhật logic lưu lịch hẹn ở [AppointmentService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AppointmentService.cs): Phân rã thành `AppointmentDate = appointmentDate.Date` và ép gán `StartTime = appointmentDate.TimeOfDay`.
- Cập nhật toàn bộ các bộ ánh xạ DTO trong Service: Sử dụng biểu thức `a.AppointmentDate.Date.Add(a.StartTime).ToString(...)` để tái tạo lại cấu trúc Datetime chuẩn gửi cho Client.
- Chỉnh sửa [SlotCalculationHelper.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Helpers/SlotCalculationHelper.cs) để tính toán chuẩn xác biến `apptTime` bằng cách cộng gộp `AppointmentDate` và `StartTime`, từ đó khắc phục triệt để khả năng bypass cơ chế kiểm tra chống đặt lịch trùng.

