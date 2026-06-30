# TÀI LIỆU ĐÀO TẠO NỘI BỘ: QUẢN TRỊ KHO THUỐC & VẮC-XIN (ADMIN VIEW) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 17, dành riêng cho **Quản trị viên (Admin)** và **Lễ tân/Thu ngân**.
> Quản lý Kho Y tế (Thuốc, Vắc-xin) phức tạp gấp 10 lần hệ thống Bán lẻ thông thường (Shopee, Tiki). Ta không chỉ cộng/trừ số lượng, mà bắt buộc phải quản lý theo **Lô hàng (Batch)**, truy vết **Hạn sử dụng (Expiry Date)** và tuân thủ nguyên tắc **FEFO (First-Expired, First-Out: Hết hạn trước thì lấy ra bán trước)**.
> Trọng tâm tài liệu này bao gồm 3 Thực thể liên hoàn, kỹ thuật `[NotMapped]` C# và thuật toán trừ FEFO.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Kho Thuốc / Vắc-xin (Medicine & Vaccine Inventory).
- **Mục đích:** Nhập kho, xuất kho, kiểm kê lệch kho và cảnh báo thuốc sắp hết hạn.
- **Điểm nổi bật (Kỹ thuật):** Hệ thống không bao giờ lưu cứng cột `StockQuantity` trong bảng `Medicine` để tránh "dữ liệu bị rác". Mọi thao tác nhập/xuất đều sinh ra 1 phiếu `InventoryTransaction` để đảm bảo Truy xuất Nguồn gốc (Traceability) - chuẩn bộ Y tế.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - BA THỰC THỂ CỐT LÕI (ENTITY)

Đây là xương sống của toàn bộ hệ thống Kho. Dữ liệu được chia làm 3 tầng: 
Tầng Danh mục (`Medicine`) -> Tầng Lô Hàng (`MedicineBatch`) -> Tầng Biến Động (`InventoryTransaction`).

**Tệp:** `MyPetClinic.Domain/Entities/Medicine.cs` & `MedicineBatch.cs` & `InventoryTransaction.cs`

```csharp
// --- TẦNG 1: DANH MỤC THUỐC ---
    public class Medicine
    {
        public long Id { get; set; }
        public string MedicineCode { get; set; } = string.Empty; // VD: MED-240101
        public string Name { get; set; } = string.Empty; // VD: Thuốc giun Sát thủ
        
        // CÁC THÔNG SỐ CƠ BẢN
        public string Unit { get; set; } = string.Empty; // Đơn vị: Viên, Hộp, Lọ
        public int MinStockLevel { get; set; } = 0; // Ngưỡng cảnh báo sắp hết hàng
        
        // QUAN HỆ 1-N VỚI LÔ HÀNG VÀ LỊCH SỬ
        public ICollection<MedicineBatch> Batches { get; set; } = new List<MedicineBatch>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

        // [QUAN TRỌNG NHẤT]: Thuộc tính Ảo (NotMapped)
        // Tổng tồn kho không lưu vào DB, mà tính sống (Sum) từ tất cả các Lô cộng lại
        [NotMapped]
        public int StockQuantity { get { return Batches?.Sum(b => b.CurrentQuantity) ?? 0; } set { } }
        
        // Hạn sử dụng của Thuốc = Hạn sử dụng của cái Lô cận Date nhất
        [NotMapped]
        public DateTime? ExpiryDate { get { return Batches?.OrderBy(b => b.ExpiryDate).FirstOrDefault()?.ExpiryDate; } set { } }
    }

// --- TẦNG 2: LÔ HÀNG THUỐC (BATCH) ---
    public class MedicineBatch
    {
        public long Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty; // Số lô (VD: L-2026A)
        public long MedicineId { get; set; } // Khóa ngoại trỏ về Tầng 1
        
        public DateTime ManufactureDate { get; set; } // Ngày sản xuất
        public DateTime ExpiryDate { get; set; } // Ngày hết hạn (Rất quan trọng)

        public int InitialQuantity { get; set; } // Số lượng lúc mới nhập về
        public int CurrentQuantity { get; set; } // Số lượng còn lại hiện tại của lô này
    }

// --- TẦNG 3: NHẬT KÝ BIẾN ĐỘNG KHO (SỔ CÁI) ---
    public class InventoryTransaction
    {
        public long Id { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow; // Giao dịch lúc mấy giờ?
        public InventoryTransactionType Type { get; set; } // Enum: Nhập (Receipt) hay Xuất (Dispense)
        
        public long MedicineId { get; set; }
        public long? BatchId { get; set; } // Giao dịch này đụng vào Lô nào?
        public int QuantityChange { get; set; } // Biến động (VD: Nhập +10, Xuất -2)
        
        public Guid CreatedByUserId { get; set; } // Ai là người thực hiện?
    }
```

