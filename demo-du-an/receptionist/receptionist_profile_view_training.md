# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG XEM HỒ SƠ KHÁCH HÀNG & THÚ CƯNG (DASHBOARD) - BẢN FULL DEEP DIVE (COMBO GIẢI THÍCH KÉP)

> [!NOTE]
> Đây là tài liệu số 9, đi sâu vào chức năng **Xem chi tiết Hồ sơ Khách hàng và Thú cưng (Góc nhìn Lễ tân)**.
> Ở góc nhìn Khách hàng, họ chỉ thấy được lịch sử của chính mình. Nhưng ở góc nhìn Lễ tân, hồ sơ này biến thành một **Dashboard 360 độ**. Hệ thống không chỉ trả về tên tuổi, mà còn tự động tính toán tổng số tiền đã chi tiêu, số lần "bùng" lịch, và công nợ chưa trả thông qua sức mạnh tính toán của **LINQ Aggregation**.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Dashboard Hồ sơ Khách hàng & Thú cưng.
- **Mục đích:** Khi Lễ tân bấm vào xem một người khách, hệ thống hiển thị ngay lập tức một bức tranh toàn cảnh: Có mấy con thú cưng, lịch sử đặt khám từ trước tới nay ra sao, khách hàng này là khách VIP hay khách hay bùng lịch.
- **Điểm nổi bật (Kỹ thuật):** Giao tiếp liên Service (Inter-Service Communication) và tính toán các chỉ số kinh doanh (Business Metrics) trực tiếp trên bộ nhớ RAM bằng LINQ thay vì viết các câu Query SQL cồng kềnh.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG CONTROLLER (GIAO DIỆN XEM CHI TIẾT)

**Tệp:** `WebApi/Controllers/ReceptionistController.cs`

```csharp
        /// <summary>
        /// Xem toàn cảnh Dashboard của Khách hàng (Bao gồm thông tin, thú cưng, lịch sử khám).
        /// </summary>
        [HttpGet("customers/{id}")] // (Ví dụ URL: /api/receptionist/customers/a1b2c3d4)
        public async Task<IActionResult> CustomerDetail(Guid id)
        {
            // (Nhờ Service gom tất cả các loại dữ liệu lại thành 1 cục ViewModel duy nhất)
            var viewModel = await _receptionistService.GetCustomerDashboardDetailAsync(id);
            
            if (viewModel == null) return NotFound(); // (Nếu không tìm thấy Khách hàng thì báo lỗi 404)

            return Ok(viewModel); // (Trả toàn bộ dữ liệu Dashboard về cho UI hiển thị)
        }

        /// <summary>
        /// Xem chi tiết riêng lẻ của 1 con Thú cưng.
        /// </summary>
        [HttpGet("pets/{id}")]
        public async Task<IActionResult> PetDetail(long id)
        {
            var viewModel = await _receptionistService.GetPetDashboardDetailAsync(id);
            if (viewModel == null) return NotFound();

            return Ok(viewModel);
        }
```

**Giải thích chi tiết:**
- Code Controller rất mỏng, nó đóng vai trò định tuyến. Thuật ngữ `viewModel` (hoặc DTO) ở đây ám chỉ một khối dữ liệu khổng lồ chứa nhiều thành phần nhỏ bên trong. 

---

### PHẦN 2.2 - TẦNG SERVICE (THUẬT TOÁN GOM DỮ LIỆU & TÍNH TOÁN DASHBOARD)

**Tệp:** `MyPetClinic.Application/Services/ReceptionistService.cs`

Đây là đoạn code cho thấy sức mạnh của việc thiết kế Clean Architecture đúng chuẩn. `ReceptionistService` đóng vai trò như một Người nhạc trưởng (Orchestrator), nó gọi các Service khác (CustomerService, AppointmentService) để lấy các mảnh ghép, sau đó ghép lại thành 1 bức tranh hoàn chỉnh.

