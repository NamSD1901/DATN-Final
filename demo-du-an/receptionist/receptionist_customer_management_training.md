# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG QUẢN LÝ KHÁCH HÀNG & THÚ CƯNG (RECEPTIONIST) - BẢN FULL DEEP DIVE (COMBO GIẢI THÍCH KÉP)

> [!NOTE]
> Đây là tài liệu Đào tạo số 8, tập trung vào Nghiệp vụ cốt lõi của Lễ tân: **Quản lý Hồ sơ Khách hàng và Thú cưng**.
> Điểm sáng chói nhất của module này không nằm ở các thao tác Thêm/Sửa/Xóa (CRUD) cơ bản, mà nằm ở hệ thống **Tìm kiếm đa năng (OmniSearch)** tốc độ cao và luồng **Tạo Hồ sơ kép** tiết kiệm thời gian cho Lễ tân.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Hồ sơ Khách hàng & Thú cưng (Góc nhìn Lễ tân).
- **Mục đích:** Khi khách vãng lai bước vào phòng khám, Lễ tân cần tra cứu cực nhanh xem người này đã từng đến chưa. Nếu chưa, Lễ tân sẽ tạo một lúc cả Hồ sơ người và Hồ sơ thú cưng.
- **Quy tắc kinh doanh (Business Rules):** 
  - Khách hàng không bắt buộc phải tải App hay tạo Tài khoản (`HasAccount = false`).
  - Lễ tân có thể tra cứu bằng: Tên, Số điện thoại, Email khách, HOẶC Tên thú cưng, Mã Microchip thú cưng.
  - Số điện thoại và Email không được phép trùng lặp trong hệ thống (Validation Thép).

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG CONTROLLER (GIAO DIỆN LỄ TÂN)

**Tệp:** `WebApi/Controllers/ReceptionistController.cs`

```csharp
        /// <summary>
        /// Thanh tìm kiếm vạn năng (Omni Box) trên thanh điều hướng của Lễ tân.
        /// </summary>
        [HttpGet("omni-search")] // (API tra cứu tốc độ cao)
        public async Task<IActionResult> OmniSearch([FromQuery] string q)
        {
            // (Chỉ đơn giản là ném từ khóa 'q' xuống cho Service làm việc nặng)
            var results = await _receptionistService.OmniSearchAsync(q);
            return Ok(results);
        }

        /// <summary>
        /// Tạo mới một Khách hàng kèm theo danh sách các Thú cưng của họ.
        /// </summary>
        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto model)
        {
            // (1. Bộ lọc mặc định của ASP.NET: Kiểm tra xem các trường Required đã điền chưa)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = string.Join("<br/>", errors) }); // (Nối các lỗi lại thành mã HTML ngắt dòng)
            }

            try
            {
                // (2. Đẩy Thùng hàng DTO xuống Service để lưu)
                var customerId = await _receptionistService.CreateCustomerWithPetsAsync(model);
                return Ok(new { success = true, customerId = customerId, message = $"Đã tạo hồ sơ cho {model.FullName} thành công!" });
            }
            catch (InvalidOperationException ex)
            {
                // (3. Bắt lỗi Validation Thép từ Service: Trùng số điện thoại, trùng Email)
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
```

---

### PHẦN 2.2 - TẦNG SERVICE (HỆ THỐNG TÌM KIẾM ĐA NĂNG - OMNISEARCH)

**Tệp:** `MyPetClinic.Application/Services/ReceptionistService.cs`

Tại sao gọi là OmniSearch? Vì bình thường bạn muốn tìm thú cưng thì vào bảng Pet, tìm người thì vào bảng Customer. OmniSearch cho phép gõ "Cậu Vàng" hoặc "0901234567" vào **Cùng một ô tìm kiếm**, hệ thống sẽ tự động quét chéo.

