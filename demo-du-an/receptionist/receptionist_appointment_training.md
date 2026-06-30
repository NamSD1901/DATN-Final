# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG QUẢN LÝ LỊCH HẸN & PHÂN CÔNG BÁC SĨ (RECEPTIONIST) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 10 (Đã cập nhật Bổ sung). Trọng tâm của Lễ tân không chỉ là đứng cười với khách, mà họ còn là người "Nắm giữ thời gian" của toàn bộ Bác sĩ trong phòng khám.
> Chức năng Quản lý Lịch hẹn (Appointments Management) chứa đựng 2 thuật toán khổng lồ: **Transaction Serializable (Chống đụng độ lịch)** và **Auto-Resolve Doctor (Thuật toán cân bằng tải Bác sĩ)**. Kèm theo đó là luồng duyệt lịch và tải dữ liệu lên Calendar. Hãy cùng mổ xẻ chúng!

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Lịch hẹn & Phân công Bác sĩ (Receptionist View).
- **Mục đích:** Cho phép Lễ tân quản lý toàn bộ lưới thời gian (Calendar) của phòng khám. Lễ tân có thể duyệt lịch do khách đặt, tạo lịch mới giùm khách, hoặc đổi lịch/đổi bác sĩ nếu có sự cố đột xuất.
- **Điểm nổi bật (Kỹ thuật):** Tích hợp State Machine quản lý 6 trạng thái lịch hẹn, kèm theo công nghệ Push Notification khi duyệt lịch. Thuật toán cân bằng tải đảm bảo chia đều việc cho các Bác sĩ.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - THỰC THỂ (ENTITY) VÀ ĐÓNG GÓI (DTO)

**Tệp:** `MyPetClinic.Domain/Entities/Appointment.cs` & `MyPetClinic.Application/DTOs/AppointmentCreateDto.cs`

Để quản lý một mạng lưới thời gian phức tạp, bảng `Appointment` (Lịch hẹn) phải đóng vai trò như một trạm trung chuyển (Junction Table), kết nối tất cả các thực thể khác lại với nhau.

```csharp
// --- TRONG TẦNG DOMAIN (Thực thể lõi Database) ---
    public class Appointment
    {
        // 1. CÁC KHÓA NGOẠI (FOREIGN KEYS) LIÊN KẾT ĐA CHIỀU
        public long PetId { get; set; } // (Khám cho Thú cưng nào?)
        public Guid CustomerId { get; set; } // (Ai là chủ thanh toán tiền?)
        public Guid DoctorId { get; set; } // (Bác sĩ nào mổ?)
        public long ServiceId { get; set; } // (Sử dụng dịch vụ gì?)
        public long? VaccineId { get; set; } // (Nếu là tiêm phòng thì tiêm thuốc gì?)

        // 2. LƯỚI THỜI GIAN
        public DateTime AppointmentDate { get; set; } // (Ngày đến khám)
        public TimeSpan StartTime { get; set; } // (Giờ bắt đầu. Ví dụ 09:00:00)
        
        // 3. STATE MACHINE (TRẠNG THÁI)
        public string Status { get; set; } = "pending"; // (Mặc định khi Khách tự tạo là Đang chờ duyệt)
        
        // 4. MÃ ĐỊNH DANH ĐIỆN TỬ
        public string? QrToken { get; set; } // (Mã QR duy nhất cấp cho khách hàng quét lúc đến cửa)
    }

// --- TRONG TẦNG APPLICATION (Thùng hàng giao tiếp với Frontend UI) ---
    public class AppointmentCreateDto
    {
        // (Validation Thép: Không cho phép Lễ tân gửi Form lên thiếu dữ liệu)
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thú cưng")]
        public long PetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ phụ trách")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn dịch vụ")]
        public long ServiceId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do khám / triệu chứng")]
        public string Symptom { get; set; } = null!;
        // ...
    }
```

---

### PHẦN 2.2 - TẦNG CONTROLLER (ĐẶC QUYỀN CỦA LỄ TÂN KHI TẠO LỊCH)

**Tệp:** `WebApi/Controllers/AppointmentController.cs`

Khác với Khách hàng tự đặt lịch (phải chờ Bác sĩ hoặc Lễ tân duyệt trạng thái `pending`), khi Lễ tân là người tạo lịch giùm, hệ thống ngầm định Lễ tân đã chốt quyền lợi trực tiếp bằng miệng, nên lịch sẽ bay thẳng lên trạng thái `confirmed`.

```csharp
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
        {
            try
            {
                // (1. Lấy ID của nhân viên Lễ tân đang đăng nhập)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var createdBy = userIdClaim != null ? Guid.TryParse(userIdClaim.Value, out var uid) ? uid : Guid.Empty : Guid.Empty;

                // (2. Đẩy xuống Service để xử lý thuật toán tạo lịch hẹn chống đụng độ)
                var appointmentId = await _appointmentService.CreateAppointmentAsync(dto, createdBy);

                // 3. ĐẶC QUYỀN LỄ TÂN: TỰ ĐỘNG XÁC NHẬN (AUTO-CONFIRM)
                // (Ghi đè trạng thái pending thành confirmed ngay lập tức)
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, "confirmed");

                return Ok(new { success = true, message = "Đã tạo lịch hẹn thành công!", id = appointmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }
```