```csharp
        public async Task<CustomerDashboardDetailDto?> GetCustomerDashboardDetailAsync(System.Guid id)
        {
            // BƯỚC 1: THU THẬP CÁC MẢNH GHÉP DỮ LIỆU (Inter-Service Communication)
            
            // (Mảnh 1: Lấy thông tin cá nhân của khách hàng)
            var customer = await _customerService.GetCustomerDetailAsync(id);
            if (customer == null) return null; // (Nếu khách đã bị xóa mềm hoặc không tồn tại thì dừng luôn)

            // (Mảnh 2: Lấy danh sách thú cưng của ông khách này)
            var pets = await _customerService.GetPetsByCustomerAsync(id);
            
            // (Mảnh 3: Lấy danh sách Bác sĩ và Dịch vụ đang hoạt động để Lễ tân có thể thao tác Đặt lịch ngay tại màn hình này)
            var doctors = await GetActiveDoctorsAsync();
            var services = await _appointmentService.GetServicesAsync();
            
            // (Mảnh 4: Lấy TOÀN BỘ lịch sử đặt khám của khách hàng từ thuở sơ khai)
            var appointments = await _appointmentService.GetCustomerAppointmentsAsync(id);

            // BƯỚC 2: RÁP MẢNH GHÉP VÀ TÍNH TOÁN CHỈ SỐ KINH DOANH BẰNG LINQ
            return new CustomerDashboardDetailDto
            {
                Customer = customer,
                Pets = pets,
                ActiveDoctors = doctors,
                Services = services,
                Appointments = appointments,
                
                // --- BẮT ĐẦU TÍNH TOÁN (AGGREGATION) ---
                
                // (Chỉ số 1: Số lần đã đến phòng khám. Chỉ đếm những ca đã Hoàn thành hoặc Đang chờ thanh toán)
                TotalVisits = appointments.Count(a => a.Status == "completed" || a.Status == "ready_to_pay"),
                
                // (Chỉ số 2: Xếp hạng độ VIP. Tính Tổng số tiền đã thanh toán (paid). Lệnh .Sum() cộng dồn toàn bộ hóa đơn)
                TotalSpent = appointments.Where(a => a.InvoiceStatus == "paid").Sum(a => a.InvoiceTotalAmount ?? 0),
                
                // (Chỉ số 3: Tỷ lệ Bùng lịch. Đếm số lần Khách đặt lịch rồi không đến (cancelled))
                NoShowCount = appointments.Count(a => a.Status == "cancelled"),
                
                // (Chỉ số 4: Công nợ. Tính Tổng tiền của các hóa đơn chưa thanh toán (unpaid))
                UnpaidBalance = appointments.Where(a => a.InvoiceStatus == "unpaid").Sum(a => a.InvoiceTotalAmount ?? 0)
            };
        }
```

**Giải thích chi tiết (Tinh hoa Tính toán):**
- **Tại sao không tính toán bằng SQL?** Nếu ta dùng `SELECT COUNT...` hay `SELECT SUM...` dưới Database, ta sẽ phải bắn 4 câu truy vấn nặng nề xuống Server. Ở đây, ta đã lấy biến `appointments` (Mảnh 4) lên RAM. Việc dùng LINQ `.Count()` và `.Sum()` thao tác trực tiếp trên biến `appointments` trong RAM mất chưa tới `0.001` giây. Tốc độ bàn thờ!
- **Tính ứng dụng của Dashboard:** 
  - Thấy `NoShowCount > 3`: Lễ tân cảnh giác, biết ngay đây là khách chuyên môn bùng lịch, có thể yêu cầu cọc tiền trước.
  - Thấy `UnpaidBalance > 0`: Khách vừa đến cửa, Lễ tân nhắc khéo: "Dạ đợt trước anh còn hóa đơn 200k chưa thanh toán ạ".
  - Thấy `TotalSpent > 10,000,000`: Biết ngay đây là khách VIP, thái độ chăm sóc đặc biệt hơn.

---

### PHẦN 2.3 - TẦNG SERVICE (DASHBOARD CỦA THÚ CƯNG)

Tương tự như trên, nhưng thu hẹp phạm vi lại thành 1 con thú cưng.

