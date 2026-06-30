# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG QUẢN LÝ THÚ CƯNG (MY PETS) - BẢN FULL DEEP DIVE (COMBO GIẢI THÍCH KÉP)

> [!NOTE]
> Tài liệu này được thiết kế với phương pháp "Giải thích Kép" (Double Explanation) tối ưu nhất cho việc học:
> 1. Trực tiếp chèn chú thích tiếng Việt ngắn gọn ngay bên cạnh code `// (...)` để bạn đọc lướt qua là hiểu biến đó/hàm đó làm gì.
> 2. Kèm theo phần **Giải thích chi tiết** nằm rành mạch bên dưới mỗi đoạn code để mổ xẻ sâu vào thuật toán, luồng đi và nguyên lý bảo mật.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Thú cưng của Khách hàng (My Pets)
- **Mục đích:** Cho phép khách hàng xem danh sách thú cưng, thêm mới, cập nhật thông tin, xóa (xóa mềm), tải ảnh đại diện và xem các lịch sử y tế (Bệnh án, Lịch tiêm phòng, Lịch hẹn, Đơn thuốc).
- **Người sử dụng:** Khách hàng đã đăng nhập (Role: `customer`).
- **Tầm quan trọng:** Thú cưng là trung tâm của hệ thống MyPetClinic. Mọi hoạt động nghiệp vụ phía sau đều xoay quanh đối tượng này.

---

## 2. Quy trình nghiệp vụ (Business Flow)
- **Thêm mới:** Đăng nhập -> "Thú cưng của tôi" -> Bấm "Thêm mới" -> Điền Tên, Giống loài -> Lưu -> Gắn vào tài khoản khách hàng.
- **Tương tác:** Chọn 1 Thú cưng -> Cập nhật thông tin / Tải ảnh / Xóa / Xem Bệnh án. Hệ thống luôn đối chiếu `CustomerId` từ Token để chặn quyền truy cập trái phép.

---

## 3. Luồng xử lý trong source code
Giao diện Client -> Gửi Request kèm JWT Token -> `MyPetsController` (Chặn quyền & Lấy ID khách hàng) -> `PetService` (Xử lý nghiệp vụ & Bảo mật cấp 2) -> `PetRepository` & `UnitOfWork` -> Database SQL Server -> Trả dữ liệu JSON về.

---

## 4. CÁC TỆP TIN THUỘC CHỨC NĂNG NÀY
1. **Entity:** `MyPetClinic.Domain/Entities/Pet.cs`
2. **DTO:** `CreatePetDto.cs`, `UpdatePetDto.cs`, `PetDto.cs`
3. **Controller:** `WebApi/Controllers/MyPetsController.cs`
4. **Service:** `MyPetClinic.Application/Services/PetService.cs`

---

## 5. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 5.1 - PHÂN TÍCH ENTITY (LỚP ĐẠI DIỆN CƠ SỞ DỮ LIỆU)

**> KHÁI NIỆM:** Entity là bản thiết kế của cái Bảng trong Database. Nó định nghĩa các cột và các mối quan hệ (Khóa ngoại). EF Core sẽ đọc file này để tạo ra Database.

**Tệp:** `MyPetClinic.Domain/Entities/Pet.cs`

```csharp
using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Pet // (Thú cưng)
    {
        public long Id { get; set; } // (Mã thú cưng - Khóa chính tự động tăng kiểu BIGINT)
        public Guid CustomerId { get; set; } // (Mã Chủ sở hữu - Khóa ngoại. BẮT BUỘC CÓ)
        public string? Name { get; set; } // (Tên thú cưng. Có thể Null)
        public string? Species { get; set; } // (Loài: Chó, Mèo...)
        public string? Breed { get; set; } // (Giống: Corgi, Poodle...)
        public short? Gender { get; set; } // (Giới tính: 0 đực, 1 cái)
        public DateTime? BirthDate { get; set; } // (Ngày sinh)
        public decimal? Weight { get; set; } // (Cân nặng)
        public string? Color { get; set; } // (Màu lông)
        public string? BloodType { get; set; } // (Nhóm máu)
        public bool? Sterilized { get; set; } = false; // (Đã triệt sản chưa? Mặc định False)
        public string? MicrochipCode { get; set; } // (Mã Microchip)
        public string? AllergyNote { get; set; } // (Ghi chú Dị ứng)
        public string? Avatar { get; set; } // (Ảnh đại diện - Link URL)
        public string? ChronicDisease { get; set; } // (Bệnh mãn tính)
        public string? CurrentDiet { get; set; } // (Chế độ ăn hiện tại)
        
        public bool IsDeceased { get; set; } = false; // (Đã qua đời? Mặc định False)
        public bool IsAggressive { get; set; } = false; // (Cờ báo thú cưng hung dữ)
        
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow; // (Ngày tạo - Lấy giờ chuẩn quốc tế UTC)
        public DateTime? DeletedAt { get; set; } // (Ngày xóa - Cột này dùng để đánh dấu Soft Delete)

        // Các thuộc tính điều hướng (Navigation Properties) để code C# có thể JOIN tự động sang bảng khác
        public Customer? Customer { get; set; } // (Khách hàng làm chủ)
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>(); // (Danh sách Lịch hẹn của pet)
        public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>(); // (Danh sách Lịch sử tiêm)
    }
}
```