**Giải thích chi tiết (`[NotMapped]`):**
- Trong các đồ án sinh viên, người ta hay thêm thẳng cột `int TồnKho` vào bảng Thuốc. Hậu quả là sau vài tháng, dữ liệu `TồnKho = 50` nhưng Lô hàng thì chỉ còn 45 viên (Do ai đó sửa bằng tay hoặc hệ thống lỗi cúp điện). Dữ liệu lệch nhau hoàn toàn!
- Ở hệ thống chuyên nghiệp MyPetClinic, ta dùng Attribute `[NotMapped]`. Entity Framework sẽ **Bỏ qua không tạo cột này trong SQL**. Mỗi khi gọi `Medicine.StockQuantity`, C# sẽ vòng qua hàm `Sum()` để cộng số lượng thực tế của các `MedicineBatch` lại. Tồn kho KHÔNG BAO GIỜ có thể sai lệch!

---

### PHẦN 2.2 - GÓI DỮ LIỆU DTO CHO NHẬP/XUẤT KHO

**Tệp:** `MyPetClinic.Application/DTOs/InventoryDtos.cs`

```csharp
    // (DTO Truyền vào khi Admin bấm "Nhập kho")
    public class ImportMedicineDto
    {
        public long MedicineId { get; set; }
        public string BatchNumber { get; set; } = string.Empty; // Admin cầm hộp thuốc đọc số Lô gõ vào
        public DateTime ManufactureDate { get; set; } 
        public DateTime ExpiryDate { get; set; } 
        public int Quantity { get; set; } // Nhập mấy hộp?
        public string? Notes { get; set; }
    }

    // (DTO Truyền vào khi Bác sĩ "Kê đơn" hoặc Thu ngân "Bán lẻ")
    public class ExportMedicineDto
    {
        public long MedicineId { get; set; } // Chỉ cần truyền tên Thuốc
        public int Quantity { get; set; } // Truyền số lượng cần bán (KHÔNG CẦN TRUYỀN SỐ LÔ, HỆ THỐNG SẼ TỰ CHỌN LÔ BẰNG THUẬT TOÁN)
    }
```

---

### PHẦN 2.3 - LOGIC NHẬP KHO (KIỂM TRA HSD & GHI SỔ CÁI)

Khi Admin/Thủ kho mang 1 thùng hàng mới về.

**Tệp:** `MyPetClinic.Application/Services/MedicineService.cs`

```csharp
        public async Task ImportMedicineAsync(ImportMedicineDto dto, Guid userId)
        {
            await _unitOfWork.BeginTransactionAsync(); // Khóa an toàn

            // 1. CHẶN LỖI NGỚ NGẨN BẰNG VALIDATION
            if (dto.ExpiryDate <= DateTime.UtcNow)
                throw new Exception("Không thể nhập lô thuốc đã hết hạn.");
            if (dto.ManufactureDate > DateTime.UtcNow)
                throw new Exception("Ngày sản xuất không hợp lệ.");

            // 2. KIỂM TRA LÔ (Nếu lô cũ đã có -> Cộng dồn, Nếu Lô mới -> Tạo mới)
            var existingBatch = await _batchRepo.GetBatchByNumberAsync(dto.BatchNumber);
            MedicineBatch batch;

            if (existingBatch != null)
            {
                batch = existingBatch;
                batch.CurrentQuantity += dto.Quantity;
                _unitOfWork.MedicineBatches.Update(batch);
            }
            else
            {
                batch = new MedicineBatch
                {
                    BatchNumber = dto.BatchNumber,
                    MedicineId = dto.MedicineId,
                    ExpiryDate = dto.ExpiryDate,
                    CurrentQuantity = dto.Quantity
                    // ...
                };
                await _unitOfWork.MedicineBatches.AddAsync(batch);
            }
            await _unitOfWork.SaveChangesAsync();

            // 3. GHI SỔ CÁI (INVENTORY TRANSACTION) DÀNH CHO KẾ TOÁN/KIỂM TOÁN
            var transaction = new InventoryTransaction
            {
                Type = InventoryTransactionType.GoodsReceipt, // Phiếu nhập
                MedicineId = dto.MedicineId,
                BatchId = batch.Id, // Nhập vào Lô nào?
                QuantityChange = dto.Quantity, // Dương (Cộng kho)
                CreatedByUserId = userId,
            };
            await _unitOfWork.InventoryTransactions.AddAsync(transaction);
            
            await _unitOfWork.CommitTransactionAsync();
        }
```

---

### PHẦN 2.4 - THUẬT TOÁN XUẤT KHO FEFO (TRÁI TIM CỦA KHO Y TẾ)