---

### PHẦN 2.3 - TẦNG SERVICE (MỨC ĐỘ CÁCH LY CAO NHẤT: SERIALIZABLE KHI TẠO LỊCH)

**Tệp:** `MyPetClinic.Application/Services/AppointmentService.cs`

Khi có 4 cô Lễ tân cùng gọi điện chốt lịch lúc 9h sáng, làm sao để hệ thống không xếp 2 người vào cùng 1 bác sĩ tại cùng 1 khung giờ? Đáp án là `IsolationLevel.Serializable`.

```csharp
        public async Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, Guid createdBy)
        {
            // (THIẾT LẬP VÒNG LẶP RETRY: Nếu bị khóa do Transaction thì thử lại 3 lần)
            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                // (BẬT CHẾ ĐỘ BẢO MẬT TỐI ĐA (SERIALIZABLE): Xếp hàng tuần tự tuyệt đối, chặn mọi giao dịch khác can thiệp)
                await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    // (1. Kiểm tra Ngày nghỉ lễ và Khung giờ đóng/mở cửa)
                    var isHoliday = _unitOfWork.ClinicHolidays.Query().Any(h => h.IsActive && h.StartDate <= targetDateStart && h.EndDate >= targetDateStart);
                    if (isHoliday) throw new InvalidOperationException("Phòng khám đóng cửa vào ngày nghỉ lễ này.");

                    // (2. Kiểm tra Đụng độ lịch của Khách hàng - Double Booking)
                    var customerSameDayApts = _unitOfWork.Appointments.Query()
                        .Where(a => a.CustomerId == dto.CustomerId && a.Status != "cancelled" && a.AppointmentDate == targetDateUtc)
                        .Select(a => a.StartTime).ToList();

                    // (Cách nhau dưới 30 phút là báo lỗi liền)
                    var isCustomerDoubleBooked = customerSameDayApts.Any(startTime => Math.Abs((startTime - appointmentDate.TimeOfDay).TotalMinutes) < 30);
                    if (isCustomerDoubleBooked) throw new InvalidOperationException("Khách hàng này đã có lịch hẹn trong khung giờ này.");

                    // (3. Kích hoạt Thuật toán cân bằng tải chọn Bác sĩ - xem chi tiết ở mục 2.4)
                    var finalDoctorId = ResolveAndValidateDoctorId(dto.DoctorId, appointmentDate, dto.ServiceId);

                    // (4. Sinh mã QR độc nhất để lát khách ra cửa quét)
                    // ... (Tạo và Lưu xuống DB)
```

**Giải thích chi tiết:**
- **Serializable Transaction:** Đây là cấp độ khóa (Lock) cao nhất trong SQL Server. Nó biến các truy vấn song song (Parallel) thành tuần tự (Sequential). Nghĩa là nếu Lễ tân A và Lễ tân B cùng bấm tạo lịch vào 09:00:00, SQL Server sẽ bắt Lễ tân B đứng đợi Lễ tân A lưu xuống Database xong, thì mới cho Lễ tân B chạy qua vạch kiểm tra trùng giờ. ĐẢM BẢO 100% KHÔNG BAO GIỜ TRÙNG LỊCH BÁC SĨ.

---

### PHẦN 2.4 - THUẬT TOÁN TỰ ĐỘNG PHÂN CÔNG BÁC SĨ (LOAD BALANCING)

Nếu Lễ tân không biết phân cho ai (chọn Bác sĩ = Trống), hệ thống tự giải quyết qua hàm `ResolveAndValidateDoctorId`:

```csharp
        private Guid ResolveAndValidateDoctorId(Guid requestedDoctorId, DateTime appointmentDate, long serviceId)
        {
            var finalDoctorId = requestedDoctorId; 

            // (NẾU KHÔNG CHỌN BÁC SĨ, BẬT CHẾ ĐỘ AUTO-LOAD-BALANCING)
            if (finalDoctorId == Guid.Empty)
            {
                // BƯỚC 1: LỌC CHUYÊN MÔN
                // (Chỉ lấy bác sĩ có skill Tiêm phòng nếu dịch vụ là Tiêm)
                // ...

                // BƯỚC 2: TÌM BÁC SĨ CÓ LỊCH TRỰC (SCHEDULE)
                var doctorsWithSchedules = _unitOfWork.DoctorSchedules.Query()
                    .Where(s => s.WorkDate == targetDateStart && s.IsAvailable).ToList();
                var doctorsList = doctorsWithSchedules
                    .Where(s => appointmentTime >= s.StartTime && appointmentTime + TimeSpan.FromMinutes(30) <= s.EndTime)
                    .Select(s => s.DoctorId).ToList();

                // BƯỚC 3: TRUY TÌM BÁC SĨ THỰC SỰ RẢNH TAY
                // (Loại bỏ những ông bác sĩ đang có ca mổ cách lúc này < 30 phút)
                var busyDoctorIds = allAptsForDoctors
                    .Where(a => Math.Abs((a.StartTime - appointmentDate.TimeOfDay).TotalMinutes) < 30)
                    .Select(a => a.DoctorId).ToList();

                // (PHÉP TRỪ TẬP HỢP: Danh sách Trực ban - Danh sách Bận = Danh sách Rảnh)
                var availableDoctors = doctorsList.Except(busyDoctorIds).ToList();

                // BƯỚC 4: CHỐT BÁC SĨ ÍT VIỆC NHẤT ĐỂ CÂN BẰNG TẢI TRỌNG
                // (Đếm số ca trong ngày của mỗi bác sĩ, xếp hạng từ nhỏ đến lớn)
                var doctorLoads = availableDoctors.Select(dId => new
                {
                    DoctorId = dId,
                    Load = allAptsForDoctors.Count(a => a.DoctorId == dId) 
                }).OrderBy(x => x.Load).ToList();

                // (Giao cho ông nhàn rỗi nhất)
                finalDoctorId = doctorLoads.First().DoctorId; 
            }
            return finalDoctorId;
        }
```