**Giải thích chi tiết:**
- `public long Id`: Mã thú cưng - Khóa chính tự động tăng kiểu `BIGINT`.
- `public Guid CustomerId`: Mã Chủ sở hữu - Khóa ngoại. BẮT BUỘC CÓ. Thú cưng không thể vô chủ.
- `string?`: Dấu `?` nghĩa là cho phép Null. Các trường như `Name` (Tên), `Species` (Loài), `Breed` (Giống) có thể không cần điền ngay trong Database.
- `public bool IsDeceased { get; set; } = false;`: Cờ đánh dấu thú cưng đã qua đời. Mặc định là `false`. Nếu bật thành `true`, hệ thống sẽ tự động hủy lịch khám tương lai (sẽ thấy ở file Service).
- `public DateTime? DeletedAt { get; set; }`: Ngày xóa. Cột này dùng để đánh dấu Soft Delete (Xóa mềm).

---

### PHẦN 5.2 - PHÂN TÍCH CÁC DTO (DATA TRANSFER OBJECT)

**> KHÁI NIỆM:** DTO giúp hứng dữ liệu từ Client gửi lên, lọc bỏ các trường nhạy cảm và kiểm tra dữ liệu đầu vào (Validation) ngay tại cửa.

**Tệp:** `MyPetClinic.Application/DTOs/CreatePetDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations; // (Thư viện chứa các Thẻ Validation)

namespace MyPetClinic.Application.DTOs
{
    public class CreatePetDto // (Đối tượng Dữ liệu Tạo mới Thú cưng)
    {
        [Required(ErrorMessage = "Vui lòng nhập tên thú cưng")] // (Thẻ BẮT BUỘC. Trống là báo lỗi 400 ngay)
        public string? Name { get; set; } 
        
        [Required(ErrorMessage = "Vui lòng chọn giống loài")]
        public string? Species { get; set; }
        
        // Các trường bên dưới không có thẻ [Required], nghĩa là Khách hàng được quyền bỏ trống
        public string? Breed { get; set; }
        public short? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public decimal? Weight { get; set; }
        public string? Color { get; set; }
        public string? BloodType { get; set; }
        public bool Sterilized { get; set; }
        public string? MicrochipCode { get; set; }
        public string? AllergyNote { get; set; }
        public string? Avatar { get; set; }
    }
}
```

**Giải thích chi tiết:**
- `[Required(ErrorMessage = ...)]`: Thẻ bắt buộc. Khách hàng bắt buộc phải điền Tên (`Name`) và Giống loài (`Species`). Nếu Client gửi JSON lên mà thiếu 2 trường này, hệ thống tự động văng lỗi 400 BadRequest.
- **Tại sao không có trường CustomerId ở đây?** Vì ID của chủ sở hữu sẽ được trích xuất ngầm từ JWT Token của phiên đăng nhập hiện tại, tuyệt đối không cho phép gửi ID từ Client lên để đề phòng hacker thay đổi ID (lỗi IDOR).

---

### PHẦN 5.3 - PHÂN TÍCH CONTROLLER (BỘ ĐIỀU HƯỚNG GIAO TIẾP)

**> KHÁI NIỆM:** Controller là Trạm thu phát sóng. Nó nhận Request, kiểm tra Token, bóc tách JSON thành DTO, rồi điều phối (gọi Service) ra làm việc. Nó KHÔNG chứa logic kinh doanh.

**Tệp:** `WebApi/Controllers/MyPetsController.cs`

