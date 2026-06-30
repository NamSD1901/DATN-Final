# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG ĐẶT LỊCH KHÁM CỦA KHÁCH HÀNG (CUSTOMER BOOKING) - BẢN FULL DEEP DIVE (COMBO GIẢI THÍCH KÉP)

> [!NOTE]
> Tài liệu số 4 này đi sâu vào "Trái tim" của hệ thống MyPetClinic: **Chức năng Đặt lịch khám dành cho Khách hàng**. 
> Đặc trưng của phần này là chứa rất nhiều **Quy tắc Nghiệp vụ (Business Rules)** khắt khe để chống spam, chống bom hàng (No-show) và giới hạn giờ giấc. 
> Tài liệu vẫn giữ nguyên phong cách **"Giải thích Kép"** truyền thống: Chú thích ngắn gọn `// (...)` nhúng trong code + Phân tích gạch đầu dòng chuyên sâu bên dưới.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Khách hàng tự đặt lịch hẹn (Customer Booking).
- **Mục đích:** Cho phép khách hàng chọn Thú cưng, chọn Dịch vụ, chọn Vắc-xin (nếu có) và chọn Ngày/Giờ khám.
- **Quy tắc kinh doanh (Business Rules):** 
  - Khách hàng PHẢI có số điện thoại mới được đặt lịch.
  - Phải đặt trước ít nhất 15 phút.
  - Chỉ được đặt trong giờ hành chính (08:00 - 20:00) và trừ giờ nghỉ trưa (12:00 - 13:30).
  - Khách bị cấm đặt lịch nếu có lịch sử "Bom hàng" (No-show) từ 3 lần trở lên.
  - Chống Spam: Không được đặt 2 lịch cho cùng 1 con thú cưng cách nhau dưới 2 tiếng.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG CONTROLLER (NGƯỜI GÁC CỔNG GIAO TIẾP VỚI CLIENT)

**Tệp:** `WebApi/Controllers/CustomerAppointmentController.cs`

File Controller này thiết kế riêng một API Endpoint chỉ dành cho Khách hàng đặt lịch, tách biệt hoàn toàn với API đặt lịch của Lễ tân.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    /// <summary>
    /// API dành riêng cho Khách hàng (customer role) để xem và quản lý lịch hẹn của họ.
    /// </summary>
    [Authorize(Roles = "customer")] // (CHỐT CHẶN 1: Chỉ user mang thẻ 'customer' mới được vào)
    [ApiController] // (Tự động bật tính năng validate DTO)
    [Route("api/my-appointments")] // (Đường dẫn API: /api/my-appointments)
    public class CustomerAppointmentController : ControllerBase
    {
        private readonly ICustomerAppointmentService _customerAppointmentService; // (Dịch vụ chuyên biệt cho khách)
        private readonly MyPetClinic.Application.Interfaces.Repositories.IUserRepository _userRepository; // (Kho User)

        public CustomerAppointmentController(
            ICustomerAppointmentService customerAppointmentService,
            MyPetClinic.Application.Interfaces.Repositories.IUserRepository userRepository) // (Tiêm phụ thuộc qua Constructor)
        {
            _customerAppointmentService = customerAppointmentService;
            _userRepository = userRepository;
        }

        // Hàm Phụ trợ: Lấy Mã KHÁCH HÀNG (Customer ID)
        private async Task<Guid> GetCurrentCustomerIdAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); // (Bóc mã User từ Token)
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _userRepository.GetUserByIdAsync(userId); // (Truy vấn DB lấy thông tin User)
                if (user != null && user.CustomerId.HasValue) // (Kiểm tra xem User này đã là Khách hàng chưa)
                {
                    return user.CustomerId.Value; // (Lấy ID Khách hàng)
                }
            }
            // (Nếu không có, ném thẳng lỗi từ chối truy cập)
            throw new UnauthorizedAccessException("Không tìm thấy thông tin khách hàng. Vui lòng xác thực hồ sơ trước.");
        }
