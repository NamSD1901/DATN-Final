# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG HỒ SƠ Y TẾ THÚ CƯNG (PET MEDICAL HISTORY) - BẢN FULL DEEP DIVE (COMBO GIẢI THÍCH KÉP)

> [!NOTE]
> Tiếp nối tinh hoa của tài liệu Quản lý Thú cưng, tài liệu này tiếp tục áp dụng **"Giải thích Kép" (Double Explanation)**: Vừa có chú thích `// (...)` trong code, vừa có phân tích gạch đầu dòng bên dưới. Và ĐẶC BIỆT: **Giải thích cặn kẽ 100% từng dòng lệnh một, không bỏ sót dòng nào**.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Xem Hồ sơ y tế chi tiết của thú cưng.
- **Mục đích:** Cho phép khách hàng xem lại toàn bộ lịch sử khám chữa bệnh của thú cưng, bao gồm: Lịch sử khám (Medical Records), Lịch sử tiêm phòng (Vaccinations), Lịch sử đặt hẹn (Appointments) và Các đơn thuốc đã kê (Prescriptions).
- **Điểm nổi bật:** Dữ liệu được trích xuất từ mô hình chuẩn SOAP của bác sĩ y khoa, nhưng được biên dịch lại (Parse) thành dạng chữ dễ hiểu cho người trần mắt thịt (Khách hàng) đọc.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG CONTROLLER (NGƯỜI GÁC CỔNG BẢO MẬT KÉP)

**Tệp:** `WebApi/Controllers/MyPetsController.cs`

Bốn hàm dưới đây nằm trong cùng file Controller với chức năng Quản lý Thú cưng, đóng vai trò trả về dữ liệu lịch sử.

```csharp
        /// <summary>
        /// Khách hàng xem lịch sử bệnh án của thú cưng (chỉ được xem thú cưng của chính mình).
        /// </summary>
        [HttpGet("{id}/medical-records")] // (Đón Request GET tại URL: /api/mypets/5/medical-records)
        [MyPetClinic.WebApi.Filters.AuthorizeOwner] // (Thẻ Filter: Tự động chặn các Request xem trộm thú cưng người khác)
        public async Task<IActionResult> GetPetMedicalRecords(long id) // (Hàm: Lấy lịch sử bệnh án)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync(); // (Lấy ID của Khách đang đăng nhập)
                if (customerId == null) return NotFound(new { message = "Không tìm thấy thú cưng." }); // (Chưa khai báo SĐT thì ẩn luôn)
                
                // (Verify ownership: Xác minh lại ở tầng DB xem pet này có đúng của khách này không)
                var pet = await _petService.GetPetByIdAsync(id, customerId.Value); 
                if (pet == null) return NotFound(new { message = "Không tìm thấy thú cưng." }); // (Không phải của mình thì báo lỗi 404)

                var records = await _medicalRecordService.GetPetMedicalHistoryAsync(id); // (Nhờ Service chui xuống Database quét toàn bộ bệnh án)
                return Ok(records); // (Gói dữ liệu thành JSON và trả về thành công)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // (Lỗi sập Server thì trả mã 500)
            }
        }
```

**Giải thích chi tiết:**
- `[HttpGet("{id}/medical-records")]`: Định tuyến URL chuẩn RESTful. Chữ `{id}` trên URL sẽ được tự động map vào biến `long id` của hàm.
- `[AuthorizeOwner]`: Chốt chặn bảo mật Vòng ngoài.
- `_petService.GetPetByIdAsync(...)`: Chốt chặn bảo mật Vòng trong (IDOR).
- `_medicalRecordService.GetPetMedicalHistoryAsync(id)`: Gọi tầng Service, đây là nơi chứa thuật toán bóc tách dữ liệu phức tạp.