```csharp
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;

namespace WebApi.Controllers
{
    [Authorize(Roles = "customer")] // (YÊU CẦU QUYỀN KHÁCH HÀNG: Chỉ User có role 'customer' mới được vào)
    [ApiController] // (Kích hoạt tính năng kiểm tra lỗi DTO tự động)
    [Route("api/[controller]")] // (Tự động biến URL thành: /api/mypets)
    public class MyPetsController : ControllerBase
    {
        private readonly IPetService _petService; // (Dịch vụ Thú cưng)
        private readonly MyPetClinic.Application.Interfaces.Repositories.IUserRepository _userRepository; // (Kho dữ liệu Người dùng)

        // (CONSTRUCTOR - TIÊM PHỤ THUỘC / DEPENDENCY INJECTION)
        public MyPetsController(
            IPetService petService,
            MyPetClinic.Application.Interfaces.Repositories.IUserRepository userRepository)
        {
            _petService = petService;
            _userRepository = userRepository;
        }
```

**Giải thích chi tiết:**
- `[Authorize(Roles = "customer")]`: Chỉ cho phép người dùng có Role là Khách hàng (`customer`) mới được truy cập các API trong file này. Bác sĩ hay nhân viên gọi sẽ bị lỗi 403.
- `public MyPetsController(...)`: Sử dụng Dependency Injection (Tiêm phụ thuộc) để đưa các Service (Dịch vụ) vào sử dụng mà không cần dùng từ khóa `new`.

```csharp
        // Hàm Phụ trợ 1: Lấy Mã User từ Token
        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); // (Trích xuất ID từ JWT Token)
            if (Guid.TryParse(userIdStr, out var userId))
            {
                return userId; // (Trả về thành công)
            }
            throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng."); // (Lỗi văng ra nếu token giả mạo)
        }

        // Hàm Phụ trợ 2: Lấy Mã KHÁCH HÀNG (Customer ID) thực sự
        private async Task<Guid?> GetCurrentCustomerIdAsync()
        {
            var userId = GetCurrentUserId(); // (Gọi hàm 1 lấy User ID)
            var user = await _userRepository.GetUserByIdAsync(userId); // (Gọi DB lấy thông tin User)
            if (user != null && user.CustomerId.HasValue) // (Kiểm tra xem User này đã khởi tạo CustomerProfile chưa)
            {
                return user.CustomerId.Value; // (Trả về CustomerId)
            }
            return null; // (Nếu chưa cập nhật SDT tạo hồ sơ KH, trả về NULL)
        }
```

**Giải thích chi tiết:**
- `GetCurrentUserId()`: Trích xuất chuỗi User ID nằm ẩn bên trong JWT Token. Bắt buộc phải lấy từ Token để chống lộ lọt dữ liệu.
- `GetCurrentCustomerIdAsync()`: Lấy mã ID của Khách hàng. Trong hệ thống MyPetClinic, bảng `Users` và `Customers` tách biệt. Một tài khoản vừa tạo có thể chưa có hồ sơ Khách hàng (do chưa cập nhật Số điện thoại). Hàm này sẽ kiểm tra xem `user.CustomerId` đã tồn tại chưa.

```csharp
        // HÀM 1: LẤY DANH SÁCH THÚ CƯNG CỦA TÔI
        [HttpGet] // (Yêu cầu HTTP GET)
        public async Task<IActionResult> GetMyPets() // (Hàm: Lấy Thú cưng của tôi)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync(); // (Lấy mã KH)
                if (customerId == null)
                {
                    return Ok(new List<PetDto>()); // (Nếu chưa có hồ sơ KH, coi như chưa có pet, trả mảng rỗng [])
                }
                var pets = await _petService.GetMyPetsAsync(customerId.Value); // (Nhờ Service gọi DB lấy list Pet)
                return Ok(pets); // (Đóng gói list Pet thành JSON gửi về Client)
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); // (Lỗi thì trả 400 Bad Request)
            }
        }
```

**Giải thích chi tiết:**
- `[HttpGet]`: Lắng nghe Request gọi danh sách.
- `if (customerId == null)`: Nếu tài khoản này chưa đăng ký thông tin khách hàng, thì trả về một danh sách rỗng `[]` thay vì văng lỗi, giúp Frontend dễ hiển thị UI hơn.

