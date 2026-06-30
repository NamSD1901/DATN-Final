# TÀI LIỆU ĐÀO TẠO NỘI BỘ: BÁO CÁO KẾ TOÁN DOANH THU (ADMIN VIEW) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 19, dành riêng cho **Quản trị viên (Admin)** và **Giám đốc Phòng khám**.
> Báo cáo doanh thu là nơi xử lý lượng dữ liệu khổng lồ nhất hệ thống (Hàng vạn hóa đơn, hàng trăm ngàn lượt khám). Nếu Code không cẩn thận, Server sẽ cạn kiệt RAM và sập lập tức.
> Trọng tâm tài liệu này phân tích **Kỹ thuật Offload tính toán xuống Database Server** (Dùng LINQ GroupBy) và nghệ thuật đóng gói DTO đa chiều.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Báo cáo Tổng hợp Doanh thu & Hiệu suất (Revenue & Performance Reports).
- **Mục đích:** Tính toán tổng doanh thu, doanh thu theo từng ngày, mặt hàng bán chạy nhất (Thuốc/Dịch vụ), và KPI xếp hạng Bác sĩ làm việc hiệu quả nhất.
- **Điểm nổi bật (Kỹ thuật):** Tuân thủ tuyệt đối **Quy tắc 2 (Tối ưu hóa Database)**: Ép toàn bộ các hàm `Sum()`, `Count()`, `GroupBy()` chạy bằng SQL Native dưới tầng Database Server. Server C# (Backend) chỉ nhận kết quả đã tính toán xong (vài byte dữ liệu), tuyệt đối không tải danh sách thô lên bộ nhớ RAM!

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - CẤU TRÚC DỮ LIỆU ĐA CHIỀU (NESTED DTO)

Dashboard (Bảng điều khiển) của Giám đốc chứa 4 biểu đồ khác nhau. Thay vì bắt Frontend phải gọi 4 API rời rạc gây nghẽn mạng, ta đóng gói tất cả vào 1 chiếc "Container" khổng lồ mang tên `RevenueReportDto`.

**Tệp:** `MyPetClinic.Application/DTOs/RevenueReportDto.cs`

```csharp
    // (1. CÁI RƯƠNG CHỨA TỔNG - Gửi cho Frontend 1 lần duy nhất)
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; } // Tổng tiền 
        public int InvoicesCount { get; set; }    // Tổng số hóa đơn
        
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new List<DailyRevenueDto>(); // Biểu đồ đường (Line Chart)
        public List<ServiceRevenueDto> ServiceRevenue { get; set; } = new List<ServiceRevenueDto>(); // Biểu đồ tròn (Pie Chart)
        public List<DoctorPerformanceDto> DoctorPerformance { get; set; } = new List<DoctorPerformanceDto>(); // Bảng Xếp Hạng Bác Sĩ
    }

    // (2. DTO Biểu đồ đường: Doanh thu theo từng ngày)
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; } // Ngày hôm đó có mấy khách?
    }

    // (3. DTO Biểu đồ tròn: Dịch vụ nào hái ra tiền nhất?)
    public class ServiceRevenueDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    // (4. DTO Bảng Xếp hạng KPI)
    public class DoctorPerformanceDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public int CompletedAppointments { get; set; } // Số ca khám thành công
    }
```

---

### PHẦN 2.2 - XỬ LÝ TIMEZONE (UTC) ĐỂ TRÁNH LỆCH NGÀY

Giám đốc chọn xem báo cáo từ ngày `01/10` đến `31/10`. Nếu không cẩn thận, Hóa đơn lúc 23h59 ngày 31/10 sẽ bị bỏ sót!

**Tệp:** `MyPetClinic.Application/Services/ReportService.cs`

```csharp
        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            // BƯỚC 1: Ép chuẩn TimeZone (Giờ thế giới) và nới rộng EndDate
            var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            
            // CỰC KỲ QUAN TRỌNG: Lấy ngày EndDate, cộng thêm 1 ngày, sau đó lùi lại 1 Tick (1 phần 10 triệu giây) 
            // => Kết quả: Chính xác 23:59:59.9999 của ngày EndDate!
            var endUtc   = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            
            // ...
        }
```

---