```csharp
        /// <summary>
        /// Khách hàng xem lịch sử tiêm phòng của thú cưng.
        /// </summary>
        [HttpGet("{id}/vaccinations")] // (Đón Request GET tại URL: /api/mypets/5/vaccinations)
        [MyPetClinic.WebApi.Filters.AuthorizeOwner] // (Bảo mật chặn xem lén)
        public async Task<IActionResult> GetPetVaccinations(long id) // (Hàm: Lấy lịch sử tiêm)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync(); // (Lấy ID Khách hàng)
                if (customerId == null) return NotFound(new { message = "Không tìm thấy thú cưng." });
                
                var pet = await _petService.GetPetByIdAsync(id, customerId.Value); // (Kiểm tra xem pet có chính chủ không)
                if (pet == null) return NotFound(new { message = "Không tìm thấy thú cưng." });

                var vaccinations = await _vaccinationService.GetPetVaccinationHistoryAsync(id); // (Gọi Service Tiêm phòng quét DB)
                return Ok(vaccinations); // (Trả về JSON)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // (Bắt lỗi sập Server)
            }
        }
```
**Giải thích chi tiết:** Luồng đi giống hệt bệnh án, chỉ khác là gọi hàm `GetPetVaccinationHistoryAsync`. Cấu trúc viết lặp đi lặp lại chuẩn xác giúp dự án dễ bảo trì.

```csharp
        /// <summary>
        /// Khách hàng xem lịch sử đơn thuốc của thú cưng.
        /// </summary>
        [HttpGet("{id}/prescriptions")] // (Đón Request GET tại URL: /api/mypets/5/prescriptions)
        [MyPetClinic.WebApi.Filters.AuthorizeOwner] // (Bảo mật)
        public async Task<IActionResult> GetPetPrescriptions(long id) // (Hàm: Lấy lịch sử đơn thuốc)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync(); // (Lấy ID Khách hàng)
                if (customerId == null) return NotFound(new { message = "Không tìm thấy thú cưng." });
                
                var pet = await _petService.GetPetByIdAsync(id, customerId.Value); // (Kiểm tra xem pet có chính chủ không)
                if (pet == null) return NotFound(new { message = "Không tìm thấy thú cưng." });

                var prescriptions = await _prescriptionService.GetPetPrescriptionsAsync(id); // (Gọi Service Đơn thuốc)
                return Ok(prescriptions); // (Trả về JSON)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
```

---

### PHẦN 2.2 - TẦNG SERVICE (LẤY LỊCH SỬ KHÁM VÀ THUẬT TOÁN JOIN DỮ LIỆU)

**Tệp:** `MyPetClinic.Application/Services/MedicalRecordService.cs`

Đây là một trong những hàm phức tạp nhất vì nó phải lấy: Lịch sử khám + Bác sĩ + Đơn thuốc + Các viên thuốc lẻ bên trong đơn thuốc đó.