```csharp
        // HÀM 2: THÊM MỚI THÚ CƯNG
        [HttpPost] // (Yêu cầu HTTP POST)
        public async Task<IActionResult> CreatePet([FromBody] CreatePetDto dto) // (Hàm: Tạo Thú cưng)
        {
            if (!ModelState.IsValid) // (Kích hoạt kiểm tra các thẻ [Required] trong DTO)
            {
                return BadRequest(ModelState); // (Sai thì văng lỗi ngay)
            }

            try
            {
                var customerId = await GetCurrentCustomerIdAsync(); // (Lấy mã KH)
                if (customerId == null)
                {
                    // (Lỗi: Khách chưa khai báo Số điện thoại thì cấm thêm Pet.)
                    return BadRequest(new { success = false, message = "Bạn cần cập nhật hồ sơ khách hàng trước khi thêm thú cưng. Vui lòng xác thực số điện thoại." });
                }
                
                await _petService.AddPetAsync(dto, customerId.Value); // (Đẩy DTO và Mã KH xuống Service để thêm vào DB)
                return Ok(new { success = true, message = "Thêm thú cưng thành công!" }); // (Trả 200 OK)
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi thêm thú cưng: " + ex.Message });
            }
        }
```

**Giải thích chi tiết:**
- `[FromBody] CreatePetDto dto`: Bóc tách cục JSON từ Client và nhồi vào Thùng hàng DTO.
- `if (customerId == null)`: Đây là Lỗi Nghiệp vụ. Khách hàng bắt buộc phải có Số điện thoại trước khi thao tác tiếp.
- Đẩy DTO xuống cho Service xử lý và đứng đợi kết quả.

```csharp
        // HÀM 3: CẬP NHẬT THÚ CƯNG
        [HttpPut("{id}")] // (Yêu cầu HTTP PUT vào URL: /api/mypets/5)
        [MyPetClinic.WebApi.Filters.AuthorizeOwner] // (Thẻ BẢO MẬT: Chặn không cho sửa pet của người khác)
        public async Task<IActionResult> UpdatePet(long id, [FromBody] UpdatePetDto dto) // (Hàm: Sửa Thú cưng)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dto.Id) // (Bảo mật: Kiểm tra chéo ID trên URL và ID trong JSON)
            {
                return BadRequest(new { success = false, message = "ID thú cưng không hợp lệ." });
            }

            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                if (customerId == null)
                {
                    return BadRequest(new { success = false, message = "Bạn cần cập nhật hồ sơ khách hàng trước." });
                }
                
                await _petService.UpdatePetAsync(dto, customerId.Value); // (Giao cho Service cập nhật vào DB)
                return Ok(new { success = true, message = "Cập nhật thông tin thú cưng thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi cập nhật thú cưng: " + ex.Message });
            }
        }
```

**Giải thích chi tiết:**
- `[AuthorizeOwner]`: Thẻ bảo mật cực cao (Filter tùy chỉnh). Nó sẽ chặn ngay tại cửa không cho phép bạn gọi API sửa thông tin thú cưng của người khác. Tránh lỗi kinh điển IDOR (Chiếm quyền truy cập gián tiếp).
- `if (id != dto.Id)`: Kiểm tra chéo. Đảm bảo ID truyền trên thanh địa chỉ URL trùng khớp với ID nằm trong gói tin JSON gửi lên. Ngăn chặn hacker dùng tool sửa lệnh lén lút.

```csharp
        // HÀM 4: LẤY CHI TIẾT 1 THÚ CƯNG
        [HttpGet("{id}")] // (Yêu cầu HTTP GET vào URL: /api/mypets/5)
        [MyPetClinic.WebApi.Filters.AuthorizeOwner] // (Bảo mật: Chặn không cho XEM pet người khác)
        public async Task<IActionResult> GetPetDetails(long id) // (Hàm: Lấy Chi tiết Thú cưng)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                if (customerId == null) return NotFound(new { message = "Không tìm thấy thú cưng." });
                
                var pet = await _petService.GetPetByIdAsync(id, customerId.Value); // (Gọi DB lấy 1 Pet ra)
                if (pet == null)
                {
                    return NotFound(new { message = "Không tìm thấy thú cưng." }); // (Trả 404 nếu pet rỗng)
                }
                return Ok(pet); // (Trả 200 kèm cục JSON của Pet)
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
```