```

**Giải thích chi tiết:**
- `[Authorize(Roles = "customer")]`: Đảm bảo chỉ khách hàng thao tác. Lễ tân, Bác sĩ không thể gọi nhầm sang API này.
- `GetCurrentCustomerIdAsync()`: Bước đầu tiên của mọi quy trình. Lấy căn cước (ID) của khách hàng từ Token một cách bí mật, không cho phép Client tự gửi lên để chống hack (lỗi bảo mật IDOR).

```csharp
        /// <summary>
        /// Khách hàng tự đặt lịch hẹn cho thú cưng của mình.
        /// </summary>
        [HttpPost] // (Lắng nghe Request POST đẩy dữ liệu lên)
        public async Task<IActionResult> BookAppointment([FromBody] CustomerBookingDto dto) // (Hàm: Đặt lịch)
        {
            if (!ModelState.IsValid) // (Hệ thống tự soi lỗi thiếu dữ liệu trong thùng hàng DTO)
                return BadRequest(ModelState);

            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); // (Lấy ID của Tài khoản)
                if (!Guid.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized("Không tìm thấy thông tin đăng nhập.");
                }

                var customerId = await GetCurrentCustomerIdAsync(); // (Lấy ID của Khách hàng thực thể)

                // (Giao toàn bộ Thùng hàng DTO, ID Khách và ID User cho Service xử lý phần khó nhất)
                var appointmentId = await _customerAppointmentService.BookAppointmentAsync(dto, customerId, userId);
                
                // (Nếu Service chạy mượt mà không văng lỗi, trả về lời nhắn thành công kèm mã lịch hẹn)
                return Ok(new { success = true, message = "Đặt lịch hẹn thành công! Chúng tôi sẽ xác nhận sớm.", id = appointmentId });
            }
            catch (Exception ex)
            {
                // (Nếu Service bắt được bất kỳ vi phạm Quy tắc nào, nó sẽ ném Ex ra đây để trả thành mã lỗi 400 cho Client)
                return BadRequest(new { success = false, message = "Đặt lịch thất bại: " + ex.Message });
            }
        }
```

**Giải thích chi tiết:**
- Controller ở đây đóng vai trò rất "nhẹ nhàng". Nó chỉ có nhiệm vụ Bóc thư (Nhận request), Kiểm tra thẻ CMND (Lấy ID), và Gọi điện nhờ Chuyên gia (Service) xử lý. Nếu Chuyên gia báo lỗi thì thông báo lại cho khách.

---

### PHẦN 2.2 - TẦNG SERVICE (NHỮNG QUY TẮC KINH DOANH THÉP)

**Tệp:** `MyPetClinic.Application/Services/CustomerAppointmentService.cs`

Đây là nơi chứa toàn bộ bộ não của chức năng Đặt lịch. Nó chặn đứng mọi nỗ lực phá hoại, spam hay thao tác sai giờ của khách hàng.

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Application.Services
{
    public class CustomerAppointmentService : ICustomerAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork; // (Trạm kiểm soát DB chung)
        private readonly IAppointmentService _appointmentService; // (Dịch vụ Lõi quản lý Lịch hẹn)

        public CustomerAppointmentService(IUnitOfWork unitOfWork, IAppointmentService appointmentService)
        {
            _unitOfWork = unitOfWork;
            _appointmentService = appointmentService;
        }

        public async Task<long> BookAppointmentAsync(MyPetClinic.Application.DTOs.CustomerBookingDto dto, Guid customerId, Guid userId)
        {
            // QUY TẮC 1: TỪ CHỐI CẤP CỨU ĐẶT ONLINE
            if (dto.IsEmergency) // (Nếu khách tick vào ô Cấp cứu)
            {
                // (Văng lỗi chặn lại ngay. Tránh việc chó mèo đang nguy kịch mà ngồi đợi duyệt web)
                throw new InvalidOperationException("TRƯỜNG HỢP CẤP CỨU: Vui lòng KHÔNG đặt lịch online. Hãy đưa bé đến phòng khám ngay lập tức hoặc gọi Hotline khẩn cấp.");
            }

            // QUY TẮC 2: BẮT BUỘC PHẢI CÓ SỐ ĐIỆN THOẠI
            var customers = await _unitOfWork.Customers.FindAsync(c => c.Id == customerId && c.DeletedAt == null);
            var customer = customers.FirstOrDefault();
            if (customer != null && string.IsNullOrWhiteSpace(customer.Phone)) // (Kiểm tra ô SDT có trống không)
            {
                throw new InvalidOperationException("MISSING_PHONE: Tài khoản của bạn chưa có số điện thoại. Vui lòng cập nhật số điện thoại trong phần Hồ sơ để chúng tôi có thể liên hệ xác nhận.");
            }

            var appointmentDate = dto.AppointmentDate ?? DateTime.Now; // (Lấy thời gian đặt, nếu để trống thì lấy giờ hiện tại)
            
            // QUY TẮC 3: LEAD TIME (THỜI GIAN CHUẨN BỊ)
            if (appointmentDate < DateTime.Now.AddMinutes(15)) // (So sánh giờ chọn với (Giờ hiện tại + 15 phút))
            {
                throw new InvalidOperationException("Vui lòng đặt lịch trước ít nhất 15 phút để chúng tôi có sự chuẩn bị tốt nhất.");
            }
```

