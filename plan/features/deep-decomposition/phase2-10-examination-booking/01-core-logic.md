# 🧠 Core Business Logic - Online Examination Booking

## 1. Thuật toán Chống đặt trùng lịch Bác sĩ (Double-booking Prevention)
Xung đột lịch hẹn của bác sĩ điều trị là vấn đề thường trực của các phòng khám thú y. MyPetClinic áp dụng quy tắc nghiệp vụ giãn cách cứng:

*   **Quy tắc $\pm30$ phút:** Một bác sĩ không thể tiếp nhận hai ca khám có thời gian bắt đầu nằm trong khoảng cách dưới 30 phút. 
*   **Ví dụ:** Nếu Bác sĩ A đã được xếp lịch khám vào lúc `14:00`, bất kỳ request đặt lịch mới cho Bác sĩ A từ `13:30` đến `14:30` đều bị chặn đứng và báo lỗi trùng lịch.
*   **Lọc lịch hủy:** Lịch hẹn ở trạng thái `cancelled` (đã hủy) sẽ không được tính vào kiểm tra trùng.
*   **Mã nguồn LINQ Entity Framework Core:**
```csharp
var isDoubleBooked = await _unitOfWork.Appointments
    .AnyAsync(a => a.DoctorId == finalDoctorId 
                && a.Status != "cancelled"
                && a.AppointmentDate >= appointmentDate.AddMinutes(-30) 
                && a.AppointmentDate <= appointmentDate.AddMinutes(30));

if (isDoubleBooked)
{
    throw new InvalidOperationException("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
}
```

---

## 2. Logic phân bổ Bác sĩ tự động (Automatic Doctor Assignment)
Để đơn giản hóa trải nghiệm cho khách hàng không quen biết hoặc không yêu cầu đích danh bác sĩ điều trị:
*   Nếu `dto.DoctorId` gửi lên rỗng (`Guid.Empty`), hệ thống tự động tìm kiếm danh sách các Bác sĩ đang hoạt động trong hệ thống.
*   **Thuật toán phân phối đơn giản (Round-robin hoặc First available):**
    *   Hệ thống truy vấn DB lấy danh sách bác sĩ thuộc nhóm Role `doctor` đang hoạt động (`IsActive == true`).
    *   Chỉ định bác sĩ đầu tiên trong danh sách hợp lệ làm bác sĩ điều trị cho ca khám này.
*   **C# Code Reference:**
```csharp
var finalDoctorId = dto.DoctorId;
if (finalDoctorId == Guid.Empty)
{
    var doctors = await _unitOfWork.Users.FindWithIncludesAsync(
        u => u.Role != null && u.Role.Name.ToLower() == "doctor" && u.IsActive == true,
        u => u.Role!
    );
    var doctor = doctors.FirstOrDefault();
    if (doctor != null)
    {
        finalDoctorId = doctor.Id;
    }
    else
    {
        throw new InvalidOperationException("Hệ thống hiện không có bác sĩ nào đang trực để phân công!");
    }
}
```

---

## 3. Logic phân loại Trạng thái Lịch hẹn (Status Categorization)
Khi lịch hẹn được tạo thành công, hệ thống tự động phân loại trạng thái y tế dựa trên khoảng cách thời gian bắt đầu so với thời điểm hiện tại:

*   **Trạng thái Chờ tiếp nhận (`waiting`):**
    *   **Điều kiện:** Thời gian khám diễn ra trong vòng **1 giờ** kể từ thời điểm hiện tại (`AppointmentDate <= DateTime.UtcNow.AddHours(1)`).
    *   **Nghiệp vụ:** Khách hàng đến trực tiếp hoặc đặt lịch sát giờ khám ngay. Chuyển thẳng vào hàng đợi chờ khám của Lễ tân để cấp số khám.
*   **Trạng thái Chờ duyệt (`pending`):**
    *   **Điều kiện:** Thời gian khám diễn ra xa hơn **1 giờ** trong tương lai.
    *   **Nghiệp vụ:** Xếp vào danh sách chờ duyệt. Lễ tân sẽ gọi điện hoặc kiểm tra lịch của phòng để nhấn nút duyệt sau.

```csharp
// Thiết lập mặc định là khám ngay
appointment.Status = "waiting"; 

// Nếu thời gian lớn hơn hiện tại 1 giờ thì chuyển sang chờ duyệt
if (appointment.AppointmentDate > DateTime.UtcNow.AddHours(1))
{
    appointment.Status = "pending"; 
}
```

---

## 4. Cơ chế sinh mã QR Token Check-in
*   Mỗi lịch hẹn được cấp một chuỗi mã bảo mật duy nhất `QrToken` có định dạng `QR-XXXXXXXX` (8 ký tự hexa ngẫu nhiên) để sinh mã QR trên giao diện người dùng.
*   Khi khách hàng đưa thú cưng đến phòng khám, lễ tân chỉ cần dùng máy quét mã vạch quét mã QR trên điện thoại khách hàng, hệ thống tự động check-in và đổi trạng thái lịch sang `waiting` nhanh chóng mà không cần tìm kiếm tên thủ công.