```csharp
        public async Task<List<OmniSearchDto>> OmniSearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<OmniSearchDto>(); // (Gõ rỗng thì trả về rỗng)

            var lowerQuery = query.ToLower(); // (Chuyển thành chữ thường để so sánh không phân biệt hoa/thường)

            // BƯỚC 1: QUÉT BẢNG THÚ CƯNG (TÌM TÊN HOẶC MÃ CHIP)
            // (Tìm con thú cưng nào có Tên HOẶC Mã Microchip chứa từ khóa)
            var matchedPets = await _unitOfWork.Pets.FindAsync(
                p => (p.Name != null && p.Name.ToLower().Contains(lowerQuery)) || 
                     (p.MicrochipCode != null && p.MicrochipCode.ToLower().Contains(lowerQuery))
            );
            
            // (Trích xuất ID của những người chủ đang sở hữu các con thú cưng vừa tìm được)
            var matchedOwnerIds = matchedPets.Select(p => p.CustomerId).ToList();

            // BƯỚC 2: QUÉT BẢNG KHÁCH HÀNG (QUÉT CHÉO)
            var usersList = await _unitOfWork.Users.FindWithIncludesAsync(
                u => u.IsActive == true && u.Role != null && u.Role.Name.ToLower() == "customer" &&
                    (
                     (u.FullName != null && u.FullName.ToLower().Contains(lowerQuery)) || // (Khớp Tên người)
                     (u.Phone != null && u.Phone.Contains(lowerQuery)) || // (Khớp SĐT)
                     (u.Email != null && u.Email.ToLower().Contains(lowerQuery)) || // (Khớp Email)
                     matchedOwnerIds.Contains(u.Id) // (HOẶC: Đây chính là chủ của con thú cưng vừa tìm thấy ở Bước 1)
                    ),
                u => u.Role!
            );
            
            // BƯỚC 3: GIỚI HẠN KẾT QUẢ ĐỂ TĂNG TỐC UI
            var users = usersList.Take(20).ToList(); // (Chỉ lấy tối đa 20 người đầu tiên để cái Popup Dropdown không bị dài ngoằng)

            // BƯỚC 4: ĐÓNG GÓI DỮ LIỆU TRẢ VỀ
            var result = new List<OmniSearchDto>();
            foreach (var user in users)
            {
                // (Kéo tất cả thú cưng của người này lên để hiển thị ảnh/tên dưới dạng list nhỏ)
                var userPets = await _unitOfWork.Pets.FindAsync(p => p.CustomerId == user.Id && !p.IsDeceased);
                
                var pets = userPets.Select(p => new OmniSearchPetDto
                    { PetId = p.Id, Name = p.Name, Species = p.Species, Breed = p.Breed, Weight = p.Weight, MicrochipCode = p.MicrochipCode })
                    .ToList();

                result.Add(new OmniSearchDto
                {
                    CustomerId = user.Id, FullName = user.FullName, Phone = user.Phone, Email = user.Email,
                    Pets = pets // (Gắn danh sách thú cưng vào Khách hàng)
                });
            }

            return result;
        }
```

**Giải thích chi tiết:**
- Đoạn code `matchedOwnerIds.Contains(u.Id)` chính là linh hồn của **Quét Chéo (Cross-Search)**. Khách hàng tới quầy bảo: "Tôi đến khám cho bé Cậu Vàng". Lễ tân gõ "Cậu Vàng" -> Hệ thống tìm thấy chó Cậu Vàng -> Tìm ra ông chủ tên Nam -> Trả kết quả hiển thị: `Nam (090xxxx) - Thú cưng: Cậu Vàng`. Tốc độ phục vụ khách hàng tăng lên gấp 10 lần so với việc phải hỏi số điện thoại.

---

### PHẦN 2.3 - TẦNG SERVICE (TẠO HỒ SƠ KÉP TRONG 1 NHÁP)

**Tệp:** `MyPetClinic.Application/Services/CustomerService.cs`

Khách vãng lai lần đầu đến phòng khám. Lễ tân không thể bắt họ đứng đợi 5 phút để tạo User, rồi lại mở form mới để tạo Pet. Phải tạo cả 2 cùng một lúc!