**Giải thích chi tiết Khối Quy tắc 1-2-3:**
- `throw new InvalidOperationException(...)`: Là cách Service "kêu la" khi phát hiện khách hàng vi phạm luật. Lỗi này sẽ bay thẳng lên Controller và đập vào màn hình điện thoại của khách hàng.
- **Quy tắc 3 (Lead Time):** Phòng khám thú y cần thời gian chuẩn bị phòng. Nếu bây giờ là 10:00, khách hàng không thể chọn đặt lịch lúc 10:05. Họ bắt buộc phải chọn từ 10:15 trở đi. Dòng `DateTime.Now.AddMinutes(15)` chính là để chặn việc đặt quá sát giờ.

```csharp
            // QUY TẮC 4: GIỜ HÀNH CHÍNH & GIỜ NGHỈ TRƯA
            var hour = appointmentDate.Hour; // (Lấy số Giờ trong cấu trúc Thời gian. VD: 13:45 thì lấy số 13)
            
            // (Chặn giờ ngoài hành chính: Trước 8h sáng hoặc từ 20h đêm trở đi)
            if (hour < 8 || hour >= 20)
            {
                throw new InvalidOperationException("Phòng khám đóng cửa vào thời gian này. Vui lòng chọn khung giờ trong giờ hành chính (08:00 - 20:00).");
            }
            
            // (Chặn giờ nghỉ trưa: Trọn vẹn 12h, hoặc 13h nhưng số phút < 30)
            if (hour == 12 || (hour == 13 && appointmentDate.Minute < 30))
            {
                throw new InvalidOperationException("Phòng khám đang trong giờ nghỉ trưa (12:00 - 13:30). Vui lòng chọn khung giờ khác.");
            }
```

**Giải thích chi tiết Khối Quy tắc 4 (Giờ làm việc):**
- Thuật toán khóa giờ nghỉ trưa ở đây rất tinh tế: Khung 12:00 - 13:30.
- `hour == 12`: Chặn toàn bộ các mốc từ 12:00 đến 12:59.
- `(hour == 13 && appointmentDate.Minute < 30)`: Chặn tiếp các mốc từ 13:00 đến 13:29. Từ 13:30 trở đi khách hàng mới đặt được.

