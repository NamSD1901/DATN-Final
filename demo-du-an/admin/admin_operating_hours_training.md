# TÀI LIỆU ĐÀO TẠO NỘI BỘ: QUẢN TRỊ KHUNG GIỜ LÀM VIỆC (ADMIN VIEW) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 16, dành riêng cho **Quản trị viên (Admin)**.
> Việc cấu hình Giờ mở cửa, Ca làm việc và Ngày nghỉ lễ (Holidays) tưởng chừng đơn giản nhưng lại tiềm ẩn rủi ro phá vỡ toàn bộ lịch hẹn đã đặt trước của Khách hàng.
> Trọng tâm của tài liệu này là kiến trúc Dữ liệu, thuật toán **Kiểm tra Xung đột (Conflict Checker)** và **Thuật toán Ghi đè (Wipe & Re-insert)** cấu hình.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Khung giờ hoạt động & Ngày nghỉ lễ (Operating Hours & Holidays).
- **Mục đích:** Khởi tạo lịch làm việc hàng tuần (T2-CN), chia ca (Sáng/Chiều), cấu hình số lượng ca tối đa và ngày nghỉ lễ.
- **Điểm nổi bật (Kỹ thuật):** Xử lý bất đồng bộ, thuật toán chặn lỗi Booking, và kỹ thuật lưu trữ File JSON đối với cấu hình tĩnh (SlotConfig) kết hợp cùng SQL Server.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - THỰC THỂ (ENTITY) VÀ QUAN HỆ 1-N (NGÀY - CA)

**Tệp:** `MyPetClinic.Domain/Entities/ClinicOperatingDay.cs` & `ClinicOperatingShift.cs`

```csharp
// --- THỰC THỂ NGÀY LÀM VIỆC TRONG TUẦN ---
    public class ClinicOperatingDay
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; } // (Enum: 0 là Chủ Nhật, 1 là Thứ 2...)
        public bool IsOpen { get; set; } = true; // (Hôm đó có mở cửa không?)
        
        // (Khóa ngoại 1-N: Một ngày có thể có nhiều Ca làm việc)
        public ICollection<ClinicOperatingShift> Shifts { get; set; } = new List<ClinicOperatingShift>();
    }

// --- THỰC THỂ CA LÀM VIỆC ---
    public class ClinicOperatingShift
    {
        public int Id { get; set; }
        public int ClinicOperatingDayId { get; set; } // (Trỏ về Ngày phía trên)
        
        public TimeSpan StartTime { get; set; } // Giờ bắt đầu (VD: 08:00:00)
        public TimeSpan EndTime { get; set; }   // Giờ kết thúc (VD: 12:00:00)
    }
```

---

### PHẦN 2.2 - LỚP VỎ ĐÓNG GÓI (DTO) CẤU TRÚC LỒNG NHAU

**Tệp:** `MyPetClinic.Application/DTOs/OperatingHoursDtos.cs`

```csharp
    // (DTO Mẹ: Đóng gói toàn bộ 7 ngày)
    public class UpdateOperatingHoursDto
    {
        public List<ClinicOperatingDayDto> Days { get; set; } = new();
    }

    // (DTO Con 1: Ngày)
    public class ClinicOperatingDayDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsOpen { get; set; }
        public List<ClinicOperatingShiftDto> Shifts { get; set; } = new(); // Chứa list các Ca
    }

    // (DTO Con 2: Ca)
    public class ClinicOperatingShiftDto
    {
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
    }
```

---

### PHẦN 2.3 - THUẬT TOÁN KIỂM TRA XUNG ĐỘT TRƯỚC KHI LƯU (BLOCKER)

Giả sử Thứ 6 Lễ tân đã nhận 10 lịch hẹn. Đùng một cái, Admin tắt "Mở cửa" của Thứ 6. Tầng Service sẽ chặn đứng thao tác.

**Tệp:** `MyPetClinic.Application/Services/OperatingHoursService.cs`

```csharp
            // Lấy các lịch hẹn trong tương lai
            var upcomingAppointments = await _unitOfWork.Appointments.FindAsync(
                a => a.AppointmentDate >= DateTime.UtcNow.Date && a.Status != "cancelled" && a.Status != "completed");

            foreach (var appt in upcomingAppointments)
            {
                var dayConfig = dto.Days.FirstOrDefault(d => d.DayOfWeek == appt.AppointmentDate.DayOfWeek);
                if (dayConfig != null)
                {
                    // Lịch đã hẹn mà Admin dám đóng cửa ngày đó?
                    if (!dayConfig.IsOpen)
                        throw new InvalidOperationException($"Conflict: Có lịch hẹn (Mã: {appt.Id}) vào ngày {appt.AppointmentDate:dd/MM/yyyy} nhưng bạn đang muốn đóng cửa ngày này.");
                }
            }
```

---

### PHẦN 2.4 - THUẬT TOÁN GHI ĐÈ CẤU HÌNH THÀNH CÔNG (WIPE & RE-INSERT)

Sau khi kiểm tra toàn vẹn dữ liệu thành công (không có ai đặt nhầm giờ), Service sẽ tiến hành lưu Cấu hình xuống Database. Thay vì đi so sánh từng ca một để Update (Rất mất thời gian), hệ thống MyPetClinic dùng chiến thuật **Wipe & Re-insert (Xóa sạch & Thêm mới toàn bộ)**.

