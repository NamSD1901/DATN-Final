# TÀI LIỆU ĐÀO TẠO NỘI BỘ: QUẢN LÝ DỊCH VỤ (ADMIN VIEW) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 15, dành riêng cho **Quản trị viên (Admin)**.
> Mô-đun Quản lý Dịch vụ (Khám bệnh, Siêu âm, X-Quang, Spa...) là nguồn thu chính của Phòng khám. Khi Admin tạo hoặc sửa giá Dịch vụ, nó ảnh hưởng trực tiếp đến Bảng giá của Lễ tân.
> Điểm cốt lõi kỹ thuật ở đây là cơ chế **Nhật ký kiểm toán (Audit Trail)** để truy vết ai đã sửa giá dịch vụ, và kỹ thuật **Xóa mềm (Soft Delete)** để không làm hỏng dữ liệu Kế toán cũ.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Dịch vụ Y tế (Service Management).
- **Mục đích:** Khởi tạo, cập nhật Bảng giá và thời lượng Khám chữa bệnh.
- **Điểm nổi bật (Kỹ thuật):** Tích hợp `IAuditLogService` ghi nhận lịch sử thay đổi (Ai sửa giá? Lúc mấy giờ?). Ngăn chặn triệt để lệnh `DELETE` cứng trong SQL để bảo vệ sự toàn vẹn của Hóa đơn (Invoice) các tháng trước.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - THỰC THỂ (ENTITY) DỊCH VỤ 

**Tệp:** `MyPetClinic.Domain/Entities/Service.cs`

```csharp
    public class Service
    {
        public long Id { get; set; }
        public long CategoryId { get; set; } // Phân loại (Ví dụ: Khám bệnh, Spa, Siêu âm)
        public string Name { get; set; } = string.Empty; // Tên dịch vụ (VD: Siêu âm ổ bụng)
        
        public decimal? Price { get; set; } // Giá tiền
        public int? DurationMinutes { get; set; } // Thời lượng ước tính (VD: 30 phút - để xếp lịch)
        
        public string? Description { get; set; }
        
        // (Cờ khóa dịch vụ: Dùng để Xóa Mềm / Ngừng cung cấp dịch vụ)
        public bool IsActive { get; set; } = true; 

        // Khóa ngoại
        public ServiceCategory? Category { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
```

**Giải thích chi tiết:**
- **DurationMinutes:** Khác với các hệ thống bán lẻ, hệ thống Y tế MyPetClinic có đặc thù "Thời gian là tiền bạc". Thuộc tính `DurationMinutes` giúp tính toán tự động thuật toán Xếp lịch của Bác sĩ (Ví dụ: Dịch vụ Siêu âm kéo dài 45 phút, Lễ tân sẽ không thể book 2 ca cách nhau 30 phút).
- **IsActive:** Dịch vụ Y tế có thể lỗi thời hoặc ngưng cung cấp, nhưng tuyệt đối không được xóa khỏi Database.

---

### PHẦN 2.2 - TẦNG CONTROLLER (ĐIỂM GIAO TIẾP VỚI ADMIN)

**Tệp:** `WebApi/Controllers/AdminController.cs`

```csharp
    [Authorize(Roles = "admin,Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        // THÊM MỚI DỊCH VỤ
        [HttpPost("services")]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
        {
            // (1. Trích xuất ID của người Admin đang thao tác từ Token JWT)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // (2. Giao việc cho Service)
            var service = await _adminService.CreateServiceAsync(dto, currentUserId!);
            return Ok(new { success = true, service });
        }

        // CẬP NHẬT GIÁ DỊCH VỤ
        [HttpPut("services/{id}")]
        public async Task<IActionResult> UpdateService(long id, [FromBody] CreateServiceDto dto)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var service = await _adminService.UpdateServiceAsync(id, dto, currentUserId!);
                return Ok(new { success = true, service });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
```

**Giải thích chi tiết:**
- Tham số `currentUserId` luôn được bóc tách từ Token bảo mật (JWT) của người đang đăng nhập. Việc này ngăn chặn tình trạng "Sửa lén" - hệ thống luôn biết ai đang gõ phím.