**Giải thích chi tiết:**
- Tương tự như Update, hàm Get cũng được bảo vệ nghiêm ngặt. Hệ thống sẽ truyền cả `id` của pet và `customerId` của người dùng xuống Service để Service đối chiếu lại lần nữa.

```csharp
        // HÀM 5: XÓA THÚ CƯNG
        [HttpDelete("{id}")] // (Yêu cầu HTTP DELETE vào URL: /api/mypets/5)
        [MyPetClinic.WebApi.Filters.AuthorizeOwner] // (Bảo mật: Chặn không cho XÓA pet người khác)
        public async Task<IActionResult> DeletePet(long id) // (Hàm: Xóa Thú cưng)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                if (customerId == null)
                {
                    return BadRequest(new { success = false, message = "Lỗi khi xóa thú cưng: Không tìm thấy hồ sơ khách hàng." });
                }
                
                await _petService.DeletePetAsync(id, customerId.Value); // (Gọi Service thực hiện xóa mềm)
                return Ok(new { success = true, message = "Đã xóa thú cưng thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi xóa thú cưng: " + ex.Message });
            }
        }
```

**Giải thích chi tiết:**
- Gọi `DeletePetAsync` ở tầng Service để thực hiện "Xóa mềm" (Soft Delete). Tuyệt đối không bao giờ cho khách hàng xóa cứng (DELETE FROM) vì sẽ làm hỏng dữ liệu báo cáo lịch sử.

```csharp
        // HÀM 6: ĐĂNG TẢI ẢNH ĐẠI DIỆN THÚ CƯNG
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadPetAvatar(IFormFile avatarFile, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        {
            // (1. Kiểm tra chống gửi file rỗng - File Ma)
            if (avatarFile == null || avatarFile.Length == 0) return BadRequest(new { message = "Vui lòng chọn một file ảnh hợp lệ." });

            // (2. Kiểm tra đuôi file an toàn. Chặn .exe, .php mã độc)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension)) return BadRequest(new { message = "Chỉ chấp nhận các file ảnh định dạng: .jpg, .jpeg, .png, .gif" });

            // (3. Kiểm tra dung lượng file. Chặn file > 2MB chống DDoS tràn ổ cứng)
            if (avatarFile.Length > 2 * 1024 * 1024) return BadRequest(new { message = "Kích thước ảnh không được vượt quá 2MB." });

            try
            {
                // (Tạo đường dẫn vật lý /wwwroot/uploads/pets)
                string webRootPath = webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(webRootPath, "uploads", "pets");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // (4. Sinh tên file ngẫu nhiên chống đụng độ file cùng tên)
                string uniqueFileName = $"{Guid.NewGuid()}_{avatarFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // (5. Mở luồng lưu file vật lý, dùng block using để TỰ ĐỘNG GIẢI PHÓNG RAM khi xong)
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream); 
                }

                string avatarUrl = $"/uploads/pets/{uniqueFileName}"; // (Đường dẫn trả về Giao diện)
                return Ok(new { success = true, avatarUrl, message = "Tải ảnh lên thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi tải ảnh lên: " + ex.Message });
            }
        }
```

**Giải thích chi tiết Thuật toán Upload Ảnh:**
- Hàm `UploadPetAvatar` chứa thuật toán 5 bước chuẩn mực.
- **Bảo mật file:** Chỉ cho phép `.jpg, .png`. Nếu hacker cố tình upload file `.php` chứa mã độc chạy trên server, hàm `Contains()` sẽ phát hiện và đá văng ra ngay.
- **Bảo vệ ổ đĩa:** File ảnh quá lớn (>2MB) không cho lên.
- **Tránh ghi đè:** Dùng `Guid.NewGuid()` trộn thêm 32 ký tự ngẫu nhiên vào tên file. Ví dụ: Khách A up `cun.jpg`, Khách B cũng up `cun.jpg`, hệ thống sẽ biến thành `abcd_cun.jpg` và `xyzt_cun.jpg`.
- **Chống kẹt Server:** Dùng cấu trúc `using (FileStream...)`. Nó đảm bảo ghi file xong là ngắt luồng (giải phóng RAM) ngay lập tức dù cho có lỗi xảy ra.

---

### PHẦN 5.4 - PHÂN TÍCH SERVICE (TẦNG XỬ LÝ LOGIC NGHIỆP VỤ)

**> KHÁI NIỆM:** Service là nơi chứa "Quy tắc kinh doanh" (Business Rules). Nó nối với Database thông qua Repository và đảm bảo mọi dữ liệu lưu xuống là hợp lệ.

