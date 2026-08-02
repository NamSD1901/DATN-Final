# TÀI LIỆU ĐÀO TẠO NỘI BỘ: PHÂN TÍCH CHUYÊN SÂU MEDICAL RECORD SERVICE (DOCTOR)

> [!NOTE]
> Đây là tài liệu đào tạo chuyên biệt phân tích từng dòng code của `MedicalRecordService.cs`. Nơi tập trung toàn bộ "chất xám" về nghiệp vụ lập Bệnh án SOAP, chống tranh chấp dữ liệu (Transaction), thuật toán trừ Tồn kho theo hạn sử dụng (FEFO) và tự động sinh Lịch tái khám.

---

## 1. TẠO BỆNH ÁN SOAP & THUẬT TOÁN TRỪ KHO FEFO

Hàm `CreateSoapMedicalRecordAsync` là trái tim của phân hệ Khám Lâm Sàng.

```c#
        public async Task<long> CreateSoapMedicalRecordAsync(MedicalRecordSoapRequestDto dto, Guid doctorId)
        {
            // 1. TRANSACTION - CHỐNG TRANH CHẤP
            // IsolationLevel.RepeatableRead khóa các bảng liên quan, ngăn chặn 2 bác sĩ cùng kê đơn 1 lọ thuốc cuối cùng trong kho.
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(a => a.Id == dto.AppointmentId, a => a.Pet!);
                var appointment = appointments.FirstOrDefault();
                if (appointment == null) throw new KeyNotFoundException("Không tìm thấy cuộc hẹn.");

                // Cấu hình JsonSerializer để lưu đối tượng SOAP (Nested Object) thành chuỗi JSON với key dạng camelCase
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
                
                // 2. KHỞI TẠO BỆNH ÁN
                var medicalRecord = new MedicalRecord
                {
                    AppointmentId = dto.AppointmentId,
                    DoctorId = doctorId,
                    PetId = dto.PetId == 0 ? appointment.PetId : dto.PetId,
                    RecordType = "Consultation",
                    // Nén từng phần của SOAP thành chuỗi JSON nguyên bản
                    MedicalHistory = JsonSerializer.Serialize(dto.Subjective, options),
                    Weight = dto.Objective.Weight,
                    Temperature = dto.Objective.Temperature ?? 0,
                    ClinicalSigns = JsonSerializer.Serialize(dto.Objective, options),
                    Diagnosis = JsonSerializer.Serialize(dto.Assessment, options),
                    TreatmentPlan = JsonSerializer.Serialize(dto.Plan.TreatmentDirections, options),
                    DoctorNotes = dto.Plan.CareInstructions,
                    FollowUpDate = dto.Plan.FollowUpDate,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);
                
                // (Optional) Cập nhật lại cân nặng mới nhất cho con vật
                if (appointment.Pet != null && dto.Objective.Weight > 0)
                {
                    appointment.Pet.Weight = dto.Objective.Weight;
                    _unitOfWork.Pets.Update(appointment.Pet);
                }

                await _unitOfWork.SaveChangesAsync(); // Lưu tạm để SQL sinh ra ID cho medicalRecord

                // 3. XỬ LÝ ĐƠN THUỐC & TRỪ KHO (Nếu Bác sĩ có kê thuốc)
                if (dto.Plan.Prescriptions != null && dto.Plan.Prescriptions.Any())
                {
                    var prescription = new Prescription
                    {
                        MedicalRecordId = medicalRecord.Id,
                        DoctorId = doctorId,
                        Note = dto.Plan.CareInstructions,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Prescriptions.AddAsync(prescription);
                    await _unitOfWork.SaveChangesAsync(); // Lưu để sinh ID đơn thuốc

                    // 3A. BƯỚC XÁC THỰC (VALIDATION): Kiểm tra Tồn kho trước khi trừ
                    foreach (var item in dto.Plan.Prescriptions)
                    {
                        var medicine = await _medicineService.GetMedicineStockAsync(item.MedicineId);
                        if (medicine == null) throw new KeyNotFoundException($"Không tìm thấy thuốc với ID {item.MedicineId}");
                        // Nếu số lượng tổng trong kho ít hơn số bác sĩ kê -> Quăng lỗi ngay lập tức
                        if (medicine.StockQuantity < item.Quantity) throw new InvalidOperationException($"Thuốc '{medicine.Name}' không đủ tồn kho.");
                    }

                    // 3B. THỰC THI TRỪ KHO (FEFO) VÀ LƯU CHI TIẾT
                    foreach (var item in dto.Plan.Prescriptions)
                    {
                        // GỌI CHÉO SERVICE: Nhờ MedicineService áp dụng luật FEFO (First-Expired-First-Out).
                        // Hàm này tự động quét các lô thuốc cận date nhất để trừ đi.
                        await _medicineService.ExportMedicineAsync(new ExportMedicineDto
                        {
                            MedicineId = item.MedicineId,
                            Quantity = item.Quantity,
                            ReferenceCode = $"MR-{medicalRecord.Id}", // Lưu vết tham chiếu
                            Notes = $"Kê đơn SOAP #{medicalRecord.Id}"
                        }, doctorId);

                        // Lưu vào chi tiết toa thuốc
                        await _unitOfWork.PrescriptionItems.AddAsync(new PrescriptionItem
                        {
                            PrescriptionId = prescription.Id,
                            MedicineId = item.MedicineId,
                            Dosage = item.Dosage,
                            Frequency = item.Frequency,
                            DurationDays = item.DurationDays,
                            Quantity = item.Quantity,
                            Instruction = item.Instruction
                        });
                    }
                }

                // 4. CẬP NHẬT TRẠNG THÁI CA KHÁM
                appointment.Status = "ready_to_pay"; // Đẩy thẻ Kanban sang cột "Chờ thanh toán"
                _unitOfWork.Appointments.Update(appointment);

                // (PHẦN 5 TỰ ĐỘNG SINH LỊCH HẸN TRÌNH BÀY BÊN DƯỚI)
                // ...
                
                // CHỐT GIAO DỊCH
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync(); // Thành công, mở khóa Database

                return medicalRecord.Id;
            }
            catch (Exception)
            {
                // Nếu 1 bước lỗi (vd: hết thuốc giữa chừng), hoàn tác TẤT CẢ mọi thay đổi (Rollback)
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
```