Khi Lễ tân bán đi `15 Hộp Thuốc Tẩy Giun`, phần mềm không trừ bừa bãi. Lễ tân KHÔNG CẦN biết kho có mấy lô. Hệ thống sẽ tự dùng Thuật toán **FEFO (Hết hạn trước - Lấy trước)**.

**Tệp:** `MyPetClinic.Infrastructure/Repositories/MedicineBatchRepository.cs`
*(Truy vấn lấy Lô hàng ra khỏi DB)*
```csharp
        public async Task<IEnumerable<MedicineBatch>> GetAvailableBatchesAsync(long medicineId)
        {
            return await _context.MedicineBatches
                .Where(b => b.MedicineId == medicineId && b.CurrentQuantity > 0)
                .OrderBy(b => b.ExpiryDate) // [THUẬT TOÁN FEFO]: OrderBy ASC cái Hạn sử dụng, cái nào Cận Date nhất sẽ nổi lên đầu tiên!
                .ToListAsync();
        }
```

**Tệp:** `MyPetClinic.Application/Services/MedicineService.cs`
*(Logic trừ dần qua các Lô)*
```csharp
        public async Task ExportMedicineAsync(ExportMedicineDto dto, Guid userId)
        {
            // (1. Lấy danh sách lô theo thứ tự FEFO)
            var availableBatches = await _batchRepo.GetAvailableBatchesAsync(dto.MedicineId);
            
            int remainingToExport = dto.Quantity; // Cần xuất 15 hộp

            // (2. Vòng lặp trừ dần (Drip-feed Algorithm))
            foreach (var batch in availableBatches)
            {
                if (remainingToExport <= 0) break; // Xuất đủ 15 hộp thì dừng

                // VD: Lô 1 (Cận Date) chỉ còn 10 hộp -> Lấy hết 10 hộp của Lô 1. 
                // Vẫn thiếu 5 hộp -> Vòng lặp chạy tiếp sang Lô 2, lấy 5 hộp từ Lô 2!
                int takeAmount = Math.Min(batch.CurrentQuantity, remainingToExport);
                batch.CurrentQuantity -= takeAmount;
                remainingToExport -= takeAmount;

                _unitOfWork.MedicineBatches.Update(batch);

                // (3. Ghi Sổ cái Xuất Kho cho TỪNG LÔ)
                var transaction = new InventoryTransaction
                {
                    Type = InventoryTransactionType.PrescriptionDispense,
                    MedicineId = dto.MedicineId,
                    BatchId = batch.Id,
                    QuantityChange = -takeAmount, // Số Âm (Trừ kho)
                    CreatedByUserId = userId,
                };
                await _unitOfWork.InventoryTransactions.AddAsync(transaction);
            }

            if (remainingToExport > 0)
                throw new Exception($"Lỗi hệ thống: Số lượng lô không đủ để xuất ({remainingToExport} thiếu).");
                
            // ...
        }
```

**Giải thích chi tiết:**
Đây là thuật toán đỉnh cao giúp Phòng khám Tiết kiệm hàng chục triệu đồng mỗi năm. Hệ thống luôn lấy thuốc cận Date ra xuất, giúp tránh tình trạng nhân viên lấy nhầm lô thuốc mới nhập (khiến lô thuốc cũ nằm đóng bụi đến hết hạn và phải vứt bỏ).

---

### PHẦN 2.5 - CẢNH BÁO TỒN KHO VÀ CẬN DATE

Nhờ cấu trúc `[NotMapped]` kết hợp với truy vấn Linq, API báo cáo cho Admin dễ như ăn kẹo:

```csharp
        public async Task<IEnumerable<ExpiringMedicineDto>> GetExpiringMedicinesAsync(int daysThreshold)
        {
            var targetDate = DateTime.UtcNow.AddDays(daysThreshold); // VD: Check trong 30 ngày tới
            var medicines = await _medicineRepo.GetMedicinesWithStockAsync();
            var expiringList = new List<ExpiringMedicineDto>();

            // Quét qua toàn bộ kho thuốc, lô nào Expired Date sắp tới gần thì nhét vào List ném ra UI để UI hiện màu Đỏ khẩn cấp!
            foreach (var medicine in medicines)
            {
                var batches = await _batchRepo.GetAvailableBatchesAsync(medicine.Id);
                foreach (var batch in batches)
                {
                    if (batch.ExpiryDate <= targetDate)
                    {
                        expiringList.Add(new ExpiringMedicineDto { ... }); // Cảnh báo Đỏ
                    }
                }
            }
            // ...
        }
```

---
*(Hết tài liệu đào tạo chuyên sâu Admin: Kho Thuốc FEFO)*