**Tệp:** `MyPetClinic.Application/Services/PetService.cs` *(Dịch vụ Xử lý Thú Cưng)*

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class PetService : IPetService // (Dịch vụ Thú cưng)
    {
        private readonly IUnitOfWork _unitOfWork; // (Đơn vị Giao dịch TOÀN CỤC)
        private readonly IPetRepository _petRepository; // (Kho dữ liệu Thú cưng)

        public PetService(IUnitOfWork unitOfWork, IPetRepository petRepository) // (Tiêm phụ thuộc qua Constructor)
        {
            _unitOfWork = unitOfWork;
            _petRepository = petRepository;
        }
```

**Giải thích chi tiết:**
- `IUnitOfWork`: (Giao diện Quản lý Giao dịch Toàn cục). Nó có sức mạnh gộp nhiều câu lệnh (Thêm, Xóa, Sửa) vào chung một Giao dịch (Transaction). Nếu 1 lệnh bị lỗi, nó sẽ Hoàn tác (Rollback) toàn bộ để tránh sai lệch Database.
- `IPetRepository`: Người Quản lý Kho thú cưng. Service không viết mã SQL, nó nhờ Repository đi lấy dữ liệu giùm.

```csharp
        // HÀM 1: LẤY DANH SÁCH THÚ CƯNG CỦA TÔI
        public async Task<IEnumerable<PetDto>> GetMyPetsAsync(Guid CustomerId) // (Hàm: Lấy DS Pet Bất đồng bộ)
        {
            // (Nhờ Repo bắn câu SELECT * FROM Pets WHERE CustomerId = ... xuống SQL)
            var pets = await _petRepository.GetPetsByOwnerIdAsync(CustomerId); 
            
            // (Sử dụng hàm mở rộng LINQ Select để chuyển từng Entity Pet to tướng thành cái thùng hàng PetDto nhỏ gọn)
            return pets.Select(MapToDto); 
        }
```

**Giải thích chi tiết:**
- Gọi Repository lấy mảng các Thực thể (Entity) lên RAM.
- Dùng LINQ `Select(MapToDto)` để biến đổi mảng Entity này thành mảng DTO, giấu đi các thuộc tính bảo mật trước khi gửi về cho Controller.

```csharp
        // HÀM 2: LẤY CHI TIẾT 1 THÚ CƯNG
        public async Task<PetDto?> GetPetByIdAsync(long id, Guid CustomerId) // (Hàm: Lấy 1 Pet Bất đồng bộ)
        {
            var pet = await _petRepository.GetPetByIdAsync(id); // (Lấy Entity theo ID)
            
            // (CHỐT CHẶN BẢO MẬT KÉP - CHỐNG IDOR): So sánh ID chủ trong DB với ID đang truy cập
            if (pet == null || pet.CustomerId != CustomerId)
            {
                return null; // (Không phải chủ thì giấu luôn, trả về null)
            }
            return MapToDto(pet); // (Đổi thành DTO rồi trả về)
        }
```

**Giải thích chi tiết:**
- **BẢO MẬT KÉP (IDOR):** Dòng `if (pet.CustomerId != CustomerId)` là một chốt chặn bảo mật cực kì vững chắc ở tầng Service. Đảm bảo rằng dù ai đó gọi trúng hàm này, nếu họ không phải là Chủ sở hữu của thú cưng đó, hệ thống sẽ chối bỏ và trả về `null` ngay lập tức. Tầng Service phải độc lập tự bảo vệ chính mình.

```csharp
        // HÀM 3: THÊM THÚ CƯNG MỚI
        public async Task<PetDto> AddPetAsync(CreatePetDto dto, Guid CustomerId) // (Hàm: Thêm Pet Bất đồng bộ)
        {
            // (Khởi tạo 1 Entity Pet hoàn toàn mới trên bộ nhớ RAM)
            var pet = new Pet
            {
                CustomerId = CustomerId, // (BẮT BUỘC gán chủ sở hữu từ Token, không lấy từ DTO)
                Name = dto.Name,
                Species = dto.Species,
                Breed = dto.Breed,
                Gender = dto.Gender,
                BirthDate = dto.BirthDate,
                Weight = dto.Weight,
                Color = dto.Color,
                BloodType = dto.BloodType,
                Sterilized = dto.Sterilized,
                MicrochipCode = dto.MicrochipCode,
                AllergyNote = dto.AllergyNote,
                Avatar = dto.Avatar,
                CreatedAt = DateTime.UtcNow // (Lấy giờ chuẩn quốc tế)
            };

            await _petRepository.CreatePetAsync(pet); // (Báo Repo đánh dấu INSERT)
            await _petRepository.SaveChangesAsync(); // (Lệnh DB chạy INSERT thật)
            return MapToDto(pet); // (Trả về DTO cho Client hiển thị luôn mà không cần tải lại trang)
        }
```

**Giải thích chi tiết:**
- Dùng từ khóa `new Pet` để tạo Thực thể mới tinh.
- `CustomerId = CustomerId`: Chốt chặn an toàn nhất. Ép cứng ID chủ sở hữu vào Thực thể, bỏ qua sự can thiệp của DTO.
- `_petRepository.CreatePetAsync(pet)`: Báo với Kho chuẩn bị hành trang lưu.
- `_petRepository.SaveChangesAsync()`: Kích hoạt SQL chạy lệnh INSERT vào bảng Pets.

```csharp
        // HÀM 4: CẬP NHẬT THÚ CƯNG
        public async Task UpdatePetAsync(UpdatePetDto dto, Guid CustomerId) // (Hàm: Sửa Pet Bất đồng bộ)
        {
            var pet = await _petRepository.GetPetByIdAsync(dto.Id); // (Kéo Entity từ DB lên RAM)
            
            // (CHỐT CHẶN BẢO MẬT - NÉM LỖI): Bắt buộc kiểm tra quyền sở hữu!
            if (pet == null || pet.CustomerId != CustomerId)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thú cưng hoặc bạn không có quyền sửa.");
            }

            // (Gán đè dữ liệu mới từ DTO sang Entity)
            pet.Name = dto.Name;
            pet.Species = dto.Species;
            pet.Breed = dto.Breed;
            pet.Gender = dto.Gender;
            pet.BirthDate = dto.BirthDate;
            pet.Weight = dto.Weight;
            pet.Color = dto.Color;
            pet.BloodType = dto.BloodType;
            pet.Sterilized = dto.Sterilized;
            pet.MicrochipCode = dto.MicrochipCode;
            pet.AllergyNote = dto.AllergyNote;
            pet.Avatar = dto.Avatar;

            await _petRepository.UpdatePetAsync(pet); // (Báo Repo đánh dấu UPDATE)
            await _petRepository.SaveChangesAsync(); // (Chạy SQL UPDATE lưu đè dữ liệu)
        }