```csharp
        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId) // (Hàm: Quét lịch sử bệnh án)
        {
            // 1. Lấy tất cả bệnh án của thú cưng
            var records = await _unitOfWork.MedicalRecords.FindWithIncludesAsync(
                r => r.PetId == petId, // (Điều kiện: Chỉ lấy bệnh án của con thú cưng này)
                r => r.Appointment!, // (Kéo theo thông tin lịch hẹn)
                r => r.Doctor! // (Kéo theo thông tin bác sĩ khám)
            );

            // Sắp xếp giảm dần theo CreatedAt (Bệnh án mới nhất được đẩy lên đầu)
            records = records.OrderByDescending(r => r.CreatedAt).ToList();

            var recordIds = records.Select(r => r.Id).ToList(); // (Tạo 1 mảng chỉ chứa các ID bệnh án. Ví dụ: [10, 15, 20])
            if (!recordIds.Any()) // (Nếu mảng trống, tức là chưa khám lần nào)
            {
                return Enumerable.Empty<MedicalRecordDto>(); // (Trả về danh sách rỗng để FE không bị lỗi)
            }

            // 2. Lấy tất cả đơn thuốc của các bệnh án này
            var prescriptions = await _unitOfWork.Prescriptions.FindWithIncludesAsync(
                p => recordIds.Contains(p.MedicalRecordId) // (Lệnh IN trong SQL: SELECT * FROM Prescriptions WHERE MedicalRecordId IN (10, 15, 20))
            );

            var prescriptionIds = prescriptions.Select(p => p.Id).ToList(); // (Tạo mảng chứa các ID đơn thuốc)

            // 3. Lấy tất cả chi tiết đơn thuốc (từng loại thuốc)
            var prescriptionItems = prescriptionIds.Any()
                ? await _unitOfWork.PrescriptionItems.FindWithIncludesAsync(
                    pi => prescriptionIds.Contains(pi.PrescriptionId), // (Lấy các viên thuốc nằm trong các đơn thuốc trên)
                    pi => pi.Medicine! // (Kéo theo thông tin Tên Thuốc từ bảng Medicine)
                  )
                : Enumerable.Empty<PrescriptionItem>(); // (Nếu không có đơn thuốc thì gán bằng rỗng)

            // (THUẬT TOÁN GOM NHÓM DỮ LIỆU BẰNG LINQ GROUP BY BỘ NHỚ RAM)
            var itemsGrouped = prescriptionItems.GroupBy(pi => pi.PrescriptionId) // (Nhóm các viên thuốc lại theo ID đơn thuốc)
                .ToDictionary(g => g.Key, g => g.ToList()); // (Biến thành cấu trúc Dictionary <Mã đơn thuốc, List<Viên thuốc>>)

            var prescriptionsGrouped = prescriptions.GroupBy(p => p.MedicalRecordId) // (Nhóm đơn thuốc theo ID bệnh án)
                .ToDictionary(g => g.Key, g => g.ToList()); // (Dictionary <Mã bệnh án, List<Đơn thuốc>>)

            var result = new List<MedicalRecordDto>(); // (Khởi tạo cái giỏ trống chuẩn bị trả về)

            // (Duyệt qua từng hồ sơ bệnh án)
            foreach (var r in records)
            {
                var prescribedMedicines = new List<PrescribedMedicineDto>(); // (Tạo giỏ nhỏ chứa thuốc)
                
                // (TÌM KIẾM NHANH TRONG DICTIONARY O(1): Xem bệnh án này có đơn thuốc nào không)
                if (prescriptionsGrouped.TryGetValue(r.Id, out var recordPrescriptions))
                {
                    foreach (var p in recordPrescriptions) // (Duyệt qua từng đơn thuốc)
                    {
                        if (itemsGrouped.TryGetValue(p.Id, out var items)) // (Xem đơn thuốc này có những viên thuốc nào)
                        {
                            foreach (var pi in items) // (Duyệt qua từng loại thuốc)
                            {
                                prescribedMedicines.Add(new PrescribedMedicineDto // (Đóng gói thuốc vào thùng DTO)
                                {
                                    MedicineName = pi.Medicine?.Name ?? "Thuốc",
                                    Dosage = pi.Dosage,
                                    Frequency = pi.Frequency,
                                    DurationDays = pi.DurationDays,
                                    Quantity = pi.Quantity,
                                    Instruction = pi.Instruction
                                });
                            }
                        }
                    }
                }

                result.Add(new MedicalRecordDto // (Đóng gói toàn bộ Bệnh án + Thuốc vào thùng to)
                {
                    RecordId = r.Id,
                    AppointmentId = r.AppointmentId,
                    PetId = r.PetId,
                    PetName = r.Appointment?.Pet?.Name ?? string.Empty, // (Lấy tên pet, nếu null thì để rỗng)
                    VisitDate = r.CreatedAt,
                    RecordType = r.RecordType,
                    
                    // (Sử dụng hàm giải mã nội dung JSON SOAP thành tiếng Việt dễ đọc cho khách hàng)
                    MedicalHistory = ExtractReadableSoap(r.MedicalHistory, "S"), 
                    Diagnosis = ExtractReadableSoap(r.Diagnosis, "A"),
                    TreatmentPlan = ExtractReadableSoap(r.TreatmentPlan, "P"),
                    
                    DoctorName = r.Doctor?.FullName ?? string.Empty,
                    DoctorId = r.DoctorId.ToString(),
                    Weight = r.Weight,
                    Temperature = r.Temperature,
                    ClinicalSigns = ExtractReadableSoap(r.ClinicalSigns, "O"),
                    DoctorNotes = r.DoctorNotes ?? string.Empty,
                    FollowUpDate = r.FollowUpDate,
                    PrescribedMedicines = prescribedMedicines // (Nhét giỏ thuốc đã gom được vào đây)
                });
            }

            return result; // (Xong! Trả hàng về Controller)
        }
```

