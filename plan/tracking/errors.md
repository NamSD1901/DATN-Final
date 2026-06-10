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

