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


### Bug: Kh�ng t?i du?c th�ng b�o tr�n Frontend
- **Nguy�n nh�n:** File 
otification.store.js g?i sai port Backend (5288 thay v� 5285).
- **Kh?c ph?c:** S?a l?i URL API v� SignalR Hub th�nh port 5285 trong store Vue.

### Bug: Th�ng b�o kh�ng hi?n th? d� d� s?a d�ng Port
- **Nguy�n nh�n:** File 
otification.store.js du?c vi?t theo chu?n d�ng JWT Token (localStorage.getItem('token')), nhung h? th?ng Backend c?a MyPetClinic l?i dang d�ng **Cookie Authentication**. Do kh�ng t�m th?y token trong localStorage, Axios request b? h?y l?ng l? (return s?m) n�n kh�ng bao gi? g?i l�n Backend.
- **Kh?c ph?c:** Lo?i b? ho�n to�n logic ki?m tra JWT Token v� th�m c?u h�nh withCredentials: true v�o t?t c? c�c request Axios v� k?t n?i SignalR d? tr�nh duy?t t? d?ng d�nh k�m Cookie x�c th?c h?p l?.

---

## [BUG-TZ-001] Gio hien thi thanh toan sai lech 7 tieng

- **Trang thai:** `FIXED`
- **Thoi gian:** 21-06-2026

### Nguyen nhan
Npgsql Legacy Mode tra ve DateTime Kind=Unspecified, JSON serialize khong co chu Z, browser coi la Local Time thay vi UTC.

### Giai phap
Them UtcDateTimeConverter + UtcNullableDateTimeConverter vao Program.cs AddJsonOptions.


## [BUG-PET-002] Hồ sơ & bệnh sử thú cưng không hiển thị

- **Trạng thái:** FIXED
- **Thời gian:** 22-06-2026

### Nguyên nhân
1. Backend MedicalRecordService.cs (GetPetMedicalHistoryAsync) đang truy vấn .Appointment.PetId == petId thay vì truy vấn trực tiếp vào .PetId == petId. Điều này có thể dẫn đến không trả về bản ghi nào nếu Appointment bị detach.
2. Frontend Vue Component ConsultationRecordTab.vue đang tham chiếu các trường không tồn tại trên MedicalRecordDto (ví dụ: ecord.symptoms thay vì ecord.clinicalSigns, ecord.treatment thay vì ecord.treatmentPlan, ecord.note thay vì ecord.doctorNotes). DTO cũng trả về mảng object thuốc prescribedMedicines chứ không phải mảng chuỗi.

### Giải pháp
1. Sửa LINQ query trong backend thành .PetId == petId để lấy trực tiếp hồ sơ bệnh án theo PetId.
2. Sửa lại các property bindings trong template Vue ConsultationRecordTab.vue để khớp chính xác với DTO trả về, bao gồm cả mảng object thuốc.

## [BUG-PET-003] Root Cause Xác định: petId=0 trong localStorage

- **Trạng thái:** FIXED
- **Thời gian:** 22-06-2026

### Nguyên nhân gốc rễ
Backend AppointmentService.GetCalendarEventsAsync() xây dựng ExtendedProps cho mỗi sự kiện lịch nhưng **thiếu trường petId và customerId**. Frontend DoctorQueueTab.vue khi bác sĩ nhấn 'Tiến hành khám' đọc evt.extendedProps?.petId → trả về undefined → fallback || '0' → lưu chuỗi "0" vào localStorage. Khi ConsultationRecordTab mount lên, gọi API /medical-records/pet/0 → không có bản ghi nào.

### Giải pháp
1. **Backend AppointmentService.cs**: Thêm petId = a.PetId và customerId = a.CustomerId vào object ExtendedProps trong GetCalendarEventsAsync().
2. **Backend DoctorController.cs**: Thêm endpoint GET /doctor/appointment/{appointmentId} cho phép bác sĩ lấy thông tin cuộc hẹn (để fallback resolve petId khi giá trị cũ trong localStorage bằng 0).
3. **Frontend ConsultationRecordTab.vue**: Thêm logic kiểm tra petId > 0 trước khi gọi etchPetHistory(). Nếu petId = 0, tự động gọi fallback API /doctor/appointment/{id} để lấy petId thực và tự sửa localStorage.

