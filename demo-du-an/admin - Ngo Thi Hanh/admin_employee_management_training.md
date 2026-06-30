# TÀI LIỆU ĐÀO TẠO NỘI BỘ: QUẢN TRỊ NHÂN SỰ (ADMIN VIEW) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 14, dành riêng cho **Quản trị viên (Admin)**.
> Mô-đun Quản lý Nhân sự của MyPetClinic tuân thủ nghiêm ngặt tiêu chuẩn bảo mật doanh nghiệp: Admin chỉ được phép tạo tài khoản (Email, Chức vụ), còn Mật khẩu phải do chính Nhân viên tự thiết lập thông qua Email Kích hoạt (Activation Link).
> Điểm cốt lõi kỹ thuật ở đây là **Quan hệ 1-1 (One-to-One)** giữa bảng `User` và bảng `EmployeeProfile`, cùng với thuật toán **Kích hoạt bằng Token** và cờ trạng thái `IsActive`.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Nhân sự (Employee Management).
- **Mục đích:** Thêm mới Bác sĩ/Lễ tân vào hệ thống, phân quyền (Role) và theo dõi trạng thái làm việc (Chờ kích hoạt / Đang làm / Đã nghỉ việc).
- **Điểm nổi bật (Kỹ thuật):** Giao tiếp bất đồng bộ qua Email. Bảng `User` dùng cờ `IsActive` để Admin biết được Nhân viên đã đọc Email và bấm kích hoạt hay chưa. Mật khẩu được băm (Hash) bởi chính tay nhân viên, Admin không thể can thiệp.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - THỰC THỂ (ENTITY) VÀ QUAN HỆ 1-1

Hệ thống có hàng ngàn User (phần lớn là Khách hàng tự đăng ký). Do đó, ta không thể nhồi CCCD, Chức vụ vào bảng `User`. Ta phải tách ra bảng `EmployeeProfile`.

**Tệp:** `MyPetClinic.Domain/Entities/User.cs` & `EmployeeProfile.cs`

```csharp
// --- THỰC THỂ TÀI KHOẢN GỐC ---
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        // (TRẠNG THÁI CỐT LÕI: Phân biệt "Chờ kích hoạt" và "Đã kích hoạt")
        public bool? IsActive { get; set; } 
        
        // (Khóa ngoại: Móc nối 1-1 sang bảng Hồ sơ Nhân sự)
        public EmployeeProfile? EmployeeProfile { get; set; } 
    }

// --- THỰC THỂ HỒ SƠ NHÂN SỰ (CHỈ DÀNH CHO NHÂN VIÊN) ---
    public class EmployeeProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // (Trỏ ngược lại bảng User)
        public string Position { get; set; } = string.Empty; // Tên Chức vụ
        
        // (Cờ báo hiệu Nghỉ việc - Dùng cho Soft Delete)
        public bool IsResigned { get; set; } = false; 
    }
```

---

### PHẦN 2.2 - TẦNG SERVICE TẠO TÀI KHOẢN (TRẠNG THÁI "CHỜ KÍCH HOẠT")

Khi Admin bấm "Thêm nhân viên", tài khoản lập tức rơi vào trạng thái "Chờ kích hoạt".

**Tệp:** `MyPetClinic.Application/Services/EmployeeService.cs`

```csharp
        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // BƯỚC 1: TẠO TÀI KHOẢN GỐC VỚI TRẠNG THÁI "CHỜ KÍCH HOẠT"
                var user = new User
                {
                    Email = request.Email,
                    RoleId = role.Id,
                    IsActive = false // QUAN TRỌNG: IsActive = false nghĩa là "Chờ Kích Hoạt", chưa cho phép Login!
                };
                await _unitOfWork.Users.AddAsync(user);

                // BƯỚC 2: TẠO HỒ SƠ NHÂN SỰ 1-1
                var profile = new EmployeeProfile { UserId = user.Id, IdentityCard = request.IdentityCard };
                await _unitOfWork.EmployeeProfiles.AddAsync(profile);

                // BƯỚC 3: TẠO TOKEN BẢO MẬT GỬI QUA EMAIL
                var token = Guid.NewGuid().ToString("N"); // Sinh chuỗi Hash 32 ký tự ngẫu nhiên
                var invitation = new Invitation
                {
                    UserId = user.Id,
                    Token = token,
                    ExpireAt = DateTime.UtcNow.AddHours(24) // Token chỉ sống được 24 tiếng
                };
                await _unitOfWork.Invitations.AddAsync(invitation);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // BƯỚC 4: GỬI EMAIL CHỨA LINK KÍCH HOẠT
                var link = $"http://localhost:5173/activate?token={token}";
                await _emailService.SendEmailAsync(user.Email, "Thiết lập mật khẩu", link);

                return MapToDto(user, profile, role.Name);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
```

