# TÀI LIỆU ĐÀO TẠO NỘI BỘ: XEM CHI TIẾT CA KHÁM VÀ LỊCH SỬ BỆNH ÁN (DOCTOR) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo bổ sung nhằm phân tích các hàm chưa được đề cập trong các tài liệu trước của module Doctor. Bác sĩ không chỉ cần hàng đợi Kanban và ghi bệnh án SOAP, mà còn cần chức năng **Xem chi tiết ca khám** và **Truy xuất lịch sử y tế (Medical History)** của thú cưng để có phác đồ điều trị chính xác nhất.
> Trọng tâm kỹ thuật ở đây là cách Backend tối ưu hóa LINQ để join nhiều bảng (Appointment, Pet, Customer, Service, Invoice) và kỹ thuật gom nhóm dữ liệu (Flattening) thành các DTO.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Chi tiết ca khám và Lịch sử y tế thú cưng.
- **Mục đích:** Cung cấp API cho bác sĩ nhấp vào một thẻ trên Kanban để xem toàn bộ thông tin chi tiết của ca khám, đồng thời tải lên lịch sử bệnh án cũ của thú cưng (bao gồm cả tiêm phòng và khám lâm sàng).
- **Điểm nổi bật (Kỹ thuật):** Tối ưu hóa truy vấn cơ sở dữ liệu với `Select` để tránh tải thừa dữ liệu, ghép nối hai luồng dữ liệu (Khám bệnh và Tiêm phòng) vào chung một danh sách kết quả, và cách tạo DTO động.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG SERVICE: LẤY CHI TIẾT CA KHÁM

**Tệp:** `MyPetClinic.Application/Services/DoctorAppointmentService.cs`

Khi bác sĩ bấm vào 1 thẻ trên màn hình Kanban, hệ thống phải gọi hàm `GetAppointmentDetailAsync` để lấy chi tiết 100% về khách hàng, thú cưng, dịch vụ, hóa đơn, và cả vaccine.

```c#
        public async Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id)
        {
            // (1. TỐI ƯU HÓA TRUY VẤN: Sử dụng Select() để chỉ lấy ra đúng những cột cần thiết, tránh SELECT * làm nặng RAM)
            // Lệnh IgnoreQueryFilters() để bỏ qua các bộ lọc mặc định (như Soft Delete) nếu cần lấy cả dữ liệu đã ẩn
            var a = _unitOfWork.Appointments.Query().IgnoreQueryFilters()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.PetId,
                    PetName = x.Pet != null ? x.Pet.Name : null,
                    Species = x.Pet != null ? x.Pet.Species : null,
                    Breed = x.Pet != null ? x.Pet.Breed : null,
                    Weight = x.Pet != null ? x.Pet.Weight : null,
                    IsAggressive = x.Pet != null ? (bool?)x.Pet.IsAggressive : null,
                    
                    x.CustomerId,
                    CustomerName = x.Customer != null ? x.Customer.FullName : null,
                    CustomerPhone = x.Customer != null ? x.Customer.Phone : null,
                    
                    x.ServiceId,
                    ServiceName = x.Service != null ? x.Service.Name : null,
                    ServicePrice = x.Service != null ? (decimal?)x.Service.Price : null,
                    
                    x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    
                    x.AppointmentDate,
                    x.StartTime,
                    x.Symptom,
                    x.Note,
                    x.Status,
                    x.QrToken,
                    x.Type,
                    x.IsSystemGenerated,
                    x.ReferenceRecordId,
                    
                    x.VaccineId,
                    VaccineName = x.Vaccine != null ? x.Vaccine.Name : null
                })
                .FirstOrDefault(); // Lấy 1 dòng duy nhất

            if (a == null) return null;

            // (2. MAPPING DỮ LIỆU: Đổ từ kiểu vô danh (Anonymous Type) sang DTO chính thức)
            var mapped = new AppointmentDetailDto
            {
                Id = a.Id,
                PetId = a.PetId,
                PetName = a.PetName,
                Species = a.Species,
                Breed = a.Breed,
                Weight = a.Weight,
                IsAggressive = a.IsAggressive ?? false, // Xử lý null
                CustomerId = a.CustomerId,
                CustomerName = a.CustomerName,
                CustomerPhone = a.CustomerPhone,
                ServiceId = a.ServiceId,
                ServiceName = a.ServiceName,
                ServicePrice = a.ServicePrice,
                DoctorId = a.DoctorId,
                DoctorName = a.DoctorName,
                // (3. XỬ LÝ THỜI GIAN: Cộng Date và Time lại, sau đó chuyển sang chuẩn ISO)
                AppointmentDate = a.AppointmentDate.ToLocalTime().Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                Symptom = a.Symptom,
                Note = a.Note,
                Status = a.Status,
                QrToken = a.QrToken,
                Type = a.Type,
                IsSystemGenerated = a.IsSystemGenerated,
                ReferenceRecordId = a.ReferenceRecordId,
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            };

            return await Task.FromResult(mapped);
        }
```

