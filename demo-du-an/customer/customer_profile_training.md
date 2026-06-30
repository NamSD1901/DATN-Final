# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG QUẢN LÝ HỒ SƠ KHÁCH HÀNG (CUSTOMER PROFILE) - BẢN FULL DEEP DIVE (CÓ DỊCH THUẬT)

> [!NOTE]
> Tài liệu này được biên soạn với mức độ chi tiết cao nhất (Deep Dive). Phân tích toàn bộ từ DTO, Entity, Controller đến Service. Mọi tên hàm (Method), tên biến (Variable) bằng tiếng Anh đều được **dịch sang tiếng Việt** để người mới không rành tiếng Anh vẫn hiểu cặn kẽ ý nghĩa của code. Không rút gọn, không bỏ sót bất kỳ tệp tin nào.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Hồ sơ Khách hàng (Customer Profile Management)
- **Mục đích:** Cho phép khách hàng xem, cập nhật thông tin cá nhân (Họ tên, SĐT, Địa chỉ, Ngày sinh), đổi mật khẩu và cập nhật ảnh đại diện (Avatar).
- **Người sử dụng:** Khách hàng đã đăng nhập.
- **Vai trò trong hệ thống:** Đây là chức năng nền tảng. Hồ sơ khách hàng phải có Số điện thoại thì mới được phép Đặt lịch khám.

---

## 2. Quy trình nghiệp vụ (Business Flow)
Khách hàng đăng nhập -> Chọn "Hồ sơ của tôi" -> Hệ thống gọi API GET `/api/profile` tải dữ liệu cũ -> Khách sửa thông tin trên form -> Bấm "Cập nhật" -> Gửi request PUT `/api/profile` -> Server kiểm tra hợp lệ -> Lưu xuống Database -> Báo thành công.

---

## 3. Luồng xử lý trong source code
View (Vue.js) -> Axios (mang theo JWT) -> `ProfileController` -> DTO Validation -> `UserService` (Logic nghiệp vụ) -> `UserRepository` -> `ApplicationDbContext` -> Database -> Trả kết quả ngược lại.

---

## 4. CÁC TỆP TIN THUỘC CHỨC NĂNG NÀY (ĐÃ PHÂN TÍCH CODE BÊN DƯỚI)
1. **Entity:** `MyPetClinic.Domain/Entities/User.cs`
2. **DTO:** `MyPetClinic.Application/DTOs/UpdateProfileDto.cs`, `UserProfileDto.cs`, `ChangePasswordDto.cs`
3. **Controller:** `WebApi/Controllers/ProfileController.cs`
4. **Service:** `MyPetClinic.Application/Services/UserService.cs`

---

## 5. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

Phần này sẽ giải thích code như đang dạy cho người mới bắt đầu lập trình. Không rút gọn bất kỳ dòng code nào.

---

### PHẦN 5.1 - PHÂN TÍCH ENTITY (LỚP ĐẠI DIỆN CƠ SỞ DỮ LIỆU)

**> KHÁI NIỆM: ENTITY LÀ GÌ VÀ ĐỂ LÀM GÌ?**
Trong kiến trúc phần mềm, **Entity** (Thực thể) là các lớp C# đóng vai trò như một "bản thiết kế" mô phỏng lại chính xác các bảng (tables) trong Database. Mặc định EF Core sẽ tự động đọc các Entity này và chuyển hóa chúng thành các bảng tương ứng trong CSDL. Nó chỉ chứa dữ liệu, không chứa logic xử lý phức tạp.

**Tệp:** `MyPetClinic.Domain/Entities/User.cs` *(Lớp thực thể Người dùng)*

```csharp
namespace MyPetClinic.Domain.Entities
{
    public class User // (Người dùng)
    {
        public Guid Id { get; set; } // (Mã định danh)
        public long RoleId { get; set; } // (Mã Vai trò)
        public string? FullName { get; set; } // (Họ và Tên)
        public string? Email { get; set; } // (Thư điện tử)
        public string? Phone { get; set; } // (Số điện thoại)
        public string? PasswordHash { get; set; } // (Mật khẩu đã bị băm)
        public string? Avatar { get; set; } // (Ảnh đại diện)
        public short? Gender { get; set; } // (Giới tính)
        public DateTime? DateOfBirth { get; set; } // (Ngày tháng năm sinh)
        public string? Address { get; set; } // (Địa chỉ)
        public bool? IsActive { get; set; } = true; // (Đang hoạt động?)
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow; // (Ngày tạo)
        public DateTime? DeletedAt { get; set; } // (Ngày xóa)

        public Role? Role { get; set; } // (Vai trò - Liên kết bảng)
        public EmployeeProfile? EmployeeProfile { get; set; } // (Hồ sơ Nhân viên - Liên kết bảng)
        public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>(); // (Danh sách Lời mời)
        
        public Guid? CustomerId { get; set; } // (Mã Khách hàng)
        public Customer? CustomerProfile { get; set; } // (Hồ sơ Khách hàng - Liên kết bảng)
    }
}
```

