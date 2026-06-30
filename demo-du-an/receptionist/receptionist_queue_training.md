# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG QUẢN LÝ HÀNG ĐỢI LỄ TÂN (CLINIC QUEUE) - BẢN FULL DEEP DIVE (COMBO GIẢI THÍCH KÉP)

> [!NOTE]
> Tài liệu số 7 chuyển hướng trọng tâm sang **Góc nhìn của Lễ tân (Receptionist)** - những người đóng vai trò điều phối viên của phòng khám.
> Trái tim của phân hệ Lễ tân chính là **Hàng đợi khám (Queue Management)**. Tính năng này phải đối mặt với một bài toán hóc búa về lập trình đồng thời (Concurrency): Làm sao để 2 lễ tân cùng lúc Check-in cho 2 khách hàng mà không bị cấp trùng 1 số thứ tự khám bệnh?
> Giải pháp nằm ở tuyệt kỹ `SemaphoreSlim` sẽ được phân tích dưới đây.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Hàng đợi & Check-in (Receptionist Queue).
- **Mục đích:** Khi khách hàng đến phòng khám (bước vào cửa), Lễ tân dùng máy quét mã QR hoặc tìm số điện thoại để Check-in. Hệ thống sẽ phát một Số thứ tự (Queue Number) và tự động xếp khách vào hàng đợi của Bác sĩ.
- **Quy tắc kinh doanh (Business Rules):** 
  - Khách Walk-in (khách đến không hẹn trước) vẫn được cấp số thứ tự nhưng phải chờ sau những khách đã đặt lịch.
  - Các ca Cấp cứu (Emergency) được ưu tiên phá vỡ hàng đợi, tự động đẩy lên số 1.
  - Chống cấp trùng Số thứ tự bằng khóa luồng Thread-safe.
  - Không cho phép Check-in nếu sai ngày hẹn (Ví dụ: Lịch hẹn thứ 6 nhưng thứ 5 đã vác mèo đến).

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG CONTROLLER (GIAO DIỆN LỄ TÂN)

**Tệp:** `WebApi/Controllers/ReceptionistController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    // (CHỐT CHẶN BẢO MẬT: Chỉ Lễ tân và Admin mới có quyền thao tác với Hàng đợi)
    [Authorize(Roles = "receptionist,admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReceptionistController : ControllerBase
    {
        private readonly IReceptionistService _receptionistService;

        public ReceptionistController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }

        /// <summary>
        /// Quét mã QR hoặc Nhập ID để Check-in và vào Hàng đợi.
        /// </summary>
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
        {
            try
            {
                // (Bàn giao cho Service xử lý cấp số thứ tự)
                var success = await _receptionistService.CheckInAsync(request);
                
                if (success)
                    return Ok(new { success = true, message = "Check-in thành công. Đã xếp vào hàng đợi." });
                return BadRequest(new { success = false, message = "Check-in thất bại. Lỗi không xác định." });
            }
            catch (InvalidOperationException ex) // (Bắt các lỗi sai ngày, lỗi đã check-in rồi...)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Hiển thị danh sách khách đang chờ ở ngoài phòng khách (Dashboard Lễ tân)
        /// </summary>
        [HttpGet("queue")]
        public async Task<IActionResult> GetTodayQueue()
        {
            var queue = await _receptionistService.GetTodayQueueAsync(); // (Gọi hàm kéo danh sách hôm nay)
            return Ok(queue);
        }
    }
}
```

---

### PHẦN 2.2 - TẦNG SERVICE (GIẢI QUYẾT BÀI TOÁN CẤP TRÙNG SỐ THỨ TỰ BẰNG SEMAPHORE)

**Tệp:** `MyPetClinic.Application/Services/ReceptionistService.cs`

Trong thực tế, Phòng khám thú y lớn thường có 2-3 quầy Lễ tân hoạt động cùng lúc. Nếu 2 Lễ tân bấm nút "Check-in" cùng 1 phần ngàn giây, lệnh `maxQueueToday + 1` sẽ tính ra 2 số thứ tự giống hệt nhau (ví dụ cùng ra số 5). Điều này dẫn đến đánh nhau tranh chỗ. 
Giải pháp là `SemaphoreSlim`.

