# TÀI LIỆU ĐÀO TẠO NỘI BỘ: QUẢN TRỊ KHO VẮC-XIN (ADMIN VIEW) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 18, dành riêng cho **Quản trị viên (Admin)**.
> Mặc dù Vắc-xin cũng là một dạng "Thuốc", nhưng trong kiến trúc hệ thống MyPetClinic, nó được tách riêng hoàn toàn thành một mô-đun độc lập. 
> Trọng tâm tài liệu này phân tích 2 điểm khác biệt sống còn: **Cấu trúc dữ liệu Phi chuẩn hóa (Denormalization)** và **Các tham số thiết lập Phác đồ tiêm chủng**.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Danh mục & Lô Vắc-xin (Vaccines & Vaccine Batches).
- **Mục đích:** Thêm các loại vắc-xin (Ví dụ: Vắc-xin 5 bệnh chó, 7 bệnh mèo) và nhập kho theo từng Lô (Batch).
- **Điểm nổi bật (Kỹ thuật):** Khác với Thuốc (dùng `[NotMapped]` tính tổng kho khi query), Vắc-xin sử dụng kỹ thuật **Denormalization (Lưu cứng Tồn Kho)**. Đồng thời bổ sung các tham số `MinAgeWeeks` và `IntervalDays` để làm nền tảng cho Thuật toán Nhắc lịch tiêm tự động của Bác sĩ.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - THỰC THỂ VẮC-XIN: THÔNG SỐ PHÁC ĐỒ TIÊM

Khác với viên thuốc uống xong là hết, Vắc-xin phải tuân thủ nghiêm ngặt theo **Phác đồ Y tế**. Hệ thống phải lưu lại độ tuổi tối thiểu và khoảng cách giữa 2 mũi tiêm.

**Tệp:** `MyPetClinic.Domain/Entities/Vaccine.cs`

```csharp
    public class Vaccine
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty; // VD: Vanguard Plus 5
        public string? Manufacturer { get; set; } // Hãng: Zoetis
        
        // [ĐIỂM KHÁC BIỆT 1]: Thuộc tính Lưu cứng (Không có NotMapped)
        public int StockQuantity { get; set; } = 0; // Aggregated from batches
        
        // [ĐIỂM KHÁC BIỆT 2]: Các tham số Y tế Thú y (Veterinary Parameters)
        public string? TargetSpecies { get; set; } // Chỉ tiêm cho: "Dog", "Cat", "All"
        public int? MinAgeWeeks { get; set; }      // Tuổi tối thiểu: Ví dụ 6 tuần tuổi
        public int? IntervalDays { get; set; }     // Khoảng cách tiêm nhắc lại: Ví dụ 21 ngày

        // Quan hệ 1-N
        public ICollection<VaccineBatch> VaccineBatches { get; set; } = new List<VaccineBatch>();
    }
```

**Giải thích chi tiết (Veterinary Parameters):**
- **MinAgeWeeks (Tuổi tối thiểu):** Khi Bác sĩ kê đơn tiêm mũi Vanguard 5 cho một chú chó con 4 tuần tuổi, hệ thống sẽ đọc biến `MinAgeWeeks = 6` và ngay lập tức ném ra cảnh báo Đỏ: *"Chó con chưa đủ 6 tuần tuổi, hệ miễn dịch quá yếu, cấm tiêm!"*
- **IntervalDays (Khoảng cách ngày):** Nếu mũi 1 tiêm ngày 1/1, hệ thống lấy `1/1 + IntervalDays (21)` để tự động tạo lịch hẹn Mũi 2 vào ngày 22/1. Đây là lõi logic sinh ra doanh thu thụ động cho Phòng khám!

---

### PHẦN 2.2 - LÔ VẮC-XIN (VACCINE BATCH) & KỸ THUẬT DENORMALIZATION

**Tệp:** `MyPetClinic.Domain/Entities/VaccineBatch.cs`

```csharp
    public class VaccineBatch
    {
        public long Id { get; set; }
        public long VaccineId { get; set; }
        public string BatchNumber { get; set; } = string.Empty; // Số Lô Vắc-xin
        
        public DateTime ExpirationDate { get; set; } // Hạn sử dụng (Bắt buộc)
        public DateTime ImportDate { get; set; } = DateTime.UtcNow; // Ngày nhập
        
        public int StockQuantity { get; set; } = 0; // Số lượng tồn của riêng Lô này
        
        public decimal ImportPrice { get; set; } // Giá nhập
        public decimal SellingPrice { get; set; } // Giá chích
    }
```