```csharp
            // QUY TẮC 5: CHỐNG "BOM HÀNG" (NO-SHOW LIMIT)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30); // (Mốc thời gian 30 ngày trước tính từ hôm nay)
            
            // (Quét tìm tất cả lịch hẹn của khách này trong 30 ngày qua)
            var recentAppointments = await _unitOfWork.Appointments.FindAsync(
                a => a.CustomerId == customerId && a.AppointmentDate >= thirtyDaysAgo);
            
            // (Đếm số lần khách hàng bị đánh dấu là Vắng mặt / Bom hàng)
            var noShowCount = recentAppointments.Count(a => a.Status.ToLower() == "no_show");

            if (noShowCount >= 3) // (Nếu đã bom hàng 3 lần trong tháng)
            {
                throw new InvalidOperationException("Tài khoản của bạn tạm thời bị hạn chế đặt lịch online do lịch sử vắng mặt nhiều lần. Vui lòng gọi trực tiếp Hotline để được hỗ trợ.");
            }
```

**Giải thích chi tiết Khối Quy tắc 5 (Chống No-Show):**
- Đây là tính năng Blacklist tự động. Hệ thống quét lịch sử 1 tháng gần nhất, đếm xem bao nhiêu lần nhân viên lễ tân đánh dấu lịch của ông khách này là `no_show`.
- Nếu >= 3 lần, tước quyền tự đặt lịch. Buộc họ phải gọi điện để xác nhận, tránh chiếm dụng chỗ của khách hàng chân chính khác.

```csharp
            // QUY TẮC 6: CHỐNG SPAM (CÙNG 1 THÚ CƯNG TRONG CÙNG 1 NGÀY)
            // (Tìm các lịch hẹn của con Thú cưng này, đặt trong ngày hôm nay, và chưa bị hủy)
            var todayAppointments = await _unitOfWork.Appointments.FindAsync(
                a => a.CustomerId == customerId && a.PetId == dto.PetId 
                  && a.AppointmentDate.Date == appointmentDate.Date 
                  && a.Status != "cancelled" && a.Status != "no_show");
            
            // (Duyệt mảng tìm xem có cái lịch nào cách cái giờ sắp đặt dưới 2 tiếng đồng hồ không)
            if (todayAppointments.Any(a => Math.Abs((a.AppointmentDate - appointmentDate).TotalHours) < 2))
            {
                throw new InvalidOperationException("Bé cưng đã có lịch hẹn quá sát với thời gian này. Bạn không thể đặt thêm lịch liên tiếp (chống spam).");
            }
```

**Giải thích chi tiết Khối Quy tắc 6 (Chống Spam):**
- Có những khách hàng thích "click nhầm" tạo ra 4-5 cái lịch khám cho con Mèo A vào lúc 14h, 14h15, 14h30.
- Lệnh `Math.Abs((a.AppointmentDate - appointmentDate).TotalHours) < 2` sẽ tính khoảng cách tuyệt đối giữa Lịch Cũ và Lịch Sắp Tạo. Nếu khoảng cách < 2 giờ (Ví dụ tạo lúc 14h và 15h) thì chặn ngay lập tức. Khách muốn khám 2 dịch vụ thì phải nhờ Bác sĩ ghép vào chứ không được book tràn lan ra hệ thống.

```csharp
            // GIAI ĐOẠN CUỐI: BÀN GIAO CHO DỊCH VỤ LÕI TẠO LỊCH (CORE SERVICE)
            var createDto = new MyPetClinic.Application.DTOs.AppointmentCreateDto // (Chuyển đổi thùng hàng)
            {
                CustomerId = customerId,
                PetId = dto.PetId,
                DoctorId = Guid.Empty, // (KHÁCH HÀNG KHÔNG ĐƯỢC CHỌN BÁC SĨ: Bắt buộc để trống để Lễ tân tự chia ca)
                ServiceId = dto.ServiceId,
                AppointmentDate = dto.AppointmentDate,
                Symptom = dto.Symptom ?? string.Empty,
                Note = dto.Note,
                VaccineId = dto.VaccineId
            };

            // (Gọi hàm CreateAppointmentAsync của Core Service để lưu DB, hàm lõi này lại có các thuật toán chặn trùng lịch chuyên sâu hơn nữa)
            var appointmentId = await _appointmentService.CreateAppointmentAsync(createDto, userId);

            return appointmentId; // (Trả mã lịch hẹn về thành công)
        }
    }
}
```