---

### PHẦN 2.3 - TẦNG SERVICE (LOGIC & NHẬT KÝ KIỂM TOÁN - AUDIT TRAIL)

Đây là nơi thực thi logic quan trọng nhất: Thêm sửa xóa và "Điểm danh" người làm việc đó vào Sổ Cái (Audit Log).

**Tệp:** `MyPetClinic.Application/Services/AdminService.cs`

```csharp
        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, string currentUserId)
        {
            // BƯỚC 1: TẠO ENTITY
            var service = new Service
            {
                Name = dto.Name,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                DurationMinutes = dto.DurationMinutes,
                Description = dto.Description,
                IsActive = true
            };

            // BƯỚC 2: LƯU DATABASE
            await _unitOfWork.Services.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();

            // BƯỚC 3: GHI NHẬT KÝ KIỂM TOÁN (AUDIT TRAIL)
            await _auditLogService.LogActionAsync(currentUserId, "CreateService", $"Tạo dịch vụ mới: {dto.Name}");
            
            return new ServiceDto { ... }; // Map qua DTO
        }
```

**Giải thích chi tiết (Kiến trúc Audit Trail):**
- Lệnh `_auditLogService.LogActionAsync` là một kỹ thuật bảo mật cốt lõi trong các hệ thống Ngân hàng / Bệnh viện. Mọi thao tác đổi giá (Tăng/giảm giá khám) đều được lưu vào Sổ Cái (`AuditLogs` table) với Dấu thời gian (Timestamp) và Người thực hiện (`currentUserId`). 
- Nếu Lễ tân thắc mắc "Tại sao giá siêu âm hôm qua 300k nay thành 200k", Giám đốc chỉ cần mở bảng Audit là bắt tận tay Admin nào đã đổi giá!

---

### PHẦN 2.4 - KỸ THUẬT XÓA MỀM (SOFT DELETE) BẢO TỒN KẾ TOÁN

Khi một dịch vụ (Ví dụ: "Cắt tỉa lông mẫu cũ") không còn được cung cấp nữa. Admin không thể nhấp nút Xóa vĩnh viễn (Lệnh `DELETE`).

```csharp
        public async Task DeleteServiceAsync(long id, string currentUserId)
        {
            // 1. TÌM KIẾM DỊCH VỤ TRONG DB
            var service = await _unitOfWork.Services.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy dịch vụ.");

            // 2. LOGIC XÓA MỀM (SOFT DELETE)
            service.IsActive = false; // Ngừng hoạt động thay vì Xóa trắng
            
            // 3. CẬP NHẬT THAY VÌ REMOVE
            _unitOfWork.Services.Update(service);
            await _unitOfWork.SaveChangesAsync();

            // 4. GHI NHẬT KÝ
            await _auditLogService.LogActionAsync(currentUserId, "DeleteService", $"Khóa dịch vụ ID {id}");
        }
```

**Giải thích chi tiết (Bài toán Ràng buộc Toàn vẹn - Foreign Key):**
- **Tại sao không dùng `_unitOfWork.Services.Remove(service)`?** Giả sử tháng trước, có 100 khách hàng đã thanh toán dịch vụ "Cắt lông". Mã dịch vụ đó đã nằm cứng trong bảng Báo Cáo Doanh Thu (Hóa Đơn / InvoiceItem). Nếu ta lệnh SQL `DELETE` dịch vụ đó, Database lập tức sụp đổ vì vi phạm khóa ngoại, hoặc tệ hơn là bay luôn 100 Hóa đơn cũ khiến Doanh thu bị âm!
- **Giải pháp `IsActive = false`:** Dịch vụ sẽ bị ẩn khỏi màn hình Lễ tân (Lễ tân không thể book thêm ca mới). Nhưng dữ liệu Báo cáo Kế toán tháng trước vẫn tồn tại trường tồn vĩnh cửu.

---
*(Hết tài liệu đào tạo chuyên sâu Admin: Quản lý Dịch vụ Y tế)*