**Giải thích chi tiết:**
- Khi hàm này chạy xong, DTO trả về cho Frontend Giao diện Admin sẽ có cờ `IsActive = false`. Lúc này, trên màn hình Admin, bảng danh sách nhân viên sẽ render ra một cái Badge (Nhãn) màu Vàng ghi chữ **"Chờ kích hoạt"**.
- Nhân viên lúc này mà cố tình tải App về đăng nhập bằng Email đó cũng sẽ bị văng ra vì `IsActive = false` chặn ở cửa `LoginAsync`.

---

### PHẦN 2.3 - TẦNG SERVICE KÍCH HOẠT (CHUYỂN SANG "ĐÃ KÍCH HOẠT")

Sau khi nhân viên mở Email, bấm vào link `http://localhost:5173/activate?token=ABC`, họ sẽ gõ Mật khẩu mới trên giao diện. Frontend sẽ gọi API Activate và truyền vào Tầng AuthService.

**Tệp:** `MyPetClinic.Application/Services/AuthService.cs`

```csharp
        public async Task<AuthResult> ActivateAccountAsync(ActivateAccountRequest request)
        {
            // BƯỚC 1: TÌM KIẾM TOKEN TRONG THÙNG CHỨA INVITATION
            var invitations = await _unitOfWork.Invitations.FindWithIncludesAsync(i => i.Token == request.Token, i => i.User!);
            var invitation = invitations.FirstOrDefault();

            // Kiểm tra hàng loạt: Token có tồn tại không? Đã bị ai dùng chưa? Đã quá 24h chưa?
            if (invitation == null || invitation.IsUsed)
                return new AuthResult { Success = false, ErrorMessage = "Token không hợp lệ hoặc đã được sử dụng." };

            if (invitation.ExpireAt < DateTime.UtcNow)
                return new AuthResult { Success = false, ErrorMessage = "Token đã hết hạn." };

            var user = invitation.User;

            // BƯỚC 2: BĂM MẬT KHẨU (BẢO MẬT)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            // BƯỚC 3: ĐẢO TRẠNG THÁI SANG "ĐÃ KÍCH HOẠT"
            user.IsActive = true; 
            
            // BƯỚC 4: ĐỐT BỎ TOKEN (Ngăn chặn bấm link lần 2)
            invitation.IsUsed = true;

            // LƯU XUỐNG DB
            _unitOfWork.Invitations.Update(invitation);
            await _userRepository.UpdateUserAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResult { Success = true };
        }
```

**Giải thích chi tiết (Vòng đời của Token):**
- Ngay khi hàm này chạy thành công, cờ `IsActive` trong DB bị đảo thành `true`. 
- Lúc này, nếu Admin bấm F5 tải lại danh sách Nhân viên, DTO sẽ trả về `IsActive = true`. Giao diện Admin lập tức đổi cái Badge màu Vàng thành màu Xanh Lá Cây ghi chữ **"Đang hoạt động"** hoặc **"Đã kích hoạt"**. Admin nhìn vào là biết ngay nhân viên này đã thiết lập mật khẩu thành công!
- Đồng thời, `invitation.IsUsed = true` (Đốt Token) đảm bảo rằng cái Link trong Email của nhân viên vĩnh viễn trở thành vô dụng. Nếu họ bấm lại lần nữa, hệ thống sẽ chửi "Token đã được sử dụng".

---

### PHẦN 2.4 - TẦNG SERVICE (CHO NGHỈ VIỆC - SOFT DELETE)

Khi Bác sĩ xin nghỉ việc, Admin gạt công tắc "Cho nghỉ việc".

```csharp
        public async Task<EmployeeDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request)
        {
            var user = await _unitOfWork.Users.FindWithIncludesAsync(...);

            // LOGIC XỬ LÝ NGHỈ VIỆC (SOFT DELETE)
            user.EmployeeProfile!.IsResigned = request.IsResigned; // Đánh dấu cờ Hồ sơ là đã nghỉ hưu
            
            if (request.IsResigned)
            {
                // ÉP TRẠNG THÁI VỀ "BỊ KHÓA"
                user.IsActive = false; // Tước quyền Login hệ thống lập tức
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }
```

**Tổng kết 3 vòng đời của `IsActive`:**
1. **Mới tạo:** `IsActive = false` (Admin nhìn thấy "Chờ kích hoạt").
2. **Kích hoạt thành công:** `IsActive = true` (Admin nhìn thấy "Đang hoạt động", nhân viên bắt đầu được đi làm).
3. **Nghỉ việc:** Bật cờ `IsResigned = true` -> kéo theo `IsActive = false` (Admin nhìn thấy "Đã nghỉ việc", nhân viên bị đá văng khỏi hệ thống lập tức, dữ liệu quá khứ vẫn còn nguyên).

---
*(Hết tài liệu đào tạo chuyên sâu Admin: Quản lý Nhân sự)*