**Giải thích chi tiết (Tại sao code lại dài và rườm rà như vậy?):**
- **Vấn đề N+1 Query:** Nếu duyệt vòng lặp `foreach` từng bệnh án rồi trong đó gọi SQL tìm đơn thuốc, ta sẽ bắn hàng trăm câu truy vấn nhỏ xuống SQL Server (lỗi N+1) làm kẹt mạng.
- **Giải pháp Dictionary Grouping:** Code ở đây dùng thủ thuật tối ưu vô cùng thông minh. Nó bắn đúng 3 câu lệnh SELECT to (`records`, `prescriptions`, `prescriptionItems`). Kéo toàn bộ lên RAM (vì chỉ lọc cho 1 con thú cưng nên bộ nhớ rất nhẹ). Sau đó dùng `GroupBy()` chuyển sang `Dictionary` (Key-Value) để tra cứu chéo tốc độ cực nhanh `O(1)`. Hàm `TryGetValue` sẽ lắp ghép các dữ liệu liên quan lại với nhau mà không tốn thêm bất kỳ câu truy vấn mạng nào.
- Dòng `ExtractReadableSoap`: Gọi một hàm nội bộ để bóc các dữ liệu chuyên khoa lằng nhằng của bác sĩ (Dạng chuỗi JSON) sang dạng text Tiếng Việt thuần túy cho khách hàng đọc hiểu trên điện thoại.

---

### PHẦN 2.3 - TẦNG SERVICE (LỊCH SỬ ĐƠN THUỐC VÀ THUẬT TOÁN TÍNH NGÀY)

**Tệp:** `MyPetClinic.Application/Services/PrescriptionService.cs`

Đoạn code này chứa một logic thú vị: Tính xem đơn thuốc này còn "Hiệu lực" (Đang dùng) hay "Hết hạn" (Đã hoàn tất quá trình uống).

```csharp
        public async Task<IEnumerable<PrescriptionDto>> GetPetPrescriptionsAsync(long petId) // (Hàm: Lấy lịch sử đơn thuốc)
        {
            // (Chạy truy vấn INCLUDE chuỗi - Lấy Đơn thuốc JOIN Bệnh Án JOIN Lịch hẹn JOIN Bác sĩ JOIN Chi tiết thuốc)
            var prescriptions = await _unitOfWork.Prescriptions.Query()
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m!.Appointment) // (Dùng ThenInclude để đi sâu hơn 1 cấp vào trong)
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(pi => pi.Medicine)
                .Where(p => p.MedicalRecord != null
                         && p.MedicalRecord.Appointment != null
                         && p.MedicalRecord.Appointment.PetId == petId) // (Lọc đúng thú cưng)
                .OrderByDescending(p => p.CreatedAt) // (Mới nhất lên đầu)
                .ToListAsync(); // (Chốt lệnh, ép thực thi SQL)

            var result = new List<PrescriptionDto>();

            foreach (var prescription in prescriptions)
            {
                var dto = MapToDto(prescription); // (Gọi hàm đóng gói)
                result.Add(dto);
            }

            return result;
        }
```

**Giải thích chi tiết:**
- `Include().ThenInclude()`: Là cú pháp của Entity Framework Core để viết câu lệnh `INNER JOIN` liên hoàn giữa 5 bảng dữ liệu trong SQL Server mà không cần viết lệnh SQL chay.
- `Where(...)`: Tìm đúng những đơn thuốc liên kết với Bệnh án, mà Bệnh án đó lại thuộc về `petId`.