**Tệp:** `MyPetClinic.Application/Services/OperatingHoursService.cs`

```csharp
            // BƯỚC 1: WIPE - XÓA SẠCH CẤU HÌNH CŨ
            var oldDays = await _unitOfWork.ClinicOperatingDays.GetAllAsync();
            foreach (var oldDay in oldDays)
            {
                // Tìm tất cả các Ca cũ của ngày này
                var oldShifts = await _unitOfWork.ClinicOperatingShifts.FindAsync(s => s.ClinicOperatingDayId == oldDay.Id);
                foreach (var shift in oldShifts)
                {
                    _unitOfWork.ClinicOperatingShifts.Remove(shift); // Xóa Ca cũ
                }
                _unitOfWork.ClinicOperatingDays.Remove(oldDay); // Xóa Ngày cũ
            }

            // BƯỚC 2: RE-INSERT - THÊM CẤU HÌNH MỚI 100%
            foreach (var d in dto.Days)
            {
                var dayEntity = new ClinicOperatingDay
                {
                    DayOfWeek = d.DayOfWeek,
                    IsOpen = d.IsOpen
                };
                
                await _unitOfWork.ClinicOperatingDays.AddAsync(dayEntity); // Thêm Ngày mới
                
                // Thêm các Ca mới thuộc về ngày đó
                if (d.IsOpen)
                {
                    foreach (var s in d.Shifts)
                    {
                        dayEntity.Shifts.Add(new ClinicOperatingShift
                        {
                            StartTime = s.StartTime,
                            EndTime = s.EndTime
                        });
                    }
                }
            }

            // BƯỚC 3: COMMIT TRANSACTION XUỐNG DATABASE
            await _unitOfWork.SaveChangesAsync();
            return true;
```

**Giải thích chi tiết (Chiến thuật Wipe & Re-insert):**
- Cấu hình 7 ngày trong tuần là một lượng dữ liệu rất nhỏ (Chỉ 7 dòng Ngày và khoảng 14 dòng Ca sáng/chiều).
- Việc lôi 21 dòng cũ lên, so sánh với 21 dòng mới xem dòng nào thay đổi để gọi hàm `Update()` là vô nghĩa và tốn CPU của Server. 
- Thay vào đó, ta Xóa trắng (Wipe) toàn bộ, rồi Insert lại 100%. Cách này sạch sẽ, đảm bảo Database không bao giờ bị rác, Code lại ngắn gọn dễ hiểu!

---

### PHẦN 2.5 - CẤU HÌNH SỐ CA TỐI ĐA VÀ THỜI LƯỢNG KHÁM (LƯU VÀO JSON)

Ngoài giờ mở cửa, Admin còn phải cấu hình 1 ca khám kéo dài bao lâu (Vd: 30 phút), và trong khung 30 phút đó được nhận bao nhiêu Bệnh nhân (Vd: Phòng khám có 3 bác sĩ -> nhận tối đa 3 ca).
Vì dữ liệu này siêu nhẹ và đọc cực nhiều (Mỗi khi book lịch đều đọc), nó được lưu thẳng ra **File JSON** trên Server thay vì lưu Database!

**Tệp:** `MyPetClinic.Application/Services/AdminService.cs`

```csharp
    public class AdminService : IAdminService
    {
        // Đường dẫn file lưu cấu hình trên Ổ cứng Server
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "slot_config.json");

        // (1) HÀM ĐỌC CẤU HÌNH TỪ FILE JSON
        public SlotConfigModel GetSlotConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new SlotConfigModel
                {
                    StartTime = "08:00:00",
                    EndTime = "17:00:00",
                    DurationMinutes = 30, // 1 Ca 30 phút
                    MaxAppointmentsPerSlot = 3 // 3 Khách / 30 Phút
                };
                SaveConfig(defaultConfig); // Tự tạo file nếu chưa có
                return defaultConfig;
            }

            // Đọc file từ ổ cứng lên
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<SlotConfigModel>(json) ?? new SlotConfigModel();
        }

        // (2) HÀM LƯU CẤU HÌNH THÀNH CÔNG VÀO FILE
        public async Task UpdateSlotConfigAsync(SlotConfigModel config, string currentUserId)
        {
            // BƯỚC 1: LƯU VÀO JSON
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);

            // BƯỚC 2: GHI LOG AUDIT KIỂM TOÁN LẠI
            await _auditLogService.LogActionAsync(currentUserId, "UpdateSlotConfig", $"Cập nhật cấu hình khung giờ làm việc");
        }
    }
```

**Giải thích chi tiết (Micro-Optimization):**
- Tại sao không tạo Bảng Database cho 4 con số con con này? Bởi vì nếu tạo bảng, mỗi khi một khách hàng vào màn hình Book lịch, hệ thống sẽ phải gửi 1 câu SQL `SELECT * FROM Config` xuống CSDL. 1000 khách hàng thì CSDL gánh 1000 truy vấn chỉ để hỏi "1 ca dài bao lâu?".
- Giải pháp lưu File `slot_config.json` giúp Ram/CPU đọc trực tiếp từ bộ nhớ cực nhanh, giảm tải tuyệt đối cho CSDL. Khi Admin cấu hình thành công, hệ thống chỉ việc đè chuỗi JSON mới xuống ổ cứng bằng hàm `File.WriteAllText`! 

---
*(Hết tài liệu đào tạo chuyên sâu Admin: Khung giờ & Ngày nghỉ)*