```

**Giải thích chi tiết:**
- `throw new UnauthorizedAccessException(...)`: Nếu không phải chủ, ném thẳng ngoại lệ ra ngoài cho Controller bắt lại. Trừng phạt kẻ táy máy sửa thông tin thú cưng người khác.

```csharp
        // HÀM 5: CẬP NHẬT TRẠNG THÁI ĐẶC BIỆT (Quy tắc kinh doanh)
        public async Task UpdatePetStatusAsync(long petId, bool isDeceased, bool isAggressive) // (Hàm: Cập nhật Trạng thái Mất/Dữ)
        {
            var pet = await _petRepository.GetPetByIdAsync(petId);
            if (pet == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thú cưng.");
            }

            pet.IsDeceased = isDeceased; // (Bật/tắt Cờ báo tử)
            pet.IsAggressive = isAggressive; // (Bật/tắt Cờ báo hung dữ)

            // QUY TẮC KINH DOANH SỐ 1: HỦY LỊCH HẸN TỰ ĐỘNG
            if (isDeceased)
            {
                // (Tìm mọi lịch hẹn: Của pet này + Đang pending/confirmed + Diễn ra trong tương lai (>= hôm nay))
                var pendingAppointments = await _unitOfWork.Appointments.FindAsync(
                    a => a.PetId == petId 
                    && (a.Status == "pending" || a.Status == "confirmed") 
                    && a.AppointmentDate >= DateTime.UtcNow.Date);
                
                // (Duyệt qua danh sách bằng vòng lặp foreach)
                foreach (var appt in pendingAppointments)
                {
                    appt.Status = "cancelled"; // (Đổi Status = Hủy)
                    _unitOfWork.Appointments.Update(appt); // (Báo UPDATE dòng lịch hẹn này)
                }
            }

            await _petRepository.UpdatePetAsync(pet); // (Báo UPDATE con pet)
            
            // SỨC MẠNH CỦA IUnitOfWork (Giao dịch ACID):
            // Lệnh này gom (1 lệnh UPDATE PET) và (Ví dụ có 3 lịch thì + 3 lệnh UPDATE APPOINTMENT) 
            // Gửi xuống SQL cùng 1 lúc dưới dạng Cùng 1 Giao dịch (Transaction). 
            // Đảm bảo: Thành công là thành công hết, Đứt cáp thì ROLLBACK (hủy) toàn bộ. Không sợ lỗi dữ liệu nửa vời.
            await _unitOfWork.SaveChangesAsync(); 
        }