## [BUG-APPT-004] Lỗi khung giờ trống không đồng bộ với cấu hình Operating Hours

- **Trạng thái:** FIXED
- **Thời gian:** 22-06-2026

### Nguyên nhân gốc rễ
Endpoint lấy các khung giờ khả dụng (GET /api/appointments/available-slots) và logic tạo lịch hẹn (CreateAppointmentAsync) hoàn toàn bỏ qua thiết lập **Khung giờ hoạt động chung** (ClinicOperatingDays / ClinicOperatingShifts) và **Ngày nghỉ lễ** (ClinicHolidays). Cả 2 đang fallback về cấu hình lịch trực của bác sĩ (DoctorSchedule) hoặc sinh tự động giờ hành chính từ slot_config.json (từ 8h-20h), do đó bác sĩ có thể có khung giờ trống và khách hàng vẫn đặt lịch được vào các khoảng thời gian mà phòng khám đáng lẽ đã đóng cửa.

### Giải pháp
1. **Trong GetAvailableSlotsAsync (AppointmentService.cs)**:
   - Truy vấn ClinicHolidays tương ứng với ngày hẹn. Nếu là ngày lễ, lập tức trả về mảng rỗng [] (không có bác sĩ nào nhận khám).
   - Truy vấn ClinicOperatingDays. Nếu ngày đó cấu hình IsOpen = false, lập tức trả về [].
   - Lọc các mốc thời gian khả dụng (do SlotCalculationHelper sinh ra) bằng cách đối chiếu với danh sách các ClinicOperatingShifts của ngày đó. Các khung giờ nào nằm ngoài hoặc tràn ra khỏi giờ hoạt động sẽ bị loại bỏ ngay từ phía Server.
   
2. **Trong CreateAppointmentAsync (AppointmentService.cs)**:
   - Thêm bước xác thực đầu vào (Validation) trước khi lấy bác sĩ và phân lịch:
     - Nếu ngày hẹn trùng ngày lễ IsActive, ném ra InvalidOperationException("Phòng khám đóng cửa vào ngày nghỉ lễ này...").
     - Nếu cấu hình phòng khám trong ngày không hoạt động (!IsOpen), ném ra lỗi.
     - Kiểm tra trực tiếp thời gian hẹn (AppointmentDate.TimeOfDay) với các ca trực của phòng khám. Nếu thời gian nằm ngoài mọi ca hoặc thời lượng khám tràn ra khỏi giờ nghỉ ca, chặn việc đặt lịch.

## [BUG-APPT-005] Ngày hẹn hiển thị sai lệch khi đặt qua giao diện khách hàng

- **Trạng thái:** FIXED
- **Thời gian:** 24-06-2026

### Nguyên nhân gốc rễ
Frontend gửi AppointmentDate dạng yyyy-MM-ddTHH:mm:00 (VD: 25/06/2026 10:30), được Backend deserialize với Kind=Unspecified. Khi EF Core lưu vào PostgreSQL, DateTimeUtcConverter gọi .ToUniversalTime() chuyển giờ Local (Vietnam +07:00) thành giờ UTC (VD: 24/06/2026 17:30 UTC). Khi Backend truy vấn và map vào AppointmentDetailDto, việc gọi .Date trên giá trị UTC này trả về ngày 24 thay vì 25, dẫn đến lỗi lệch ngày hiển thị trên giao diện người dùng.

### Giải pháp
1. **Trong AppointmentService.cs**: Cập nhật tất cả các biểu thức mapping DTO từ .AppointmentDate.Date.Add(a.StartTime) thành .AppointmentDate.ToLocalTime().Date.Add(a.StartTime). Việc chuyển đổi về LocalTime trước khi lấy Date giúp lấy lại đúng múi giờ trước khi nối chuỗi ngày tháng gửi về Frontend.
2. **Trong SlotCalculationHelper.cs**: Cập nhật biểu thức tính pptTime tương tự để logic kiểm tra trùng lịch không bị sai lệch ngày.