```csharp
namespace MyPetClinic.Application.Services
{
    public class ReceptionistService : IReceptionistService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        // TẠO CÁNH CỔNG BẢO VỆ (SEMAPHORE): (1, 1) nghĩa là "Chỉ cho phép 1 luồng đi qua cánh cửa này tại 1 thời điểm"
        private static readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);

        // ...
        
        public async Task<bool> CheckInAsync(CheckInRequestDto request)
        {
            // (1. TÌM LỊCH HẸN BẰNG MÃ QR HOẶC ID)
            Appointment? appointment = null;
            if (!string.IsNullOrEmpty(request.QrToken))
            {
                appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(a => a.QrToken == request.QrToken, a => a.Pet!);
            }
            
            if (appointment == null) throw new InvalidOperationException("Không tìm thấy Lịch hẹn.");

            // (2. KIỂM TRA ĐIỀU KIỆN CHẶN CÁC CA ĐÃ VÀO RỒI HOẶC BỊ HỦY)
            if (appointment.Status == "waiting" || appointment.Status == "in_progress" || appointment.Status == "ready_to_pay")
                throw new InvalidOperationException("Khách hàng này đã nằm trong hàng đợi rồi.");

            // (3. KIỂM TRA NGÀY KHÁM - Chống khách hàng đến sai ngày)
            var (todayStartUtc, todayEndUtc) = GetVietnamTodayUtcRange(); // (Hàm tự viết để đồng bộ múi giờ UTC và VN)
            if (appointment.AppointmentDate < todayStartUtc || appointment.AppointmentDate >= todayEndUtc)
            {
                throw new InvalidOperationException($"Lịch hẹn này dành cho ngày khác. Không thể Check-in hôm nay.");
            }

            // ... (Cập nhật cân nặng nếu có)

            // 4. KỸ THUẬT SEMAPHORE - CHỐNG CẤP TRÙNG SỐ THỨ TỰ (RACE CONDITION)
            await _queueSemaphore.WaitAsync(); // (YÊU CẦU LẤY CHÌA KHÓA: Nếu có Lễ tân khác đang cấp số, luồng này sẽ đứng đợi ở đây)
            try
            {
                // (Sau khi vào được trong cửa, yên tâm tìm Số Max hiện tại mà không sợ ai phá)
                var todayAppointments = await _unitOfWork.Appointments.FindAsync(a => 
                    a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc && a.QueueNumber > 0);
                
                // (Tìm số lớn nhất hiện tại. Ví dụ trong ngày đã có số 4)
                var maxQueueToday = todayAppointments.Any() ? todayAppointments.Max(a => (int?)a.QueueNumber) ?? 0 : 0;

                // (Cập nhật dữ liệu)
                appointment.Status = "waiting"; // (Chuyển thành Đang chờ)
                appointment.CheckInTime = DateTime.UtcNow; // (Ghi nhận thời gian đến phòng khám)
                appointment.QueueNumber = maxQueueToday + 1; // (Cấp số thứ tự = 4 + 1 = 5)
                appointment.IsEmergency = request.IsEmergency; // (Đánh dấu có phải Cấp cứu không)

                _unitOfWork.Appointments.Update(appointment);
                await _unitOfWork.SaveChangesAsync(); // (Lưu xuống Database)
            }
            finally
            {
                // (BẮT BUỘC PHẢI CÓ FINALLY)
                // (Dù code chạy thành công hay sập lỗi giữa chừng, thì cũng TRẢ LẠI CHÌA KHÓA để Lễ tân khác còn được dùng)
                _queueSemaphore.Release(); 
            }

            return true;
        }
```