```

**Giải thích chi tiết LOGIC TỰ ĐỘNG HỦY LỊCH CỰC ĐỈNH:**
- Chức năng này dành cho Bác sĩ. Khi Bác sĩ báo pet đã qua đời (`isDeceased = true`), hệ thống sẽ kích hoạt một cơ chế thu dọn tự động (Cleanup).
- `_unitOfWork.Appointments.FindAsync(...)`: Tìm TẤT CẢ các Lịch khám trong tương lai (`AppointmentDate >= DateTime.UtcNow.Date`) của con thú cưng này đang ở trạng thái Chờ duyệt (`pending`) hoặc Đã duyệt (`confirmed`).
- Dùng vòng lặp `foreach` duyệt qua danh sách các lịch đó, chuyển trạng thái Status thành Đã hủy (`cancelled`).
- Cuối cùng, dòng `await _unitOfWork.SaveChangesAsync()` đóng vai trò "Trùm cuối". Nó sẽ mở MỘT Giao dịch SQL (Transaction). Gửi lệnh UPDATE bảng Pet và lệnh UPDATE bảng Appointment CÙNG MỘT LÚC. Nếu lúc đó đứt cáp hoặc mất điện, Database sẽ TỰ ĐỘNG ROLLBACK hủy bỏ mọi thao tác.

```csharp
        // HÀM 6: XÓA MỀM THÚ CƯNG
        public async Task DeletePetAsync(long id, Guid CustomerId) // (Hàm: Xóa Pet Bất đồng bộ)
        {
            var pet = await _petRepository.GetPetByIdAsync(id);
            
            // (BẢO MẬT: Không được xóa pet người khác)
            if (pet == null || pet.CustomerId != CustomerId)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thú cưng hoặc bạn không có quyền xóa.");
            }

            await _petRepository.SoftDeletePetAsync(id); // (Hàm này ở tầng Repo sẽ set cột DeletedAt = DateTime.UtcNow)
            await _petRepository.SaveChangesAsync(); // (Lưu thay đổi)
        }

        // Hàm ẩn: Biến Entity thành DTO (Mục đích tái sử dụng code)
        private PetDto MapToDto(Pet pet)
        {
            return new PetDto
            {
                Id = pet.Id,
                CustomerId = pet.CustomerId,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Gender = pet.Gender,
                BirthDate = pet.BirthDate,
                Weight = pet.Weight,
                Color = pet.Color,
                BloodType = pet.BloodType,
                Sterilized = pet.Sterilized,
                MicrochipCode = pet.MicrochipCode,
                AllergyNote = pet.AllergyNote,
                Avatar = pet.Avatar,
                CreatedAt = pet.CreatedAt
            };
        }
    }
}
```

**Giải thích chi tiết:**
- Tương tự mọi hàm sửa/xóa, phải kiểm tra ID chủ.
- `SoftDeletePetAsync`: Hệ thống áp dụng kỹ thuật **XÓA MỀM (Soft Delete)**. Tức là không chạy lệnh `DELETE FROM` làm mất dữ liệu vĩnh viễn (sẽ ảnh hưởng tới các báo cáo doanh thu y tế cũ). Thay vào đó, Repository chỉ cập nhật cột `DeletedAt = Hôm nay`. Thú cưng sẽ bị ẩn khỏi giao diện nhưng dữ liệu gốc vẫn còn nằm trên SQL Server để truy vết.
- Hàm `MapToDto`: Viết một lần, dùng nhiều lần. Tách code biến đổi Entity -> DTO ra riêng để tránh lặp code.

---
*(Hết tài liệu đào tạo chuyên sâu Quản lý Thú cưng - BẢN HOÀN HẢO THEO YÊU CẦU)*