## [BUG-WALKIN-001] Lỗi 500 khi tạo lịch hẹn vãng lai do truy vấn LINQ
- **Trạng thái:** FIXED
- **Thời gian:** 28-06-2026
### Nguyên nhân
Dùng .Contains("doctor") trên đối tượng Role gây lỗi dịch ngược (InvalidOperationException) của EF Core trên PostgreSQL.
### Giải pháp
Sửa lại truy vấn so sánh chuỗi tường minh: .Where(u => u.Role != null && (u.Role.Name.ToLower() == "clinical_doctor" || u.Role.Name.ToLower() == "vaccination_doctor" || u.Role.Name.ToLower() == "doctor") && u.IsActive == true && u.DeletedAt == null).

## [BUG-WALKIN-002] Lỗi 400 Bad Request thiếu Số điện thoại
- **Trạng thái:** FIXED
- **Thời gian:** 28-06-2026
### Nguyên nhân
API GetCustomerByPhone không trả về số điện thoại. Frontend lấy selectedCustomer.value.phone bị undefined, dẫn đến Model Validation [Required] của WalkInRequestDto.Phone bị lỗi 400 Bad Request, trả về thông báo chung chung.
### Giải pháp
1. Thêm Phone vào object trả về của API GetCustomerByPhone trong ReceptionistController.cs.
2. Bọc lót lấy customerForm.value.customerPhone trong AppointmentsTab.vue nếu phone rỗng. Thêm Validation frontend cho serviceId và hiển thị chi tiết mảng errors.

## [BUG-WALKIN-003] Lỗi lệch múi giờ khi lưu giờ hẹn vãng lai
- **Trạng thái:** FIXED
- **Thời gian:** 28-06-2026
### Nguyên nhân
CreateWalkInAsync sử dụng DateTime.UtcNow để lưu AppointmentDate, StartTime, CheckInTime. Nếu giờ Việt Nam là 19h27, giờ UTC là 12h27, CSDL lưu 12h27 dẫn đến giao diện hiển thị sai.
### Giải pháp
Dùng TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTimeZone) để chuyển sang giờ Việt Nam trước khi gán vào các thuộc tính thời gian.
## [BUG-QUEUE-001] Trang thai lich kham bi bo qua buoc Cho thanh toan
- **Trang thai:** FIXED
- **Thoi gian:** 30-06-2026
### Nguyen nhan
Khi bac si hoan tat kham va tao MedicalRecord, MedicalRecordService.cs gan truc tiep appointment.Status = "completed" thay vi "ready_to_pay". Do do, ca kham bi lot qua buoc hien thi tren bang Hang kham voi cot "Cho thanh toan".
### Giai phap
Sua doi appointment.Status = "ready_to_pay" trong MedicalRecordService.cs khi khoi tao benh an moi qua SOAP hoac thong thuong.

| 6/30/2026 | BUG-MED-001 | Medicine stock desync causes MedicalRecord save failure | ExportMedicineAsync deducted Batch CurrentQuantity but forgot Medicine StockQuantity | Fixed in MedicineService.cs |

| L?i bi?n m?t l?ch h?n cu | Entity Framework Core INNER JOIN v?i c�c b?n ghi li�n quan (b�c si, th� cung) b? soft-delete (x�a m?m), khi?n truy v?n Select v� t�nh lo?i b? l?ch h?n trong danh s�ch tr? v?. Count v?n d?m d? nhung khi Skip().Take() th� k?t qu? b? h?t. | Th�m .IgnoreQueryFilters() v�o truy v?n LINQ t?i AppointmentService.cs d? b? qua b? l?c x�a m?m c?a b?ng Users v� Pets. |

| L?i ph�n trang b? tr?ng (?n n�t) do d? li?u b? orphaned | Vi?c d�ng .IgnoreQueryFilters() chua d? n?u b?n ghi (Pet, Doctor) b? x�a c?ng (hard-delete), EF Core v?n d�ng INNER JOIN lo?i b? l?ch h?n. Gi?i ph�p: L?y danh s�ch l?ch h?n tru?c b?ng ToList() (ch? 5 record m?i trang) r?i g�n d? li?u th? c�ng (manual fetching). Frontend cung c?n chuy?n n�t ph�n trang ra ngo�i -else d? kh�ng b? ?n. | Vi?t h�m MapToDetailDtoAsync trong AppointmentService.cs d? query d? li?u r?i r?c, tr�nh EF Core t?o INNER JOIN. |