**Giải thích chi tiết:**
- `public Guid Id`: Thuộc tính Khóa chính (Primary Key). `Guid` là chuỗi 32 ký tự ngẫu nhiên, ưu điểm là khó bị hacker đoán mò (so với dùng số 1, 2, 3...).
- Cú pháp `string?`: Dấu `?` có nghĩa là "Có thể rỗng" (`Nullable`). Tức là trong database, cột này được phép chứa giá trị rỗng (`NULL`).
- `PasswordHash` *(Mật khẩu đã băm)*: **Không bao giờ lưu mật khẩu gốc**, mà chỉ lưu chuỗi đã bị băm (Hash) nát bét để bảo mật.
- `IsActive` *(Đang hoạt động)*: Mặc định tài khoản khi tạo ra là đang hoạt động (`true`).
- `CreatedAt` *(Ngày tạo)*: Mặc định khi được khởi tạo, tự động lấy giờ hiện tại theo giờ quốc tế (`UtcNow`).
- `DeletedAt` *(Ngày xóa)*: Cột dùng để "Xóa mềm" (Soft Delete). Thay vì xóa mất khỏi DB thì chỉ cập nhật ngày giờ xóa vào đây để ẩn đi.
- `CustomerProfile` *(Hồ sơ Khách hàng)*: Đây là Navigation Properties (Thuộc tính điều hướng) giúp nối (JOIN) sang bảng Khách Hàng tự động bằng C#.

---

### PHẦN 5.2 - PHÂN TÍCH CÁC DTO (DATA TRANSFER OBJECT)

**> KHÁI NIỆM: DTO LÀ GÌ VÀ TẠI SAO KHÔNG DÙNG LUÔN ENTITY?**
**DTO (Đối tượng truyền tải dữ liệu)** là những chiếc "Thùng hàng" chuyên dụng để chở dữ liệu giữa Giao diện (Vue.js) và Máy chủ (Controller).
- Tại sao phải dùng DTO? Tại vì Entity chứa thông tin Database bảo mật (như `PasswordHash`). Nếu gửi thẳng Entity cho Client, ta làm lộ dữ liệu nhạy cảm. Còn nếu dùng Entity để hứng dữ liệu từ Client, hacker có thể nhồi nhét sửa trường `IsActive = false` để phá hoại.
- DTO giúp chúng ta **chỉ nhận/gửi đúng những gì cần thiết**, đồng thời làm **Người gác cổng (Validator)** kiểm tra dữ liệu đầu vào.

