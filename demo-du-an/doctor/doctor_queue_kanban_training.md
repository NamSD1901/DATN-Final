# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG QUẢN LÝ CA KHÁM & KANBAN (DOCTOR VIEW) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 12 (Đã bổ sung cấu trúc Entity & DTO), xoay trục sang góc nhìn của **Bác sĩ (Doctor)**.
> Đối với Bác sĩ, thời gian và sự tập trung là vàng. Hệ thống cung cấp cho họ một bảng điều khiển (Dashboard) dạng **Kanban Board**, nơi họ có thể thấy chính xác Khách hàng nào đang đứng đợi trước cửa phòng mình.
> Điểm cốt lõi kỹ thuật ở đây là **RBAC (Phân quyền động)**, **State Machine (Chuyển trạng thái Khám)**, và **Cấu trúc dữ liệu đóng gói (DTO)**.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Ca khám & Lịch làm việc (Doctor Workspace).
- **Mục đích:** Giúp bác sĩ xem lịch làm việc trong tuần (FullCalendar) và quản lý luồng khám bệnh trong ngày (Kéo thả thẻ bệnh nhân từ Chờ khám -> Đang khám -> Chờ thanh toán).
- **Điểm nổi bật (Kỹ thuật):** Tái sử dụng (Reuse) hoàn toàn API hàng đợi của Lễ tân, nhưng bổ sung lớp màng lọc Bảo mật Phân quyền (RBAC) để chặn Bác sĩ A nhìn trộm hoặc thao tác nhầm vào bệnh nhân của Bác sĩ B.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG CONTROLLER (BẢO MẬT PHÂN QUYỀN RBAC)

**Tệp:** `WebApi/Controllers/DoctorController.cs`

Làm sao để một API duy nhất có thể phục vụ cả Lễ tân (xem toàn bộ) và Bác sĩ (chỉ xem của mình)? Bí mật nằm ở việc giải mã JWT Token để ép buộc (force) bộ lọc ID.

```csharp
    [Authorize(Roles = "doctor,admin,receptionist,Doctor,Admin,Receptionist,SystemAdmin,clinical_doctor,vaccination_doctor")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        // 1. TẢI LỊCH LÀM VIỆC TRONG TUẦN (FULLCALENDAR)
        [HttpGet("weekly-schedule")]
        public async Task<IActionResult> GetMyWeeklySchedule([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] Guid? targetDoctorId = null)
        {
            // (1. Bóc tách ID của người đang gửi Request từ JWT Token)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var currentDoctorId)) return Unauthorized();

            // (2. KIỂM TRA PHÂN QUYỀN ĐỘNG - RBAC)
            // (Lễ tân và Admin được coi là Quản trị viên cấp cao)
            var isStrictAdmin = User.IsInRole("admin") || User.IsInRole("Admin") || User.IsInRole("receptionist") || User.IsInRole("Receptionist") || User.IsInRole("SystemAdmin");
            
            // (BÍ QUYẾT LỌC ĐỘNG)
            // - Nếu là Admin/Lễ tân: Cho phép họ xem lịch của bất kỳ bác sĩ nào (targetDoctorId).
            // - Nếu là Bác sĩ: ÉP BUỘC biến filter phải bằng chính ID của họ (currentDoctorId). Tránh việc truyền láo ID để xem trộm người khác.
            Guid? filterDoctorId = isStrictAdmin ? targetDoctorId : currentDoctorId;

            // (3. Đẩy xuống Service dùng chung của hệ thống để lấy dữ liệu)
            var events = await _appointmentService.GetCalendarEventsAsync(start, end, filterDoctorId);
            return Ok(events);
        }

        // 2. TẢI HÀNG ĐỢI KANBAN TRONG NGÀY
        [HttpGet("queue")]
        public async Task<IActionResult> GetMyQueue()
        {
            // (1. Vay mượn hàm GetTodayQueueAsync của Lễ tân để lấy toàn bộ 50 khách đang chờ ngoài sảnh)
            var queue = await _receptionistService.GetTodayQueueAsync();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdStr)) return Ok(new List<object>());

            // (2. Dùng LINQ để Lọc thô trên RAM: Chỉ giữ lại những người khách được phân công cho đúng vị Bác sĩ đang đăng nhập)
            var myQueue = queue.Where(q => q.DoctorId.ToString().Equals(userIdStr, StringComparison.OrdinalIgnoreCase)).ToList();
            
            return Ok(myQueue); // (Bác sĩ chỉ thấy 5 người của mình)
        }
```

**Giải thích chi tiết:**
- Code thể hiện rõ triết lý **"Zero Trust"** (Không tin tưởng Client). Dù Client (Frontend) có cố tình truyền lên `targetDoctorId` của sếp giám đốc, Backend vẫn sẽ phớt lờ và ép biến `filterDoctorId` thành ID của chính Bác sĩ đó nếu họ không có quyền Admin.
- Tốc độ tải trang `GetMyQueue` cực kỳ nhanh vì nó tận dụng lại Cache/Query của Lễ tân và chỉ chạy vòng lặp lọc lại trên RAM (LINQ `Where`), thay vì bắn thêm 1 câu truy vấn SQL độc lập xuống Database.

---

### PHẦN 2.2 - TẦNG CONTROLLER (STATE MACHINE - CHUYỂN TRẠNG THÁI KHÁM)