**Giải thích chi tiết:**
- Khách hàng không được phép chọn Bác sĩ cụ thể (`DoctorId = Guid.Empty`). Đây là chiến lược phân bổ nguồn lực của phòng khám. Đặt lịch online chỉ giành chỗ (Slot), Lễ tân sẽ nhìn vào lịch trực để xếp bác sĩ sau.
- Gọi hàm lõi `_appointmentService.CreateAppointmentAsync`. Dịch vụ lõi này ở bên trong sẽ tiếp tục sinh ra **Mã QR Code** và lưu lịch hẹn xuống Database, sau đó bắn Thông báo (Notification) "Đặt lịch thành công" về điện thoại cho khách hàng.

---

### PHẦN 2.3 - XEM DANH SÁCH LỊCH HẸN (ĐÃ QUA & ĐANG CHỜ)

**Tệp:** `MyPetClinic.Application/Services/AppointmentService.cs`

Sau khi đặt lịch xong, khách hàng có nhu cầu vào mục "Lịch hẹn của tôi" để xem lại các lịch sắp tới hoặc lịch sử khám cũ. Hàm này được thiết kế để phân trang (Paginated) và lọc theo trạng thái (Status).

```csharp
        public async Task<PaginatedResultDto<AppointmentDetailDto>> GetCustomerAppointmentsPaginatedAsync(Guid customerId, string? status, int page, int pageSize)
        {
            // 1. Dựng khung câu truy vấn (Chưa chạy SQL vội)
            var query = _unitOfWork.Appointments.Query()
                .Where(a => a.CustomerId == customerId); // (Bắt buộc lọc theo Khách hàng hiện tại - Chống IDOR)

            // 2. Lọc theo trạng thái (Tab Đang chờ / Tab Đã qua)
            if (!string.IsNullOrEmpty(status) && status != "all") // (Nếu khách có chọn bộ lọc)
            {
                var lowerStatus = status.ToLower();
                query = query.Where(a => a.Status.ToLower() == lowerStatus); // (Gắn thêm điều kiện WHERE Status = ...)
            }

            // 3. Đếm tổng số lượng (Phục vụ cho việc vẽ nút Phân trang 1, 2, 3... trên giao diện)
            var totalCount = query.Count(); 

            // 4. Lấy dữ liệu chi tiết của đúng trang hiện tại
            var rawList = query
                .OrderByDescending(a => a.AppointmentDate) // (Sắp xếp ưu tiên: Ngày gần nhất lên đầu)
                .ThenByDescending(a => a.StartTime) // (Cùng ngày thì sắp xếp giờ muộn nhất lên đầu)
                .Select(a => new // (Dùng Select vô danh để JOIN bảng trực tiếp bằng LINQ to SQL)
                {
                    a.Id,
                    a.PetId,
                    PetName = a.Pet != null ? a.Pet.Name : null, // (Kéo theo tên Pet)
                    // ... (Các trường dữ liệu khác)
                    a.AppointmentDate,
                    a.StartTime,
                    a.Status,
                    a.QrToken, // (Kéo theo mã QR Code để xuất trình tại quầy)
                    InvoiceId = a.Invoice != null ? (long?)a.Invoice.Id : null // (Xem lịch hẹn này đã thanh toán hóa đơn chưa)
                })
                .Skip((page - 1) * pageSize) // (Bỏ qua các dữ liệu của trang trước)
                .Take(pageSize) // (Chỉ lấy đúng số lượng của trang hiện tại - VD: 10 dòng)
                .ToList(); // (CHỐT HẠ: Lúc này Entity Framework mới bắn câu lệnh SQL xuống Database)

            // 5. Đóng gói kết quả
            // ... (Đoạn ánh xạ từ rawList sang AppointmentDetailDto)
            
            // (Đóng gói vào cấu trúc Phân trang để trả về Frontend vẽ UI)
            var result = new PaginatedResultDto<AppointmentDetailDto>(mapped, totalCount, page, pageSize);
            return await Task.FromResult(result);
        }
```