```csharp
        // -----------------------------------------------------------------------
        // Business Rule (Quy tắc kinh doanh): Tính trạng thái đơn thuốc
        //   - "active"    : đơn còn trong thời gian dùng thuốc (chưa hết hạn)
        //   - "completed" : đơn đã hết thời gian dùng thuốc
        // Công thức: ngày kê đơn + số ngày dùng thuốc dài nhất >= hôm nay
        // -----------------------------------------------------------------------
        private static PrescriptionDto MapToDto(Domain.Entities.Prescription prescription) // (Hàm: Chuyển đổi và Tính toán)
        {
            var dto = new PrescriptionDto // (Tạo thùng hàng DTO)
            {
                Id        = prescription.Id,
                Diagnosis = prescription.MedicalRecord?.Diagnosis ?? string.Empty, // (Lấy bệnh lý chẩn đoán)
                Date      = prescription.CreatedAt,
                Doctor    = prescription.Doctor != null ? $"BS. {prescription.Doctor.FullName}" : "Bác sĩ thú y",
                Notes     = prescription.Note,
                Status    = "completed" // (Gắn mặc định là completed trước)
            };

            int maxDurationDays = 0; // (Biến đếm: Số ngày uống thuốc lâu nhất)

            foreach (var item in prescription.PrescriptionItems) // (Duyệt qua các loại thuốc trong đơn)
            {
                // (Thuật toán tìm số lớn nhất: Nếu loại thuốc này uống lâu ngày hơn số max hiện tại thì cập nhật số max)
                if (item.DurationDays.HasValue && item.DurationDays.Value > maxDurationDays)
                    maxDurationDays = item.DurationDays.Value; 

                dto.Medicines.Add(new PrescriptionItemDto // (Đóng gói viên thuốc)
                {
                    Id              = item.Id,
                    MedicineId      = item.MedicineId,
                    Name            = item.Medicine?.Name ?? "Thuốc không xác định",
                    ActiveIngredient = item.Medicine?.Description ?? string.Empty,
                    Dosage          = item.Dosage,
                    Frequency       = item.Frequency,
                    DurationDays    = item.DurationDays,
                    Usage           = item.Instruction,
                    Quantity        = item.Quantity,
                    Unit            = item.Medicine?.Unit ?? "Đơn vị"
                });
            }

            // Business rule: Đơn còn hiệu lực nếu ngày kê đơn + số ngày uống >= hôm nay (UTC)
            if (maxDurationDays > 0 && prescription.CreatedAt.AddDays(maxDurationDays) >= DateTime.UtcNow)
            {
                dto.Status = "active"; // (Đổi cờ thành active - hiển thị nhãn xanh trên giao diện)
            }

            return dto;
        }
```

**Giải thích chi tiết:**
- `int maxDurationDays = 0;`: Khởi tạo biến tìm số lớn nhất. Tại sao phải làm vậy? Vì một đơn thuốc có nhiều loại. Loại A uống 3 ngày, Loại B uống 7 ngày, Loại C uống 5 ngày. Số ngày hiệu lực của **Cả cái Đơn thuốc** phụ thuộc vào loại thuốc uống lâu nhất (7 ngày).
- `item.DurationDays.Value > maxDurationDays`: Vòng lặp tìm ra số ngày 7 đó gắn vào `maxDurationDays`.
- `prescription.CreatedAt.AddDays(maxDurationDays) >= DateTime.UtcNow`: Đây là dòng code cực hay. Nó lấy Ngày tạo đơn thuốc (Ví dụ mùng 1) cộng thêm 7 ngày (thành mùng 8). Sau đó đem so với ngày hôm nay (`DateTime.UtcNow`). Nếu hôm nay là mùng 6 (nhỏ hơn mùng 8), nghĩa là thú cưng VẪN ĐANG TRONG THỜI GIAN UỐNG THUỐC. Cờ sẽ đổi thành `active`. Trên Frontend sẽ sáng đèn màu Xanh lá cây lên báo cho khách hàng biết.

---
*(Hết tài liệu đào tạo chuyên sâu Hồ sơ y tế Thú cưng - Giải thích tường tận từng dòng code)*