**Giải thích chi tiết:**
- Thay vì dùng `Include(a => a.Pet).Include(a => a.Customer)...` rồi tải toàn bộ hàng chục cột của các bảng đó lên RAM, ở đây tác giả sử dụng **LINQ Projection (`Select`)** để chỉ bóc ra tên, số điện thoại, giá tiền. SQL Server sẽ dịch nó ra thành câu `SELECT Pet.Name, Customer.FullName...` siêu nhẹ và nhanh.

---

### PHẦN 2.2 - TẦNG SERVICE: TỔNG HỢP LỊCH SỬ KHÁM BỆNH & TIÊM PHÒNG (MEDICAL HISTORY)

**Tệp:** `MyPetClinic.Application/Services/DoctorAppointmentService.cs`

Để bác sĩ biết chó/mèo này tháng trước bị bệnh gì, hệ thống phải gộp (Concat) hai loại hồ sơ: **Bệnh án Lâm sàng** và **Hồ sơ Tiêm phòng**.

```c#
        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid CustomerId)
        {
            // 1. KIỂM TRA QUYỀN TRUY CẬP BẢO MẬT (IDOR PREVENTION)
            var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.Id == petId && p.CustomerId == CustomerId);
            if (pet == null)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin bệnh án của thú cưng này.");
            }

            // 2. TRUY VẤN 1: LẤY BỆNH ÁN LÂM SÀNG
            var records = _unitOfWork.MedicalRecords.Query().IgnoreQueryFilters()
                .Where(mr => mr.Appointment != null && mr.Appointment.PetId == petId)
                .OrderByDescending(mr => mr.CreatedAt)
                .Select(mr => new
                {
                    mr.Id,
                    mr.AppointmentId,
                    // ... các trường của MedicalRecord
                    InvoiceId = mr.Appointment != null && mr.Appointment.Invoice != null ? (long?)mr.Appointment.Invoice.Id : null,
                    // (Lồng danh sách Thuốc đã kê vào bên trong Bệnh án)
                    PrescribedMedicines = mr.Prescriptions
                        .SelectMany(p => p.PrescriptionItems)
                        .Where(pi => pi.Medicine != null)
                        .Select(pi => new PrescribedMedicineDto
                        {
                            MedicineName = pi.Medicine!.Name,
                            Dosage = pi.Dosage,
                            Frequency = pi.Frequency,
                            // ...
                        })
                        .ToList()
                })
                .ToList();

            // Thực hiện Map sang DTO chuẩn
            var mapped = records.Select(mr => new MedicalRecordDto { ... }).ToList();

            // 3. TRUY VẤN 2: LẤY LỊCH SỬ TIÊM PHÒNG (Vì tiêm phòng nằm ở bảng khác)
            var vacRecords = _unitOfWork.VaccinationRecords.Query().IgnoreQueryFilters()
                .Where(v => v.PetId == petId)
                .Select(v => new { ... })
                .ToList();

            var vacMapped = vacRecords.Select(v => new MedicalRecordDto
            {
                // (MẸO NHỎ: Cố tình cộng 1,000,000 vào ID của hồ sơ tiêm phòng để tránh bị trùng ID với hồ sơ khám bệnh trên UI Frontend)
                RecordId = v.Id + 1000000, 
                AppointmentId = v.AppointmentId ?? 0,
                // ...
                RecordType = "Vaccination", // (Đóng dấu cờ để UI biết đây là Tiêm phòng)
                TreatmentPlan = v.Dose != null ? $"Tiêm {v.Dose} ml {v.VaccineName}" : $"Tiêm {v.VaccineName}",
                PrescribedMedicines = new List<PrescribedMedicineDto>() // (Tiêm phòng thì không kê đơn thuốc mang về)
            }).ToList();

            // 4. GHÉP CHUỖI VÀ SẮP XẾP CHUNG LẠI BẰNG CONCAT
            var combinedList = mapped.Concat(vacMapped).OrderByDescending(r => r.VisitDate).ToList();

            return await Task.FromResult(combinedList);
        }
```