```csharp
        public async Task<PetDashboardDetailDto?> GetPetDashboardDetailAsync(long id)
        {
            // (1. Tìm thú cưng và dùng Include(p => p.Customer) để lấy luôn thông tin người Chủ sở hữu đính kèm)
            var pet = await _unitOfWork.Pets.GetFirstOrDefaultWithIncludesAsync(p => p.Id == id, p => p.Customer!);
            if (pet == null) return null;

            // (2. Lấy toàn bộ lịch sử khám của riêng con thú cưng này)
            var appointments = await _appointmentService.GetPetAppointmentsAsync(id);

            // (3. Đóng gói vào DTO và vứt hết các cột dư thừa)
            return new PetDashboardDetailDto
            {
                Pet = new PetDto 
                { 
                    Id = pet.Id, 
                    CustomerId = pet.CustomerId, 
                    Name = pet.Name, 
                    Species = pet.Species, 
                    // ... (Đổ dữ liệu)
                },
                Customer = new UserProfileDto 
                { 
                    Id = pet.Customer?.Id ?? Guid.Empty, 
                    FullName = pet.Customer?.FullName ?? string.Empty, 
                    Phone = pet.Customer?.Phone ?? string.Empty, 
                    RoleName = "Customer" 
                    // ... (Đổ dữ liệu người chủ)
                },
                Appointments = appointments // (Gắn kèm lịch sử khám)
            };
        }
```

**Giải thích chi tiết:**
- Chức năng này hữu ích khi Lễ tân không nhớ Khách hàng, nhưng nhớ bé chó "Cậu Vàng". Bấm vào xem Cậu Vàng, hệ thống lập tức map ngược ra xem Chủ của Cậu Vàng là ai thông qua khóa ngoại (`Include(p => p.Customer!)`). 

---

### PHẦN 2.4 - XEM HỒ SƠ Y TẾ THÚ CƯNG (GÓC NHÌN LỄ TÂN)

Ngoài việc xem các thông tin thống kê trên Dashboard, Lễ tân cũng có quyền xem Lịch sử Y tế (Medical History) của thú cưng để giải đáp thắc mắc cho Khách hàng khi cần thiết (Ví dụ: Khách hỏi đợt trước bác sĩ kê thuốc gì).

Thay vì phải tự viết lại một API mới, Lễ tân được cấp quyền sử dụng chung API với Bác sĩ thông qua `MedicalRecordsController`.

**Tệp:** `WebApi/Controllers/MedicalRecordsController.cs`

```csharp
    // (BẢO MẬT: Thêm role 'receptionist' vào danh sách được phép truy cập)
    [Authorize(Roles = "doctor,admin,receptionist,Doctor,Admin,Receptionist,SystemAdmin,clinical_doctor,vaccination_doctor")]
    [ApiController]
    [Route("api/medical-records")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        // ...

        /// <summary>
        /// Lễ tân và Bác sĩ dùng chung hàm này để xem toàn bộ lịch sử bệnh án của 1 thú cưng.
        /// </summary>
        [HttpGet("pet/{petId}")] // (URL: /api/medical-records/pet/5)
        public async Task<IActionResult> GetPetMedicalHistory(long petId)
        {
            try
            {
                // (Gọi xuống Service dùng chung để lấy dữ liệu)
                var history = await _medicalRecordService.GetPetMedicalHistoryAsync(petId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }
    }
```

**Giải thích chi tiết:**
- **Tái sử dụng Code (Code Reusability):** Hàm `GetPetMedicalHistoryAsync(petId)` chính là hàm siêu phức tạp dùng **Dictionary Grouping O(1)** để gom Bệnh án + Lịch hẹn + Đơn thuốc lại với nhau (đã được giải thích ở tài liệu Bệnh án SOAP). 
- Bằng cách cấp thêm role `receptionist` vào đầu Controller, Lễ tân có thể tái sử dụng nguyên vẹn luồng dữ liệu này mà Backend không cần phải viết thêm một dòng code nào mới trong `ReceptionistController`. Đây là lợi ích của việc chia nhỏ Service theo kiến trúc Clean Architecture.

---
*(Hết tài liệu đào tạo chuyên sâu Lễ tân: Dashboard Hồ sơ Khách hàng & Thú cưng)*
