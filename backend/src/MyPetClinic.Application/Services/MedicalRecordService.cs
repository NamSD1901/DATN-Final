using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Text.Json;

namespace MyPetClinic.Application.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMedicineService _medicineService;

        public MedicalRecordService(IUnitOfWork unitOfWork, IMedicineService medicineService)
        {
            _unitOfWork = unitOfWork;
            _medicineService = medicineService;
        }

        public async Task<long> CreateMedicalRecordAsync(CreateMedicalRecordDto dto, Guid doctorId)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                // 1. Lấy thông tin cuộc hẹn
                var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(a => a.Id == dto.AppointmentId, a => a.Pet!);
                var appointment = appointments.FirstOrDefault();
                if (appointment == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy cuộc hẹn.");
                }

                // 2. Tạo MedicalRecord chuẩn SOAP
                var medicalRecord = new MedicalRecord
                {
                    AppointmentId = dto.AppointmentId,
                    DoctorId = doctorId,
                    PetId = dto.PetId == 0 ? appointment.PetId : dto.PetId, // Fallback to appointment's pet
                    RecordType = string.IsNullOrWhiteSpace(dto.RecordType) ? "Consultation" : dto.RecordType,
                    MedicalHistory = dto.MedicalHistory,
                    Weight = dto.Weight,
                    Temperature = dto.Temperature,
                    ClinicalSigns = dto.ClinicalSigns,
                    Diagnosis = dto.Diagnosis,
                    TreatmentPlan = dto.TreatmentPlan,
                    DoctorNotes = dto.DoctorNotes,
                    FollowUpDate = dto.FollowUpDate,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);
                await _unitOfWork.SaveChangesAsync(); // Lưu để có ID của MedicalRecord

                // 3. Nếu có đơn thuốc, tạo Prescription
                if (dto.Prescriptions != null && dto.Prescriptions.Any())
                {
                    var prescription = new Prescription
                    {
                        MedicalRecordId = medicalRecord.Id,
                        DoctorId = doctorId,
                        Note = dto.DoctorNotes,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Prescriptions.AddAsync(prescription);
                    await _unitOfWork.SaveChangesAsync(); // Lưu để có ID của Prescription

                    // Validate all stock first
                    foreach (var item in dto.Prescriptions)
                    {
                        var medicine = await _unitOfWork.Medicines.GetByIdAsync(item.MedicineId);
                        if (medicine == null)
                        {
                            throw new KeyNotFoundException($"Không tìm thấy thuốc với ID {item.MedicineId}");
                        }

                        if (medicine.StockQuantity < item.Quantity)
                        {
                            throw new InvalidOperationException($"Thuốc '{medicine.Name}' không đủ tồn kho. Yêu cầu: {item.Quantity}, Hiện có: {medicine.StockQuantity}");
                        }
                    }

                    // Process export and add prescription items
                    foreach (var item in dto.Prescriptions)
                    {

                        // Xuất kho tự động áp dụng FEFO qua MedicineService
                        await _medicineService.ExportMedicineAsync(new ExportMedicineDto
                        {
                            MedicineId = item.MedicineId,
                            Quantity = item.Quantity,
                            ReferenceCode = $"MR-{medicalRecord.Id}",
                            Notes = $"Kê đơn từ hồ sơ khám bệnh #{medicalRecord.Id}"
                        }, doctorId);

                        var prescriptionItem = new PrescriptionItem
                        {
                            PrescriptionId = prescription.Id,
                            MedicineId = item.MedicineId,
                            Dosage = item.Dosage,
                            Frequency = item.Frequency,
                            DurationDays = item.DurationDays,
                            Quantity = item.Quantity,
                            Instruction = item.Instruction
                        };

                        await _unitOfWork.PrescriptionItems.AddAsync(prescriptionItem);
                    }
                }

                // 4. Đồng bộ trạng thái cuộc hẹn
                appointment.Status = "completed";
                appointment.CheckOutTime = DateTime.UtcNow;
                _unitOfWork.Appointments.Update(appointment);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return medicalRecord.Id;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private string ExtractReadableSoap(string? jsonStr, string fieldType)
        {
            if (string.IsNullOrWhiteSpace(jsonStr)) return string.Empty;
            if (!jsonStr.TrimStart().StartsWith("{") && !jsonStr.TrimStart().StartsWith("[")) return jsonStr; // It's plain text

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            try
            {
                if (fieldType == "S")
                {
                    var obj = JsonSerializer.Deserialize<SubjectiveDto>(jsonStr, options);
                    if (obj == null) return jsonStr;
                    var parts = new List<string>();
                    if (!string.IsNullOrEmpty(obj.ChiefComplaint)) parts.Add($"Lý do khám: {obj.ChiefComplaint}");
                    if (!string.IsNullOrEmpty(obj.OnsetDuration)) parts.Add($"Thời gian phát bệnh: {obj.OnsetDuration}");
                    
                    // Tình trạng chung
                    var generalState = new List<string>();
                    if (!string.IsNullOrEmpty(obj.Appetite) && obj.Appetite != "Bình thường") generalState.Add($"Ăn uống: {obj.Appetite}");
                    if (!string.IsNullOrEmpty(obj.Thirst) && obj.Thirst != "Bình thường") generalState.Add($"Uống nước: {obj.Thirst}");
                    if (!string.IsNullOrEmpty(obj.ActivityLevel) && obj.ActivityLevel != "Bình thường") generalState.Add($"Hoạt động: {obj.ActivityLevel}");
                    if (!string.IsNullOrEmpty(obj.UrinationIssues) && obj.UrinationIssues != "Bình thường") generalState.Add($"Tiểu tiện: {obj.UrinationIssues}");
                    if (generalState.Any()) parts.Add(string.Join(", ", generalState));

                    // Triệu chứng đặc biệt
                    var symptoms = new List<string>();
                    if (obj.HasVomiting) symptoms.Add("Nôn ói" + (!string.IsNullOrEmpty(obj.VomitingDetails) ? $" ({obj.VomitingDetails})" : ""));
                    if (obj.HasDiarrhea) symptoms.Add("Tiêu chảy" + (!string.IsNullOrEmpty(obj.DiarrheaDetails) ? $" ({obj.DiarrheaDetails})" : ""));
                    if (obj.HasCoughing) symptoms.Add("Ho");
                    if (obj.HasSneezing) symptoms.Add("Hắt hơi");
                    if (obj.HasBreathingDifficulty) symptoms.Add("Khó thở");
                    if (obj.HasItching) symptoms.Add("Ngứa ngáy");
                    if (obj.HasHairLoss) symptoms.Add("Rụng lông");
                    if (symptoms.Any()) parts.Add("Triệu chứng: " + string.Join(", ", symptoms));

                    if (!string.IsNullOrEmpty(obj.CurrentMedications) && obj.CurrentMedications != "Không có") parts.Add($"Thuốc đang dùng: {obj.CurrentMedications}");
                    if (!string.IsNullOrEmpty(obj.PetOwnerNotes)) parts.Add($"Ghi chú chủ nuôi: {obj.PetOwnerNotes}");

                    return parts.Any() ? string.Join(" | ", parts) : "Khám tổng quát";
                }
                else if (fieldType == "O")
                {
                    var obj = JsonSerializer.Deserialize<ObjectiveDto>(jsonStr, options);
                    if (obj == null) return jsonStr;
                    var parts = new List<string>();
                    if (!string.IsNullOrEmpty(obj.Mentation)) parts.Add($"Tri giác: {obj.Mentation}");
                    if (obj.BodyConditionScore > 0) parts.Add($"BCS: {obj.BodyConditionScore}/9");
                    if (!string.IsNullOrEmpty(obj.Hydration) && !obj.Hydration.Contains("Bình thường")) parts.Add($"Mất nước: {obj.Hydration}");
                    if (obj.HeartRate.HasValue) parts.Add($"Nhịp tim: {obj.HeartRate} bpm");
                    if (obj.RespiratoryRate.HasValue) parts.Add($"Nhịp thở: {obj.RespiratoryRate} lần/phút");

                    // Các cơ quan bất thường
                    var abnormal = new List<string>();
                    if (obj.Eyes != null && !obj.Eyes.IsNormal) abnormal.Add("Mắt" + (!string.IsNullOrEmpty(obj.Eyes.Note) ? $": {obj.Eyes.Note}" : ""));
                    if (obj.Ears != null && !obj.Ears.IsNormal) abnormal.Add("Tai" + (!string.IsNullOrEmpty(obj.Ears.Note) ? $": {obj.Ears.Note}" : ""));
                    if (obj.Nose != null && !obj.Nose.IsNormal) abnormal.Add("Mũi" + (!string.IsNullOrEmpty(obj.Nose.Note) ? $": {obj.Nose.Note}" : ""));
                    if (obj.Mouth != null && !obj.Mouth.IsNormal) abnormal.Add("Miệng" + (!string.IsNullOrEmpty(obj.Mouth.Note) ? $": {obj.Mouth.Note}" : ""));
                    if (obj.SkinCoat != null && !obj.SkinCoat.IsNormal) abnormal.Add("Da lông" + (!string.IsNullOrEmpty(obj.SkinCoat.Note) ? $": {obj.SkinCoat.Note}" : ""));
                    if (obj.Gastrointestinal != null && !obj.Gastrointestinal.IsNormal) abnormal.Add("Tiêu hóa" + (!string.IsNullOrEmpty(obj.Gastrointestinal.Note) ? $": {obj.Gastrointestinal.Note}" : ""));
                    if (obj.Respiratory != null && !obj.Respiratory.IsNormal) abnormal.Add("Hô hấp" + (!string.IsNullOrEmpty(obj.Respiratory.Note) ? $": {obj.Respiratory.Note}" : ""));
                    if (abnormal.Any()) parts.Add("Bất thường: " + string.Join(", ", abnormal));

                    return parts.Any() ? string.Join(", ", parts) : string.Empty;
                }
                else if (fieldType == "A")
                {
                    var obj = JsonSerializer.Deserialize<AssessmentDto>(jsonStr, options);
                    if (obj == null) return jsonStr;
                    var diagnosis = !string.IsNullOrEmpty(obj.DefinitiveDiagnosis) ? obj.DefinitiveDiagnosis : obj.TentativeDiagnosis;
                    if (!string.IsNullOrEmpty(obj.DiseaseSeverity) && obj.DiseaseSeverity != "Nhẹ")
                        diagnosis += $" (Mức độ: {obj.DiseaseSeverity})";
                    return diagnosis ?? string.Empty;
                }
                else if (fieldType == "P")
                {
                    var obj = JsonSerializer.Deserialize<List<string>>(jsonStr, options);
                    return obj != null && obj.Any() ? string.Join(", ", obj) : string.Empty;
                }
            }
            catch
            {
                return jsonStr; // If parse fails, return raw string
            }
            return jsonStr;
        }


        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId)
        {
            // 1. Lấy tất cả bệnh án của thú cưng
            var records = await _unitOfWork.MedicalRecords.FindWithIncludesAsync(
                r => r.PetId == petId,
                r => r.Appointment!,
                r => r.Doctor!
            );

            // Sắp xếp giảm dần theo CreatedAt (mới nhất lên đầu)
            records = records.OrderByDescending(r => r.CreatedAt).ToList();

            var recordIds = records.Select(r => r.Id).ToList();
            if (!recordIds.Any())
            {
                return Enumerable.Empty<MedicalRecordDto>();
            }

            // 2. Lấy tất cả đơn thuốc của các bệnh án này
            var prescriptions = await _unitOfWork.Prescriptions.FindWithIncludesAsync(
                p => recordIds.Contains(p.MedicalRecordId)
            );

            var prescriptionIds = prescriptions.Select(p => p.Id).ToList();

            // 3. Lấy tất cả chi tiết đơn thuốc kèm theo thuốc
            var prescriptionItems = prescriptionIds.Any()
                ? await _unitOfWork.PrescriptionItems.FindWithIncludesAsync(
                    pi => prescriptionIds.Contains(pi.PrescriptionId),
                    pi => pi.Medicine!
                  )
                : Enumerable.Empty<PrescriptionItem>();

            var itemsGrouped = prescriptionItems.GroupBy(pi => pi.PrescriptionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var prescriptionsGrouped = prescriptions.GroupBy(p => p.MedicalRecordId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<MedicalRecordDto>();

            foreach (var r in records)
            {
                var prescribedMedicines = new List<PrescribedMedicineDto>();
                if (prescriptionsGrouped.TryGetValue(r.Id, out var recordPrescriptions))
                {
                    foreach (var p in recordPrescriptions)
                    {
                        if (itemsGrouped.TryGetValue(p.Id, out var items))
                        {
                            foreach (var pi in items)
                            {
                                prescribedMedicines.Add(new PrescribedMedicineDto
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

                result.Add(new MedicalRecordDto
                {
                    RecordId = r.Id,
                    AppointmentId = r.AppointmentId,
                    PetId = r.PetId,
                    PetName = r.Appointment?.Pet?.Name ?? string.Empty,
                    VisitDate = r.CreatedAt,
                    RecordType = r.RecordType,
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
                    PrescribedMedicines = prescribedMedicines
                });
            }

            return result;
        }

        public async Task<MedicalRecordDto?> GetMedicalRecordByAppointmentAsync(long appointmentId)
        {
            var records = await _unitOfWork.MedicalRecords.FindWithIncludesAsync(
                r => r.AppointmentId == appointmentId,
                r => r.Appointment!,
                r => r.Appointment!.Pet!,
                r => r.Doctor!
            );
            
            var record = records.FirstOrDefault();
            if (record == null) return null;

            var prescriptions = await _unitOfWork.Prescriptions.FindWithIncludesAsync(
                p => p.MedicalRecordId == record.Id
            );

            var prescriptionIds = prescriptions.Select(p => p.Id).ToList();
            var prescriptionItems = prescriptionIds.Any()
                ? await _unitOfWork.PrescriptionItems.FindWithIncludesAsync(
                    pi => prescriptionIds.Contains(pi.PrescriptionId),
                    pi => pi.Medicine!
                  )
                : Enumerable.Empty<PrescriptionItem>();

            var prescribedMedicines = new List<PrescribedMedicineDto>();
            foreach (var pi in prescriptionItems)
            {
                prescribedMedicines.Add(new PrescribedMedicineDto
                {
                    MedicineName = pi.Medicine?.Name ?? "Thuốc",
                    Dosage = pi.Dosage,
                    Frequency = pi.Frequency,
                    DurationDays = pi.DurationDays,
                    Quantity = pi.Quantity,
                    Instruction = pi.Instruction
                });
            }

            return new MedicalRecordDto
            {
                RecordId = record.Id,
                AppointmentId = record.AppointmentId,
                PetId = record.PetId,
                PetName = record.Appointment?.Pet?.Name ?? string.Empty,
                VisitDate = record.CreatedAt,
                RecordType = record.RecordType,
                MedicalHistory = ExtractReadableSoap(record.MedicalHistory, "S"),
                Diagnosis = ExtractReadableSoap(record.Diagnosis, "A"),
                TreatmentPlan = ExtractReadableSoap(record.TreatmentPlan, "P"),
                DoctorName = record.Doctor?.FullName ?? string.Empty,
                DoctorId = record.DoctorId.ToString(),
                Weight = record.Weight,
                Temperature = record.Temperature,
                ClinicalSigns = ExtractReadableSoap(record.ClinicalSigns, "O"),
                DoctorNotes = record.DoctorNotes ?? string.Empty,
                FollowUpDate = record.FollowUpDate,
                PrescribedMedicines = prescribedMedicines
            };
        }

        public async Task UpdateMedicalRecordAsync(long id, UpdateMedicalRecordDto dto, Guid doctorId)
        {
            var record = await _unitOfWork.MedicalRecords.GetByIdAsync(id);
            if (record == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ bệnh án.");

            if (record.DoctorId != doctorId)
                throw new UnauthorizedAccessException("Bạn không có quyền sửa bệnh án này.");

            record.Weight = dto.Weight;
            record.Temperature = dto.Temperature;
            record.ClinicalSigns = dto.ClinicalSigns;
            record.Diagnosis = dto.Diagnosis;
            record.TreatmentPlan = dto.TreatmentPlan;
            record.DoctorNotes = dto.DoctorNotes;
            record.FollowUpDate = dto.FollowUpDate;

            _unitOfWork.MedicalRecords.Update(record);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<long> CreateSoapMedicalRecordAsync(MedicalRecordSoapRequestDto dto, Guid doctorId)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(a => a.Id == dto.AppointmentId, a => a.Pet!);
                var appointment = appointments.FirstOrDefault();
                if (appointment == null) throw new KeyNotFoundException("Không tìm thấy cuộc hẹn.");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
                
                var medicalRecord = new MedicalRecord
                {
                    AppointmentId = dto.AppointmentId,
                    DoctorId = doctorId,
                    PetId = dto.PetId == 0 ? appointment.PetId : dto.PetId,
                    RecordType = "Consultation",
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
                await _unitOfWork.SaveChangesAsync(); 

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
                    await _unitOfWork.SaveChangesAsync(); 

                    // Validate all stock first
                    foreach (var item in dto.Plan.Prescriptions)
                    {
                        var medicine = await _unitOfWork.Medicines.GetByIdAsync(item.MedicineId);
                        if (medicine == null) throw new KeyNotFoundException($"Không tìm thấy thuốc với ID {item.MedicineId}");
                        if (medicine.StockQuantity < item.Quantity) throw new InvalidOperationException($"Thuốc '{medicine.Name}' không đủ tồn kho.");
                    }

                    // Process export
                    foreach (var item in dto.Plan.Prescriptions)
                    {

                        // Xuất kho tự động áp dụng FEFO
                        await _medicineService.ExportMedicineAsync(new ExportMedicineDto
                        {
                            MedicineId = item.MedicineId,
                            Quantity = item.Quantity,
                            ReferenceCode = $"MR-{medicalRecord.Id}",
                            Notes = $"Kê đơn SOAP #{medicalRecord.Id}"
                        }, doctorId);

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

                appointment.Status = "completed";
                appointment.CheckOutTime = DateTime.UtcNow;
                _unitOfWork.Appointments.Update(appointment);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return medicalRecord.Id;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<MedicalRecordSoapResponseDto?> GetSoapMedicalRecordByAppointmentAsync(long appointmentId)
        {
            var records = await _unitOfWork.MedicalRecords.FindWithIncludesAsync(
                r => r.AppointmentId == appointmentId,
                r => r.Appointment!,
                r => r.Appointment!.Pet!,
                r => r.Doctor!
            );
            
            var record = records.FirstOrDefault();
            if (record == null) return null;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var response = new MedicalRecordSoapResponseDto
            {
                RecordId = record.Id,
                AppointmentId = record.AppointmentId,
                PetId = record.PetId,
                PetName = record.Appointment?.Pet?.Name ?? string.Empty,
                VisitDate = record.CreatedAt,
                DoctorName = record.Doctor?.FullName ?? string.Empty,
                DoctorId = record.DoctorId.ToString(),
            };

            try { response.Subjective = string.IsNullOrEmpty(record.MedicalHistory) ? new SubjectiveDto() : JsonSerializer.Deserialize<SubjectiveDto>(record.MedicalHistory, options)!; } catch {}
            try { response.Objective = string.IsNullOrEmpty(record.ClinicalSigns) ? new ObjectiveDto() : JsonSerializer.Deserialize<ObjectiveDto>(record.ClinicalSigns, options)!; } catch {}
            try { response.Assessment = string.IsNullOrEmpty(record.Diagnosis) ? new AssessmentDto() : JsonSerializer.Deserialize<AssessmentDto>(record.Diagnosis, options)!; } catch {}
            
            response.Plan.CareInstructions = record.DoctorNotes ?? string.Empty;
            response.Plan.FollowUpDate = record.FollowUpDate;
            try { response.Plan.TreatmentDirections = string.IsNullOrEmpty(record.TreatmentPlan) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(record.TreatmentPlan, options)!; } catch {}

            var prescriptions = await _unitOfWork.Prescriptions.FindWithIncludesAsync(p => p.MedicalRecordId == record.Id);
            var prescriptionIds = prescriptions.Select(p => p.Id).ToList();
            if (prescriptionIds.Any())
            {
                var prescriptionItems = await _unitOfWork.PrescriptionItems.FindWithIncludesAsync(
                    pi => prescriptionIds.Contains(pi.PrescriptionId),
                    pi => pi.Medicine!
                );

                foreach (var pi in prescriptionItems)
                {
                    response.Plan.Prescriptions.Add(new PrescriptionLineDto
                    {
                        MedicineId = pi.MedicineId,
                        Quantity = pi.Quantity ?? 0,
                        Dosage = pi.Dosage,
                        Frequency = pi.Frequency,
                        DurationDays = pi.DurationDays,
                        Instruction = pi.Instruction
                    });
                }
            }

            return response;
        }
    }
}