---

## 2. LOGIC TỰ ĐỘNG SINH LỊCH TÁI KHÁM & CHỐNG TRÙNG LỊCH

Nằm ở phần 5 của hàm `CreateSoapMedicalRecordAsync`. Khi Bác sĩ chọn "Cần Tái Khám" (FollowUp), hệ thống tự động sinh ra một ca khám mới trong tương lai và kiểm tra xem có bị trùng giờ không.

```c#
                // 5. Tự động sinh lịch hẹn tái khám nếu có yêu cầu (SOAP)
                if (dto.Plan.CreateFollowUpAppointment && dto.Plan.FollowUpDate.HasValue)
                {
                    // 5A. Sinh mã QR Độc nhất vô nhị bằng vòng lặp
                    string qrToken = string.Empty;
                    bool isQrUnique = false;
                    for (int q = 0; q < 5 && !isQrUnique; q++)
                    {
                        qrToken = "QR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                        isQrUnique = !_unitOfWork.Appointments.Query().Any(a => a.QrToken == qrToken);
                    }
                    if (!isQrUnique) throw new InvalidOperationException("Không thể tạo mã QR duy nhất cho lịch hẹn tái khám.");

                    // 5B. Tạo thực thể Cuộc hẹn tái khám mồ côi (Pending)
                    var followUpApt = new Appointment
                    {
                        CustomerId = appointment.CustomerId,
                        PetId = medicalRecord.PetId,
                        ServiceId = dto.Plan.FollowUpServiceId ?? appointment.ServiceId,
                        DoctorId = dto.Plan.FollowUpDoctorId ?? doctorId,
                        Symptom = "Tái khám / Tái tiêm theo chỉ định",
                        Note = dto.Plan.FollowUpNote,
                        Status = "pending",
                        // ...
                        IsSystemGenerated = true, // Đóng dấu là Máy tự sinh ra
                        ReferenceRecordId = medicalRecord.Id
                    };

                    // 5C. VALIDATION: CHUỖI KIỂM TRA CHỐNG TRÙNG LỊCH SIÊU KHẮT KHE
                    var targetDateStart = followUpApt.AppointmentDate.Date;
                    var appointmentTimeCheck = followUpApt.StartTime;

                    // KIỂM TRA 1: Có rơi vào ngày nghỉ lễ chung (Tết, Lễ) không?
                    var isHoliday = _unitOfWork.ClinicHolidays.Query().Any(h => h.IsActive && h.StartDate <= targetDateStart && h.EndDate >= targetDateStart);
                    if (isHoliday) throw new InvalidOperationException("Phòng khám đóng cửa vào ngày nghỉ lễ này.");

                    // KIỂM TRA 2: Có rơi vào ngày phòng khám đóng cửa định kỳ (vd: Chủ nhật) không?
                    var clinicDay = _unitOfWork.ClinicOperatingDays.Query().FirstOrDefault(d => d.DayOfWeek == targetDateStart.DayOfWeek);
                    if (clinicDay != null && !clinicDay.IsOpen) throw new InvalidOperationException($"Phòng khám không hoạt động vào {targetDateStart.DayOfWeek}.");

                    // KIỂM TRA 3: Bác sĩ đó có Lịch Trực (Doctor Schedule) vào ngày đó không?
                    var docSchedule = _unitOfWork.DoctorSchedules.Query()
                        .Where(s => s.DoctorId == followUpApt.DoctorId && s.WorkDate == targetDateUtc && s.IsAvailable)
                        .ToList();
                    if (!docSchedule.Any()) throw new InvalidOperationException("Bác sĩ không có lịch trực vào ngày này.");

                    // KIỂM TRA 4: Bác sĩ có bị trùng với một bệnh nhân khác (khoảng cách < 30 phút) không?
                    var doctorApts = _unitOfWork.Appointments.Query()
                        .Where(a => a.DoctorId == followUpApt.DoctorId && a.Status != "cancelled" && a.AppointmentDate == targetDateUtc)
                        .Select(a => a.StartTime)
                        .ToList();
                    var isDoctorDoubleBooked = doctorApts.Any(startTime => Math.Abs((startTime - appointmentTimeCheck).TotalMinutes) < 30);
                    if (isDoctorDoubleBooked) throw new InvalidOperationException("Khung giờ tái khám này đã có khách hàng khác đặt.");

                    // Vượt qua 4 vòng kiểm tra, chốt lưu cuộc hẹn tái khám vào DB
                    await _unitOfWork.Appointments.AddAsync(followUpApt);
                }
```