**Giải thích chi tiết - Tinh hoa Đa luồng (Multi-threading):**
- **Tại sao dùng `static SemaphoreSlim`?** Vì Backend API xử lý song song hàng ngàn Request cùng lúc. Chữ `static` đảm bảo rằng cái Khóa (Semaphore) này tồn tại độc lập, duy nhất xuyên suốt vòng đời của Server. 
- **`WaitAsync()` và `Release()`:** Giống như một cái nhà vệ sinh chỉ có 1 phòng. Khi Lễ tân A vào (`WaitAsync`), cửa khóa lại. Lễ tân B bấm Check-in phải đứng chờ ngoài vòng lặp. Khi Lễ tân A cấp số xong và bước ra (`Release`), Lễ tân B mới được vào lấy số tiếp theo. Nó đảm bảo dãy số luôn là 1, 2, 3, 4, 5... không bao giờ có 2 số 5.

---

### PHẦN 2.3 - TẦNG SERVICE (THUẬT TOÁN SẮP XẾP ƯU TIÊN - EMERGENCY FIRST)

**Tệp:** `MyPetClinic.Application/Services/ReceptionistService.cs`

Sau khi mọi người đã có Số thứ tự, làm sao để màn hình hiển thị (Dashboard) của Lễ tân biết ai nên gọi vào trước, ai gọi vào sau?

```csharp
        public async Task<List<QueueItemDto>> GetTodayQueueAsync()
        {
            var (todayStartUtc, todayEndUtc) = GetVietnamTodayUtcRange();
            
            // Lấy tất cả ca còn active (waiting/in_progress/ready_to_pay) của ngày hôm nay
            var appointmentsList = await _unitOfWork.Appointments.FindWithIncludesAsync(
                a => (a.Status == "waiting" || a.Status == "in_progress" || a.Status == "ready_to_pay")
                     && (
                         (a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc) // (Lịch đặt trong hôm nay)
                         || (a.CheckInTime != null && a.CheckInTime >= todayStartUtc && a.CheckInTime < todayEndUtc) // (Lịch cũ dời qua nhưng check-in hôm nay)
                     ),
                a => a.Pet!, a => a.Customer!, a => a.Doctor!
            );

            // THUẬT TOÁN SẮP XẾP LINQ ĐA CẤP ĐỘ
            var appointments = appointmentsList
                .OrderByDescending(a => a.IsEmergency) // Ưu tiên 1: Ca cấp cứu bị tai nạn luôn nằm ĐẦU TIÊN (True đẩy lên trên False)
                .ThenBy(a => a.QueueNumber > 0 ? a.QueueNumber : int.MaxValue) // Ưu tiên 2: Ai có số nhỏ hơn (STT 1,2,3...) gọi trước. Ai chưa cấp số (int.MaxValue) vứt xuống chót.
                .ThenBy(a => a.CheckInTime) // Ưu tiên 3: Nếu số thứ tự bằng nhau, ai đến phòng khám bấm Check-in sớm hơn thì vào trước
                .ToList();

            // (Đóng gói thành List<QueueItemDto> trả về)
            return appointments.Select(a => new QueueItemDto
            {
                // ... (Map các dữ liệu ID, Name, Status trả về)
            }).ToList();
        }
```

**Giải thích chi tiết:**
- **OrderByDescending(IsEmergency):** Rất nhân văn! Kể cả bạn đang cầm Số thứ tự là số 1, nhưng có một chú chó bị tai nạn đưa vào cấp cứu (`IsEmergency = true`). Chú chó đó sẽ tự động nhảy lên đầu hàng mà không cần quan tâm đến số thứ tự.
- **ThenBy(QueueNumber):** Là lớp sắp xếp thứ 2. Sau khi xếp xong cấp cứu, những người còn lại sẽ tuân thủ nghiêm ngặt Số thứ tự (Số 1, 2, 3...) mà Semaphore cấp.
- **ThenBy(CheckInTime):** Dùng để chống lỗi và giải quyết triệt để tranh chấp nếu vì lý do nào đó 2 người bằng số.

---
*(Hết tài liệu đào tạo chuyên sâu Quản lý Hàng đợi Lễ tân - Đỉnh cao xử lý đa luồng Concurrency)*