**Tại sao Vắc-xin lại lưu cứng `StockQuantity` (Denormalization) thay vì dùng `[NotMapped]` như Thuốc?**
- **Thuốc (Medicines):** Giao dịch nhập/xuất diễn ra hàng trăm lần mỗi ngày (1 ca khám bán ra 5-6 loại thuốc x 30 ca/ngày). Nếu lưu cứng, database sẽ bị lock (khóa) liên tục vì Update quá nhiều. Do đó ta dùng `[NotMapped]` để Sum().
- **Vắc-xin (Vaccines):** Giao dịch ít hơn rất nhiều, nhưng Vắc-xin thường xuất hiện trên trang chủ hoặc App Khách hàng để khách book lịch. Kỹ thuật **Phi chuẩn hóa (Denormalization)** - tức là chủ động lưu thừa cột `StockQuantity` vào bảng mẹ - sẽ giúp API Get Danh sách Vắc-xin chạy nhanh hơn x5 lần vì không cần Join xuống bảng Batch để tính Sum()!

---

### PHẦN 2.3 - LOGIC NHẬP LÔ VẮC-XIN (CỘNG DỒN THỦ CÔNG)

Bởi vì ta đã chọn kỹ thuật Denormalization, nên mỗi khi tạo Lô mới, Backend phải dùng "Cơm" để tự động cộng dồn lên bảng mẹ.

**Tệp:** `MyPetClinic.Application/Services/AdminService.cs`

```csharp
        public async Task<VaccineBatchAdminDto> CreateVaccineBatchAsync(long vaccineId, CreateVaccineBatchDto dto, string currentUserId)
        {
            // 1. TÌM VẮC-XIN MẸ
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(vaccineId) ?? throw new KeyNotFoundException("Không tìm thấy vắc-xin.");

            // 2. TẠO LÔ MỚI
            var batch = new VaccineBatch
            {
                VaccineId = vaccineId,
                BatchNumber = dto.BatchNumber,
                ExpirationDate = DateTime.SpecifyKind(dto.ExpirationDate, DateTimeKind.Utc), // Đưa về chuẩn UTC toàn cầu
                ImportDate = dto.ImportDate ?? DateTime.UtcNow,
                StockQuantity = dto.StockQuantity,
                ImportPrice = dto.ImportPrice,
                SellingPrice = dto.SellingPrice
            };

            await _unitOfWork.VaccineBatches.AddAsync(batch);
            
            // 3. [ĐIỂM MẤU CHỐT]: CẬP NHẬT TỒN KHO LÊN BẢNG MẸ
            // (Đồng bộ hóa dữ liệu Denormalized)
            vaccine.StockQuantity += batch.StockQuantity;
            _unitOfWork.Vaccines.Update(vaccine);

            await _unitOfWork.SaveChangesAsync();

            // 4. GHI NHẬT KÝ KIỂM TOÁN (AUDIT TRAIL)
            await _auditLogService.LogActionAsync(currentUserId, "CreateVaccineBatch", $"Nhập lô vắc-xin mới {dto.BatchNumber} cho vắc-xin ID {vaccineId}");
            
            return new VaccineBatchAdminDto { ... };
        }
```

---

### PHẦN 2.4 - LOGIC RÀO CHẮN KHI XÓA VẮC-XIN

Lỗ hổng chết người của các phần mềm giá rẻ là: Lô hàng vẫn còn 100 chai Vắc-xin, nhưng Admin lỡ tay bấm Xóa Vắc-xin. Hậu quả là 100 chai đó thành "Bóng ma", trôi nổi trong kho mà không ai biết, làm thâm hụt tài sản phòng khám!

```csharp
        public async Task DeleteVaccineBatchAsync(long batchId, string currentUserId)
        {
            var batch = await _unitOfWork.VaccineBatches.GetByIdAsync(batchId) ?? throw new KeyNotFoundException("Không tìm thấy lô vắc-xin.");
            
            // LUẬT RÀO CHẮN TÀI SẢN (ASSET PROTECTION)
            if (batch.StockQuantity > 0)
            {
                throw new InvalidOperationException("Không thể xoá lô vắc-xin vẫn còn tồn kho. Hãy dùng tính năng huỷ hàng hỏng/hết hạn nếu cần.");
            }

            _unitOfWork.VaccineBatches.Remove(batch);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(currentUserId, "DeleteVaccineBatch", $"Xoá lô vắc-xin ID {batchId}");
        }
```

**Giải thích chi tiết:**
- Lệnh `if (batch.StockQuantity > 0)` là tấm khiên bảo vệ tài sản (Asset Protection). 
- Nó ép nhân viên Kho: Nếu vắc-xin bị bể/hết hạn, mày phải làm "Phiếu Xuất Hủy" đàng hoàng để Kế toán trừ tiền, chứ không được bấm nút `Delete` xóa cái rụp hòng phi tang chứng cứ!

---
*(Hết tài liệu đào tạo chuyên sâu Admin: Kho Vắc-xin)*