**Tệp:** `MyPetClinic.Application/DTOs/UpdateProfileDto.cs` *(Thùng hàng chứa Dữ liệu Cập nhật Hồ sơ)*

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class UpdateProfileDto // (Đối tượng Dữ liệu Cập nhật Hồ sơ)
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự.")]
        public string? FullName { get; set; } // (Họ và Tên)

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không vượt quá 15 ký tự.")]
        public string? Phone { get; set; } // (Số điện thoại)

        public string? Address { get; set; } // (Địa chỉ)

        public short? Gender { get; set; } // (Giới tính)

        public DateTime? DateOfBirth { get; set; } // (Ngày sinh)
    }
}
```

**Giải thích từng dòng:**
- `[Required]`: Bắt buộc nhập. Nếu Client gửi JSON mà thiếu `FullName`, hệ thống tự chặn và trả về lỗi 400.
- `[StringLength(100)]`: Chặn tên dài quá 100 chữ, bảo vệ Database khỏi tràn bộ nhớ.
- `[Phone]`: Kiểm tra tính hợp lệ của định dạng số điện thoại.
- Các trường `Address`, `Gender`, `DateOfBirth` không có thẻ kiểm tra, tức là khách hàng được phép để trống.

---

**Tệp:** `MyPetClinic.Application/DTOs/ChangePasswordDto.cs` *(Thùng hàng chứa Dữ liệu Đổi Mật Khẩu)*

```csharp
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class ChangePasswordDto // (Đối tượng Dữ liệu Đổi Mật khẩu)
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống.")]
        public string CurrentPassword { get; set; } = null!; // (Mật khẩu hiện tại)

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự.")]
        public string NewPassword { get; set; } = null!; // (Mật khẩu mới)

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmNewPassword { get; set; } = null!; // (Xác nhận Mật khẩu mới)
    }
}
```

**Giải thích từng dòng:**
- `= null!`: Khai báo biến bắt buộc có dữ liệu, dấu `!` báo cho bộ biên dịch biết "Cứ yên tâm, nó sẽ không rỗng đâu".
- `[MinLength(8)]`: Mật khẩu mới bắt buộc >= 8 ký tự.
- `[Compare("NewPassword")]`: Tính năng tự động lấy nội dung của `ConfirmNewPassword` so sánh với `NewPassword`. Khác nhau là tự động văng lỗi.

---

### PHẦN 5.3 - PHÂN TÍCH CONTROLLER (BỘ ĐIỀU HƯỚNG GIAO TIẾP)

**> KHÁI NIỆM: CONTROLLER LÀ GÌ VÀ LÀM NHIỆM VỤ GÌ?**
**Controller** (Bộ Điều Khiển) là cánh cửa giao tiếp giữa thế giới bên ngoài và hệ thống Backend.
- Nhiệm vụ: Tiếp nhận yêu cầu HTTP (GET, POST), kiểm tra quyền (đăng nhập chưa?), bóc tách JSON thành DTO, rồi điều phối (gọi Service) ra làm việc.
- Quy tắc vàng: Controller KHÔNG ĐƯỢC CHỨA logic tính toán, truy vấn Database. Nó chỉ đóng vai trò Trạm Trung Chuyển (Điều phối).

**Tệp:** `WebApi/Controllers/ProfileController.cs` *(Bộ Điều khiển Hồ sơ)*

```csharp
namespace WebApi.Controllers
{
    [Authorize] // (Yêu cầu Đăng nhập)
    [ApiController] // (Đánh dấu đây là API)
    [Route("api/[controller]")] // (Định tuyến tự động: /api/profile)
    public class ProfileController : ControllerBase // (Lớp Điều khiển Hồ sơ)
    {
        private readonly IUserService _userService; // (Dịch vụ Người dùng)
        private readonly IWebHostEnvironment _webHostEnvironment; // (Môi trường máy chủ)

        public ProfileController(IUserService userService, IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
        }
```
**Giải thích:**
- `[Authorize]`: Chốt gác bảo vệ. Không có Token hợp lệ thì cấm cửa.
- `public ProfileController(...)`: Kỹ thuật **Dependency Injection** (Tiêm phụ thuộc). Thay vì dùng lệnh `new UserService()`, Controller xin Framework cấp phát sẵn Service để dùng.

```csharp
        // Hàm phụ trợ: Lấy Mã Người Dùng (từ Token)
        private Guid GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId)) return userId;
            throw new UnauthorizedAccessException("User ID not found in token/cookie.");
        }
```
**Giải thích:**
- `GetUserId()`: Tự động trích xuất Mã (ID) của khách hàng đang được giấu kín bên trong cái JWT Token.
- **Tại sao phải tự lấy ID từ Token?** Để CHỐNG LỖ HỔNG IDOR (Chiếm quyền). Nếu ta cho Client gửi `userId` lên, hacker đổi `userId=2` là có thể sửa hồ sơ của người khác! Lấy từ Token thì hacker không làm giả được.

```csharp
        [HttpGet] // (Nhận Yêu cầu Lấy Dữ liệu)
        public async Task<IActionResult> GetProfile() // (Hàm: Lấy Hồ sơ)
        {
            try
            {
                var userId = GetUserId(); // (Lấy ID)
                var profile = await _userService.GetUserProfileAsync(userId); // (Gọi Service lấy hồ sơ bất đồng bộ)
                if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ." }); // (Trả lỗi 404)
                return Ok(profile); // (Trả về thành công kèm dữ liệu)
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message }); // (Trả lỗi cấm truy cập)
            }
        }