---

## 3. LOGIC BIÊN DỊCH JSON (PARSER)

Dữ liệu SOAP lưu trong DB là chuỗi JSON. Hàm `ExtractReadableSoap` có nhiệm vụ biến những khối JSON vô hồn đó thành văn bản tiếng Việt dễ đọc cho màn hình lịch sử y tế.

```c#
        public string ExtractReadableSoap(string? jsonStr, string fieldType)
        {
            if (string.IsNullOrWhiteSpace(jsonStr)) return string.Empty;
            // Nếu không phải chuỗi JSON (bắt đầu bằng { hoặc [) thì trả lại nguyên gốc
            if (!jsonStr.TrimStart().StartsWith("{") && !jsonStr.TrimStart().StartsWith("[")) return jsonStr; 

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            try
            {
                // XỬ LÝ KHỐI O - OBJECTIVE (KHÁM LÂM SÀNG)
                if (fieldType == "O")
                {
                    // Giải mã JSON thành đối tượng ObjectiveDto của C#
                    var obj = JsonSerializer.Deserialize<ObjectiveDto>(jsonStr, options);
                    if (obj == null) return jsonStr;
                    
                    var parts = new List<string>(); // Mảng hứng câu văn
                    
                    if (obj.HeartRate.HasValue) parts.Add($"Nhịp tim: {obj.HeartRate} bpm");
                    if (obj.RespiratoryRate.HasValue) parts.Add($"Nhịp thở: {obj.RespiratoryRate} lần/phút");

                    // Gom các cơ quan có triệu chứng bất thường
                    var abnormal = new List<string>();
                    // Nếu mắt bất thường (IsNormal == false), lôi ghi chú Note ra
                    if (obj.Eyes != null && !obj.Eyes.IsNormal) abnormal.Add("Mắt" + (!string.IsNullOrEmpty(obj.Eyes.Note) ? $": {obj.Eyes.Note}" : ""));
                    if (obj.SkinCoat != null && !obj.SkinCoat.IsNormal) abnormal.Add("Da lông" + (!string.IsNullOrEmpty(obj.SkinCoat.Note) ? $": {obj.SkinCoat.Note}" : ""));
                    
                    if (abnormal.Any()) parts.Add("Bất thường: " + string.Join(", ", abnormal));

                    return parts.Any() ? string.Join(", ", parts) : string.Empty;
                }
                // ... (Logic tương tự cho S, A, P)
            }
            catch
            {
                // Fallback cực quan trọng: Lỡ JSON bị hỏng, vẫn trả về chuỗi gốc thay vì làm sập (Crash) toàn bộ Web
                return jsonStr; 
            }
        }
```
*(Hết tài liệu phân tích Medical Record Service)*