**Giải thích chi tiết:**
- **Tab "Đang chờ" (Upcoming) / Tab "Đã qua" (Past):** Trên Frontend VueJS, khi khách chuyển Tab, nó sẽ gọi API này và truyền tham số `status` (ví dụ: `pending`, `completed`, `cancelled`). Khối lệnh số 2 sẽ tự động móc nối thêm điều kiện `WHERE` vào SQL một cách linh hoạt.
- **Kỹ thuật Lazy Loading (Trì hoãn thực thi):** Điểm thông minh ở khối lệnh 1 và 2 là nó trả về dạng `IQueryable` (chỉ là câu lệnh nháp). Kể cả khi nối thêm `.Where` nó vẫn chưa chạy SQL. Chỉ đến khi đụng vào `.Count()` hoặc `.ToList()` ở cuối khối 4, nó mới gom tất cả điều kiện lại và bắn đúng 1 câu lệnh duy nhất xuống SQL Server. Điều này giúp tối ưu hóa hiệu năng cực cao.
- `Skip` và `Take`: Đây là công thức kinh điển của Phân trang (Pagination). Nếu bạn đang xem Trang 3, mỗi trang 10 dòng. Thì `Skip((3-1)*10)` sẽ là bỏ qua 20 dòng đầu tiên, và `Take(10)` là bốc lấy 10 dòng tiếp theo (từ 21-30).

---

### PHẦN 2.4 - THUẬT TOÁN TỰ ĐỘNG PHÂN CÔNG BÁC SĨ (LOAD BALANCING)

**Tệp:** `MyPetClinic.Application/Services/AppointmentService.cs`

Như đã đề cập ở trên, khách hàng không được chọn bác sĩ (`DoctorId = Guid.Empty`). Nhiệm vụ phân công bác sĩ được giao phó cho một hàm phụ trợ cực kỳ thông minh mang tên `ResolveAndValidateDoctorId`. Thuật toán này hoạt động như một hệ thống Cân bằng tải (Load Balancer).