### PHẦN 2.3 - TỐI ƯU HÓA TRUY VẤN (OFFLOAD XUỐNG SQL SERVER)

Nhiều Dev non tay sẽ gọi lệnh `ToListAsync()` để tải 1.000.000 Hóa đơn lên RAM, sau đó mới dùng C# `foreach` để cộng dồn tiền. Hậu quả là sập Server OutOfMemory!
Ở đây, ta dùng IQueryable để bắt SQL Server làm "Cửu Vạn".

```csharp
            // BƯỚC 2: TẠO CÂU LỆNH CHỜ (Chưa chạy xuống DB)
            var paidInvoicesQuery = _unitOfWork.Invoices.Query()
                .Where(i => i.PaymentStatus.ToLower() == "paid" 
                         && i.CreatedAt >= startUtc 
                         && i.CreatedAt <= endUtc);

            // BƯỚC 3: TÍNH TỔNG TIỀN
            // Lệnh SumAsync() sẽ biên dịch ra câu SQL: "SELECT SUM(TotalAmount) FROM Invoices WHERE..."
            // Server SQL chạy cực nhanh và chỉ trả về C# đúng 1 con số duy nhất!
            var totalRevenue  = await paidInvoicesQuery.SumAsync(i => i.TotalAmount);
            var invoicesCount = await paidInvoicesQuery.CountAsync();
```

---

### PHẦN 2.4 - GOM NHÓM (GROUP BY) BIỂU ĐỒ DOANH THU NGÀY

Làm sao để vẽ biểu đồ 30 ngày?

```csharp
            // BƯỚC 4: TÍNH DOANH THU THEO TỪNG NGÀY
            var dailyRevenue = await paidInvoicesQuery
                .GroupBy(i => i.CreatedAt.Date) // Nhóm tất cả hóa đơn cùng ngày lại với nhau
                .Select(g => new DailyRevenueDto
                {
                    Date   = g.Key, // g.Key chính là Ngày (Ví dụ: 15/10)
                    Amount = g.Sum(i => i.TotalAmount), // Tổng tiền ngày 15/10
                    Count  = g.Count() // Số lượng hóa đơn ngày 15/10
                })
                .OrderBy(d => d.Date)
                .ToListAsync(); // Lúc này C# chỉ tải đúng 30 dòng (30 ngày) lên RAM, siêu nhẹ!
```

---

### PHẦN 2.5 - TÍNH KPI BÁC SĨ (BẢNG XẾP HẠNG HIỆU SUẤT)

Ngoài tiền, Giám đốc cần biết Bác sĩ nào "cày" nhiều ca nhất để thưởng Tết. Ta không dựa vào Hóa đơn (vì Hóa đơn do Lễ tân thu tiền), mà phải lục lọi vào bảng Lịch Hẹn (`Appointments`).

```csharp
            // BƯỚC 5: CHẤM ĐIỂM BÁC SĨ THÚ Y
            var doctorPerformance = await _unitOfWork.Appointments.Query()
                .Where(a => a.Status.ToLower() == "completed" // Chỉ tính những ca khám Đã Xong (Bỏ qua Bị Hủy/Vắng)
                         && a.AppointmentDate >= startUtc
                         && a.AppointmentDate <= endUtc)
                .GroupBy(a => a.Doctor!.FullName) // Gom nhóm theo Tên Bác sĩ
                .Select(g => new DoctorPerformanceDto
                {
                    DoctorName            = g.Key ?? "Bác sĩ thú y",
                    CompletedAppointments = g.Count() // Đếm xem ông này khám xong mấy ca
                })
                .OrderByDescending(d => d.CompletedAppointments) // Sắp xếp từ Cao xuống Thấp
                .ToListAsync();
```

**Tổng kết:** Khi hàm này chạy xong, toàn bộ 4 biểu đồ đã được tính toán xong xuôi từ dưới Database Server và nén lại thành 1 gói DTO duy nhất gửi cho Frontend Vue3. Frontend chỉ việc bóc tách ra và nhét vào bộ thư viện Chart.js để vẽ hình. Hiệu năng đạt mức 100/100!

---
*(Hết tài liệu đào tạo chuyên sâu Admin: Báo cáo Kế toán)*