---

### PHẦN 2.5 - DUYỆT LỊCH VÀ XUẤT LÊN BẢNG CALENDAR

Để Lễ tân có thể quản lý lịch mượt mà, hệ thống phải xuất dữ liệu ra chuẩn của thư viện **FullCalendar** trên giao diện, đồng thời Lễ tân có thể bấm Duyệt (Approve) trạng thái.

**Tệp:** `MyPetClinic.Application/Services/AppointmentService.cs`

```csharp
        // 1. LẤY DỮ LIỆU ĐỔ LÊN CALENDAR
        public async Task<IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId)
        {
            var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => a.AppointmentDate >= start && a.AppointmentDate <= end, // (Quét theo tuần/tháng)
                a => a.Pet!, a => a.Customer!, a => a.Doctor!, a => a.Service!
            );

            var events = new List<CalendarEventDto>();
            foreach (var a in appointments)
            {
                // (GẮN MÀU SẮC DỰA TRÊN TRẠNG THÁI ĐỂ LỄ TÂN NHÌN CÁI LÀ HIỂU)
                var color = "#6c757d"; // Mặc định xám
                if (a.Status == "pending") color = "#ffc107"; // (Màu Vàng: Chờ duyệt)
                else if (a.Status == "confirmed") color = "#0dcaf0"; // (Xanh Cyan: Đã chốt lịch)
                else if (a.Status == "waiting") color = "#0d6efd"; // (Xanh dương: Khách đã tới cửa chờ)
                else if (a.Status == "cancelled") color = "#dc3545"; // (Màu Đỏ: Khách bùng lịch/Hủy)

                events.Add(new CalendarEventDto
                {
                    Id = a.Id.ToString(),
                    Title = $"{a.Pet?.Name} - {a.Customer?.FullName}", // (Hiển thị tiêu đề ô Lịch)
                    Start = startDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Color = color,
                    // ... (Gắn thêm các trường chi tiết vào ExtendedProps)
                });
            }
            return events;
        }

        // 2. LỄ TÂN DUYỆT LỊCH (PENDING -> CONFIRMED)
        public async Task<bool> UpdateAppointmentStatusAsync(long id, string status, string? reason = null)
        {
            var appointment = _unitOfWork.Appointments.Query().FirstOrDefault(a => a.Id == id);
            
            // (Đổi trạng thái)
            appointment.Status = status.ToLower();
            
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            // (HỆ THỐNG PUSH NOTIFICATION: Gửi thông báo về App của khách hàng)
            if (status == "confirmed" || status == "cancelled")
            {
                var message = status == "confirmed" 
                    ? $"Lịch hẹn của bạn vào lúc {appointment.AppointmentDate} đã được phê duyệt."
                    : $"Lịch hẹn của bạn đã bị hủy.";

                var customerUser = _unitOfWork.Users.Query().FirstOrDefault(u => u.CustomerId == appointment.CustomerId);
                if (customerUser != null)
                {
                    // (Gắn chung với hệ thống chuông báo đỏ góc màn hình của Customer)
                    await _notificationService.CreateNotificationAsync(customerUser.Id, "Cập nhật lịch hẹn", message, "AppointmentUpdate");
                }
            }
            return true;
        }
```

**Giải thích chi tiết:**
- Code trả về FullCalendar không chỉ trả thô dữ liệu, mà nó tích hợp **Logic Tô Màu (Color Coding)**. Đây là UX tuyệt vời cho UI. Nhìn bảng lịch vàng khè là Lễ tân biết "Chết rồi, còn đống lịch chưa duyệt".
- Chức năng duyệt lịch `UpdateAppointmentStatusAsync` không chỉ cập nhật Database, mà nó còn đánh thức **NotificationService** để "ting ting" lên điện thoại Khách hàng biết rằng lịch của họ đã được chốt.

---
*(Hết tài liệu đào tạo chuyên sâu Lễ tân: Quản lý Lịch hẹn - Bản Đầy Đủ Entity & DTO)*