```csharp
        private Guid ResolveAndValidateDoctorId(Guid requestedDoctorId, DateTime appointmentDate, long serviceId)
        {
            var finalDoctorId = requestedDoctorId;
            var targetDateStart = appointmentDate.Date;

            if (finalDoctorId == Guid.Empty) // (Vì khách hàng truyền vào Guid.Empty nên sẽ lọt vào khối IF này)
            {
                var appointmentTime = appointmentDate.TimeOfDay;

                // BƯỚC 1: LỌC DANH SÁCH BÁC SĨ TRỰC VÀ ĐÚNG CHUYÊN MÔN
                // (Đoạn code trên: Xác định xem Dịch vụ thuộc nhóm "Khám bệnh" hay "Tiêm phòng" để gom nhóm Bác sĩ chuyên khoa tương ứng)
                
                // (Tìm tất cả các bác sĩ được xếp lịch trực trong ngày này)
                var doctorsWithSchedules = _unitOfWork.DoctorSchedules.Query()
                    .Where(s => s.WorkDate == targetDateStart && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true)
                    .ToList();

                // (Lọc tiếp: Bác sĩ trực nhưng khung giờ trực phải bao trùm lên cái giờ mà khách muốn khám)
                List<Guid> doctorsList = doctorsWithSchedules
                    .Where(s => appointmentTime >= s.StartTime && appointmentTime + TimeSpan.FromMinutes(30) <= s.EndTime)
                    .Select(s => s.DoctorId)
                    .Distinct()
                    .ToList();

                if (!doctorsList.Any())
                    throw new InvalidOperationException("Hệ thống hiện không có bác sĩ nào đang trực vào khung giờ này!");

                // BƯỚC 2: KIỂM TRA "DOUBLE-BOOKING" - TÌM NHỮNG NGƯỜI THỰC SỰ RẢNH
                // (Trong số các bác sĩ trực, ai là người KHÔNG bị vướng lịch khám khác trong vòng +/- 30 phút?)
                var allAptsForDoctors = _unitOfWork.Appointments.Query()
                    .Where(a => a.Status != "cancelled" && a.AppointmentDate == appointmentDate.Date && doctorsList.Contains(a.DoctorId))
                    .Select(a => new { a.DoctorId, a.StartTime })
                    .ToList();

                // (Thuật toán quét xem bác sĩ nào bị kẹt lịch sát giờ)
                var busyDoctorIds = allAptsForDoctors
                    .Where(a => Math.Abs((a.StartTime - appointmentTime).TotalMinutes) < 30)
                    .Select(a => a.DoctorId)
                    .Distinct()
                    .ToList();

                // (Dùng phép trừ Tập hợp - LINQ Except: Danh sách Rảnh = Toàn bộ trực - Danh sách Bận)
                var availableDoctors = doctorsList.Except(busyDoctorIds).ToList();

                if (!availableDoctors.Any())
                    throw new InvalidOperationException("Tất cả bác sĩ trực khung giờ này đã kín lịch. Vui lòng chọn khung giờ khác.");

                // BƯỚC 3: THUẬT TOÁN "CÂN BẰNG TẢI" (TÌM NGƯỜI ÍT VIỆC NHẤT)
                // (Đếm số lượng ca khám trong ngày của từng bác sĩ rảnh)
                var doctorApptCounts = _unitOfWork.Appointments.Query()
                    .Where(a => a.AppointmentDate >= targetDateStart && a.AppointmentDate < targetDateStart.AddDays(1) 
                             && a.Status != "cancelled" && availableDoctors.Contains(a.DoctorId))
                    .GroupBy(a => a.DoctorId)
                    .Select(g => new { DoctorId = g.Key, Count = g.Count() }) // (Gom nhóm và đếm Count)
                    .ToList();

                // (Sắp xếp tăng dần theo số lượng ca khám, và chọn người đứng ĐẦU TIÊN - tức là người rảnh nhất)
                var doctorWithCount = availableDoctors
                    .Select(id => new { DoctorId = id, Count = doctorApptCounts.FirstOrDefault(c => c.DoctorId == id)?.Count ?? 0 })
                    .OrderBy(x => x.Count) // (Sắp xếp từ ít nhất đến nhiều nhất)
                    .First(); // (Chốt đơn!)

                finalDoctorId = doctorWithCount.DoctorId; // (Gán mã bác sĩ được chọn vào biến cuối)
            }
            
            // ... (Các dòng code bảo mật kiểm tra trùng lặp lần cuối)

            return finalDoctorId; // (Bàn giao lại Bác sĩ rảnh nhất cho Controller để lưu xuống DB)
        }
```

**Giải thích chi tiết:**
- Đây là một đoạn code mẫu mực về **Phân bổ tài nguyên (Resource Allocation)** trong các hệ thống Booking:
- **Bước 1 (Lọc bao phủ):** Rõ ràng khách book 14h thì chỉ lấy những bác sĩ có ca trực từ sáng đến chiều (ví dụ: 08:00 - 16:00). Nếu ca trực là 16:00 - 20:00 thì loại ngay lập tức.
- **Bước 2 (Lọc trống lịch - Double booking check):** Bác sĩ A trực ca sáng đến chiều, nhưng lúc 14h15 bác A đã có khách rồi. Nếu khách này book 14h00, thì 2 lịch cách nhau có 15 phút `< 30 phút` (bị đè lên nhau). Hàm `Except` sẽ gạch tên bác sĩ A ra khỏi danh sách đề cử.
- **Bước 3 (Load Balancing):** Còn lại bác sĩ B (đã khám 5 ca hôm nay) và bác sĩ C (mới khám 1 ca). Lệnh `GroupBy` và `OrderBy` sẽ ưu tiên gọi tên Bác sĩ C. Tính năng này giúp các bác sĩ không bị tình trạng "người làm sấp mặt, người ngồi chơi xơi nước", tối ưu hóa nguồn lực phòng khám.

---
*(Hết tài liệu đào tạo chuyên sâu Đặt lịch Khách hàng - Đỉnh cao xử lý nghiệp vụ C#)*