```csharp
        public async Task<Guid> CreateCustomerWithPetsAsync(CustomerCreateDto dto)
        {
            // 1. VALIDATION THÉP: KIỂM TRA TRÙNG LẶP SỐ ĐIỆN THOẠI & EMAIL
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingEmail = await _unitOfWork.Customers.GetFirstOrDefaultWithIncludesAsync(c => c.Email == dto.Email.Trim().ToLower() && c.DeletedAt == null);
                if (existingEmail != null) throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existingPhone = await _unitOfWork.Customers.GetFirstOrDefaultWithIncludesAsync(c => c.Phone == dto.Phone.Trim() && c.DeletedAt == null);
                if (existingPhone != null) throw new InvalidOperationException($"Số điện thoại '{dto.Phone}' đã tồn tại.");
            }

            // 2. KHỞI TẠO KHÁCH HÀNG (CHƯA CÓ TÀI KHOẢN APP)
            var newCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                // (Tự động sinh mã Khách hàng: CUS + NgàyThángNăm + GiờPhútGiây. Ví dụ: CUS231015093012)
                CustomerCode = "CUS" + DateTime.UtcNow.ToString("yyMMddHHmmss"), 
                FullName = dto.FullName?.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower(),
                Phone = dto.Phone?.Trim(),
                Address = dto.Address?.Trim(),
                HasAccount = false, // (Lưu ý: Khách tạo tại quầy thì HasAccount = false. Nghĩa là không dùng Password đăng nhập App được)
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Customers.AddAsync(newCustomer); // (Lưu nháp vào Memory)

            // 3. TẠO LUÔN DANH SÁCH THÚ CƯNG (TẠO KÉP)
            if (dto.Pets != null && dto.Pets.Any()) // (Nếu Lễ tân có điền Form thêm Thú cưng bên dưới)
            {
                foreach (var p in dto.Pets)
                {
                    var newPet = new Pet
                    {
                        CustomerId = newCustomer.Id, // (Móc nối thẳng vào ID của ông chủ vừa tạo ở trên)
                        Name = p.Name?.Trim(),
                        Species = p.Species?.Trim(),
                        Breed = p.Breed?.Trim(),
                        Weight = p.Weight,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Pets.AddAsync(newPet); // (Lưu nháp Thú cưng vào Memory)
                }
            }

            // 4. LƯU XUỐNG DATABASE CÙNG MỘT LÚC
            // (Entity Framework Core sẽ gộp lệnh Insert Customer và Insert Pet thành một Transaction ẩn. An toàn tuyệt đối)
            await _unitOfWork.SaveChangesAsync(); 
            
            return newCustomer.Id; // (Trả mã ID về để UI tự động chuyển hướng sang trang Chi tiết)
        }
```

**Giải thích chi tiết:**
- Lệnh `CustomerCode = "CUS" + DateTime.UtcNow.ToString("yyMMddHHmmss")`: Là một mẹo nhỏ để sinh Mã khách hàng tự động dựa trên Timestamp. Nó gần như không bao giờ bị trùng lặp và rất dễ đọc (Biết được tạo năm nào, tháng nào).
- **Sức mạnh của Entity Framework Core:** Bạn để ý thấy không có lệnh `BeginTransaction` ở đây, tại sao vẫn gọi là an toàn tuyệt đối? Vì khi gọi `SaveChangesAsync()`, EF Core sẽ tự động bọc toàn bộ các lệnh `AddAsync` trước đó vào một Transaction nội bộ. Nếu tạo Khách hàng thành công nhưng lúc tạo Thú cưng bị lỗi (do thiếu trường dữ liệu bắt buộc), nó sẽ tự động Xóa luôn Khách hàng kia, Database không bao giờ bị rác.

---
*(Hết tài liệu đào tạo chuyên sâu Lễ tân: Quản lý Khách hàng - OmniSearch)*