```
**Giải thích:**
- `async/await`: (Lập trình Bất đồng bộ). Giúp Server không bị "đóng băng" đứng chờ Database phản hồi, mà sẽ đi phục vụ khách khác trong lúc chờ.
- `return Ok(profile)`: Chuyển đối tượng C# thành định dạng JSON và gửi về cho Client kèm mã 200 Thành công.

```csharp
        [HttpPut] // (Nhận Yêu cầu Cập nhật Dữ liệu)
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model) // (Hàm: Cập nhật Hồ sơ)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState); // (Nếu dữ liệu sai, trả lỗi ngay)

            try
            {
                var userId = GetUserId();
                bool success = await _userService.UpdateUserProfileAsync(userId, model); // (Gọi Service Cập nhật Hồ sơ)
                if (success) return Ok(new { success = true, message = "Cập nhật thông tin cá nhân thành công!" });
                return BadRequest(new { success = false, message = "Có lỗi xảy ra khi cập nhật thông tin." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
```
**Giải thích:**
- `[FromBody]`: Báo Framework đọc JSON gửi lên từ Frontend và nạp vào Thùng hàng `UpdateProfileDto model`.
- `bool success = await _userService.UpdateUserProfileAsync(...)`: Quăng dữ liệu xuống Service để làm việc nặng, đợi lấy kết quả Đúng/Sai (`True/False`).

```csharp
        [HttpPost("avatar")] // (Nhận Yêu cầu Đăng tải Ảnh)
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile) // (Hàm: Cập nhật Ảnh đại diện)
        {
            if (avatarFile == null || avatarFile.Length == 0) // (Kiểm tra file rỗng/ma)
                return BadRequest(new { message = "Vui lòng chọn một file ảnh hợp lệ." });

            // (Kiểm tra đuôi file an toàn)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Chỉ chấp nhận các file ảnh định dạng: .jpg, .jpeg, .png, .gif" });

            // (Kiểm tra dung lượng file < 2MB)
            if (avatarFile.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "Kích thước ảnh không được vượt quá 2MB." });

            try
            {
                var userId = GetUserId();
                string webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(webRootPath, "uploads", "avatars"); // (Thư mục lưu ảnh)
                if (!Directory.Exists(uploadsFolder)) // (Nếu chưa có thì tạo thư mục mới)
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // (Sinh tên file mới ngẫu nhiên chống trùng lặp)
                string uniqueFileName = $"{Guid.NewGuid()}_{avatarFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // (Tiến hành Lưu file xuống ổ cứng vật lý)
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream); // (Sao chép dữ liệu bất đồng bộ)
                }

                string avatarUrl = $"/uploads/avatars/{uniqueFileName}"; // (Đường dẫn ảo trên web)
                
                bool success = await _userService.UpdateAvatarAsync(userId, avatarUrl); // (Lưu đường dẫn vào DB)
                if (success) return Ok(new { success = true, avatarUrl, message = "Cập nhật ảnh đại diện thành công!" });
                return BadRequest(new { success = false, message = "Có lỗi xảy ra khi lưu ảnh đại diện." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
```
**Giải thích Thuật toán Upload File:**
- **Chống Hacker up Virus:** Quét đuôi file biến thành chữ thường (`.ToLowerInvariant()`), sau đó kiểm tra xem có thuộc danh sách an toàn `[".jpg", ".png"]` hay không. Chặn `.exe`, `.php`.
- **Chống DDoS đầy ổ cứng:** Giới hạn file <= 2MB.
- **Bảo mật đụng độ tên:** Dùng `Guid.NewGuid()` sinh mã ngẫu nhiên 32 ký tự nhét trước tên file. Tránh việc 2 user tải 2 file trùng tên đè lên nhau.
- `using (var fileStream = new FileStream(...))`: Cấu trúc này giúp tự động ĐÓNG FILE và GIẢI PHÓNG RAM ngay khi lưu xong, tránh lỗi treo file trên máy chủ.

---

### PHẦN 5.4 - PHÂN TÍCH SERVICE (TẦNG XỬ LÝ LOGIC NGHIỆP VỤ)

**> KHÁI NIỆM: SERVICE LÀ GÌ VÀ TẠI SAO PHẢI TÁCH RA KHỎI CONTROLLER?**
**Service** (Dịch vụ) là "Bộ não" của hệ thống. Nó chứa toàn bộ logic, tính toán, băm mật khẩu, xử lý múi giờ.
- **Tại sao phải tách riêng?** Nếu nhét hết code vào Controller, Controller sẽ phình to cả ngàn dòng. Tách ra Service giúp tái sử dụng code dễ dàng (VD: Hàm tính tiền có thể được gọi từ cả API Khách hàng lẫn API Bác sĩ). Hơn nữa, tách ra giúp dễ viết chương trình test (Unit Test).

**Tệp:** `MyPetClinic.Application/Services/UserService.cs` *(Dịch vụ Xử lý Người Dùng)*

```csharp
namespace MyPetClinic.Application.Services
{
    public class UserService : IUserService // (Dịch vụ Xử lý Người dùng)
    {
        private readonly IUserRepository _userRepository; // (Kho quản lý dữ liệu Người dùng)

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
```
**Giải thích:**
- Inject `IUserRepository` (Kho Dữ liệu). Service không tự viết lệnh SQL bừa bãi, nó phải nhờ anh thủ kho "Repository" đi lấy data từ Database lên.

```csharp
        // Hàm: Cập nhật Hồ sơ Người dùng Bất đồng bộ
        public async Task<bool> UpdateUserProfileAsync(Guid userId, UpdateProfileDto dto) 
        {
            var user = await _userRepository.GetUserByIdAsync(userId); // (Gọi kho ra lấy dữ liệu user cũ)
            if (user == null) return false;

            user.FullName = dto.FullName; // (Gán tên mới)
            user.Phone = dto.Phone; // (Gán sdt mới)
            user.Address = dto.Address; // (Gán địa chỉ mới)
            user.Gender = dto.Gender; // (Gán giới tính mới)
            
            // Xử lý Lỗi Múi Giờ Quốc Tế (UTC) của PostgreSQL
            if (dto.DateOfBirth.HasValue) // (Nếu có gửi lên Ngày sinh)
            {
                user.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc); // (Ép kiểu múi giờ về UTC)
            }
            else
            {
                user.DateOfBirth = null;
            }

            await _userRepository.UpdateUserAsync(user); // (Báo Cập nhật)
            await _userRepository.SaveChangesAsync(); // (Lưu thay đổi xuống Database)

            return true;
        }
```
**Giải thích:**
- Gán dữ liệu từ DTO sang Entity.
- **Xử lý Múi Giờ Quốc Tế (UTC):** Đây là "Cú lừa" lớn nhất của PostgreSQL. Nếu bạn đưa 1 biến `DateOfBirth` không rõ múi giờ vào Database, nó sẽ Văng lỗi sập chương trình. Lệnh `DateTime.SpecifyKind(..., DateTimeKind.Utc)` là lệnh ép C# đóng dấu "Đây là giờ Quốc Tế", giúp DB hiểu và lưu mượt mà.
- `SaveChangesAsync()`: **ĐÂY LÀ LÚC CHẠY LỆNH SQL THẬT**. Dữ liệu từ RAM (bộ nhớ tạm) chính thức được ghi vĩnh viễn xuống ổ cứng Server.

```csharp
        // Hàm: Đổi Mật Khẩu Bất đồng bộ
        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId); // (Lấy user)
            if (user == null || string.IsNullOrEmpty(user.PasswordHash)) return false; // (Nếu rỗng thì cút)

            // Kiểm tra mật khẩu hiện tại (Verify current password)
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash); 
            if (!isPasswordValid) return false; // (Sai mật khẩu cũ)

            // Băm mật khẩu mới (Hash new password)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _userRepository.UpdateUserAsync(user); // (Báo cập nhật)
            await _userRepository.SaveChangesAsync(); // (Lưu DB)

            return true;
        }
```
**Giải thích Thuật toán Băm (Hashing) - Đỉnh Cao Bảo Mật:**
- **Không bao giờ dùng dấu `==` để so sánh mật khẩu.** Mật khẩu trong DB là 1 đống băm nát bét vô nghĩa (Ví dụ: `$2a$11$N9xlz...`).
- Hàm `BCrypt.Verify()` (Xác thực Băm) sẽ: Nhận chuỗi khách vừa gõ, lôi phần gia vị (Salt) đang giấu trong chuỗi ở DB ra, tự trộn và xay nhuyễn lại. Xong đem kết quả so sánh với chuỗi DB. Giống thì là Nhập đúng pass cũ (`isPasswordValid = true`).
- `BCrypt.HashPassword(...)` (Băm Mật khẩu): Khi khách nhập pass mới, hàm này tự sinh ngẫu nhiên Salt, trộn, xay nát thành 1 cục vô nghĩa rồi đè vào `user.PasswordHash`. Hacker có trộm được DB cũng khóc thét vì không dịch ngược được.

---
*(Hết tài liệu phân tích siêu cấp Deep Dive - Version 4.0 - Có kèm Dịch thuật tiếng Việt)*