**Giải thích chi tiết:**
- Vấn đề hóc búa nhất là **Sổ tiêm** và **Sổ khám** nằm ở 2 bảng khác nhau, nhưng UI lại muốn hiện thị chung trên 1 dòng Timeline thời gian. Cách giải quyết là ép hai bảng đó về cùng chung một chuẩn `MedicalRecordDto`, đặt biến `RecordType` để phân biệt.
- Thủ thuật cộng ID `v.Id + 1000000` là một cách giải quyết nhanh (Hack) để ID không bị trùng lặp khi binding trên Frontend (Frontend thường dùng khóa chính ID làm key trong vòng lặp `v-for`), tuy nhiên nó có thể tiềm ẩn rủi ro nếu hệ thống vượt quá 1 triệu bệnh án.

---

### PHẦN 2.3 - TẦNG SERVICE: HÀM MAP DTO HÀNG LOẠT DỰA TRÊN TỪ ĐIỂN (DICTIONARY)

**Tệp:** `MyPetClinic.Application/Services/DoctorAppointmentService.cs`

Khi có một danh sách hàng chục cuộc hẹn (`GetPetAppointmentsAsync`), việc ánh xạ DTO sẽ dễ gây ra lỗi **N+1 Query** nếu không cẩn thận.

```c#
        private async Task<List<AppointmentDetailDto>> MapToDetailDtoAsync(IEnumerable<Appointment> appointments)
        {
            if (!appointments.Any()) return new List<AppointmentDetailDto>();

            // 1. GOM NHÓM TẤT CẢ ID LẠI (TRÁNH LỖI N+1 QUERIES)
            var petIds = appointments.Select(a => a.PetId).Distinct().ToList();
            // CHỈ 1 CÂU TRUY VẤN LẤY TOÀN BỘ PET TRONG DANH SÁCH (Sử dụng ToDictionaryAsync để tra cứu O(1))
            var pets = await _unitOfWork.Pets.Query().IgnoreQueryFilters().Where(p => petIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            var customerIds = appointments.Select(a => a.CustomerId).Distinct().ToList();
            var customers = await _unitOfWork.Customers.Query().IgnoreQueryFilters().Where(c => customerIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);

            // (Làm tương tự cho Doctor, Service, Vaccine, Invoices...)
            // ...

            // 2. LẮP RÁP DỮ LIỆU TẠI RAM (SIÊU TỐC ĐỘ)
            var result = new List<AppointmentDetailDto>();
            foreach (var a in appointments)
            {
                // Thay vì lấy dữ liệu từ DB, ta tra cứu O(1) từ bộ nhớ RAM (Dictionary)
                pets.TryGetValue(a.PetId, out var pet);
                customers.TryGetValue(a.CustomerId, out var customer);
                // ...

                result.Add(new AppointmentDetailDto
                {
                    Id = a.Id,
                    PetName = pet?.Name ?? "Thú cưng đã xóa",
                    CustomerName = customer?.FullName ?? "Khách hàng",
                    // ...
                });
            }

            return result;
        }
```

**Giải thích chi tiết:**
- Đây là **Kỹ thuật chống N+1 Query kinh điển**. Nếu không gom list ID lại và dùng `Contains`, vòng lặp `foreach` sẽ bắn 5 câu truy vấn cho MỖI dòng. Nếu có 100 cuộc hẹn, sẽ tạo ra 500 câu truy vấn làm chết SQL Server. Bằng cách dùng `ToDictionaryAsync`, toàn bộ quy trình chỉ tốn đúng 6 câu truy vấn tổng cộng, bất kể dữ liệu lớn đến đâu. Việc tra cứu trên từ điển (Dictionary) tại RAM tốn chi phí thời gian gần như bằng 0 `O(1)`.

---
*(Hết tài liệu bổ sung đào tạo chuyên sâu Bác sĩ: Chi tiết Ca Khám & Lịch sử Y tế)*