Trong Bệnh viện, hồ sơ bệnh án vật lý được chuyển từ tay người này sang tay người khác. Trên hệ thống, nó là hành động "Chuyển Trạng thái" (State Machine).

```csharp
        // 3. BÁC SĨ NHẬN BỆNH NHÂN VÀO PHÒNG KHÁM
        [HttpPost("start-treatment")]
        public async Task<IActionResult> StartTreatment([FromBody] long appointmentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var queue = await _receptionistService.GetTodayQueueAsync();
            var appt = queue.FirstOrDefault(q => q.AppointmentId == appointmentId);
            if (appt == null) return NotFound(new { message = "Không tìm thấy ca khám." });

            // (VALIDATION KÉP: Chặn đứng hành vi Bác sĩ A bấm nhầm nút "Bắt đầu khám" của Bác sĩ B)
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !appt.DoctorId.ToString().Equals(userIdStr, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid(); // (Mã lỗi 403: Không có quyền)
            }

            // KÍCH HOẠT STATE MACHINE: Chuyển thẻ Kanban từ cột "Chờ khám" sang "Đang khám"
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, "in_progress");
            return Ok(new { success });
        }

        // 4. BÁC SĨ KHÁM XONG, ĐẨY KHÁCH RA QUẦY LỄ TÂN
        [HttpPost("finish-treatment")]
        public async Task<IActionResult> FinishTreatment([FromBody] long appointmentId)
        {
            // ... (Các bước Validation giống y hệt StartTreatment)

            // KÍCH HOẠT STATE MACHINE: Chuyển thẻ Kanban từ cột "Đang khám" sang "Chờ thanh toán"
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, "ready_to_pay");
            return Ok(new { success });
        }
```

**Giải thích chi tiết:**
- Chuỗi State Machine của toàn hệ thống Kanban diễn ra như sau: 
  - Khách tự đặt (`pending`) -> Lễ tân duyệt (`confirmed`) -> Khách tới cửa check-in (`waiting`) -> **Bác sĩ gọi vào phòng (`in_progress`)** -> **Bác sĩ khám xong (`ready_to_pay`)** -> Thu ngân thu tiền (`completed`).

---

### PHẦN 2.3 - THỰC THỂ & ĐÓNG GÓI (ENTITY & DTO) TRONG KANBAN

Để giao diện Kanban có thể hiển thị dạng thẻ (Cards) với đầy đủ màu sắc cảnh báo (ví dụ Chó cắn người báo màu Đỏ), Backend phải nhào nặn bảng `Appointment` thô kệch thành một chiếc DTO chuyên dụng mang tên `QueueItemDto`.

**Tệp:** `MyPetClinic.Application/DTOs/QueueItemDto.cs` & `MyPetClinic.Domain/Entities/Appointment.cs`

```csharp
// --- TRONG TẦNG DOMAIN (Thực thể gốc) ---
    public class Appointment
    {
        public long Id { get; set; }
        public long PetId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid DoctorId { get; set; }
        public string Status { get; set; } = "pending";
        // ... (Chứa các khóa ngoại)
    }

// --- TRONG TẦNG APPLICATION (Lớp vỏ DTO bọc lại phục vụ Kanban) ---
    public class QueueItemDto
    {
        // 1. Dữ liệu gốc
        public long AppointmentId { get; set; }
        
        // 2. DỮ LIỆU ĐÃ ĐƯỢC "TRẢI PHẲNG" (FLATTENING) ĐỂ UI RENDER SIÊU TỐC
        public long PetId { get; set; }
        public string? PetName { get; set; } // (Lấy từ bảng Pet thông qua Include)
        public string? Species { get; set; }
        public decimal? Weight { get; set; }
        
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; } // (Lấy từ bảng Customer)
        
        public Guid DoctorId { get; set; }
        public string? DoctorName { get; set; } // (Lấy từ bảng User/Doctor)
        
        public string? Status { get; set; } // (Waiting, InProgress, ReadyToPay)
        public int QueueNumber { get; set; } // (Số thứ tự ngoài sảnh)
        
        // 3. CỜ CẢNH BÁO (WARNING FLAGS) CHO BÁC SĨ
        public bool IsEmergency { get; set; } // (Cấp cứu! Cần ưu tiên vượt hàng đợi)
        public bool IsAggressive { get; set; } // (Cảnh báo Thú cưng hung dữ, bác sĩ nhớ đeo găng tay)
    }
```

**Giải thích chi tiết:**
- `QueueItemDto` là minh chứng rõ nhất cho việc sử dụng **Data Transfer Object**. Nếu ta ném thẳng `Appointment` nguyên bản xuống giao diện Kanban, UI sẽ phải tự đi móc nối để moi ra Tên Chó, Tên Chủ. Nhờ có `QueueItemDto`, Backend đã trải phẳng (Flatten) dữ liệu. UI chỉ việc nhận 1 cục JSON và in ra màn hình.
- Các cờ `IsEmergency` và `IsAggressive` cực kỳ quan trọng cho Bác sĩ, giúp họ biết ngay con chó đang đứng ngoài cửa có cắn người hay không để chuẩn bị rọ mõm.

---
*(Hết tài liệu đào tạo chuyên sâu Bác sĩ: Quản lý Ca khám & Luồng Kanban)*
