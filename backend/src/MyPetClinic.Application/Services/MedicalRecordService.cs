using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

namespace MyPetClinic.Application.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalRecordService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                        // Trừ tồn kho
                        medicine.StockQuantity -= item.Quantity;
                        _unitOfWork.Medicines.Update(medicine);

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

        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId)
        {
            // 1. Lấy tất cả bệnh án của thú cưng
            var records = await _unitOfWork.MedicalRecords.FindWithIncludesAsync(
                r => r.Appointment != null && r.Appointment.PetId == petId,
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
                var prescribedMedicines = new List<string>();
                if (prescriptionsGrouped.TryGetValue(r.Id, out var recordPrescriptions))
                {
                    foreach (var p in recordPrescriptions)
                    {
                        if (itemsGrouped.TryGetValue(p.Id, out var items))
                        {
                            foreach (var pi in items)
                            {
                                var medName = pi.Medicine?.Name ?? "Thuốc";
                                var medUnit = pi.Medicine?.Unit ?? "đơn vị";
                                prescribedMedicines.Add($"{medName} ({pi.Quantity} {medUnit}) - {pi.Dosage} {pi.Frequency}");
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
                    MedicalHistory = r.MedicalHistory ?? string.Empty,
                    Diagnosis = r.Diagnosis ?? string.Empty,
                    TreatmentPlan = r.TreatmentPlan ?? string.Empty,
                    DoctorName = r.Doctor?.FullName ?? string.Empty,
                    DoctorId = r.DoctorId.ToString(),
                    Weight = r.Weight,
                    Temperature = r.Temperature,
                    ClinicalSigns = r.ClinicalSigns ?? string.Empty,
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

            var prescribedMedicines = new List<string>();
            foreach (var pi in prescriptionItems)
            {
                var medName = pi.Medicine?.Name ?? "Thuốc";
                var medUnit = pi.Medicine?.Unit ?? "đơn vị";
                prescribedMedicines.Add($"{medName} ({pi.Quantity} {medUnit}) - {pi.Dosage} {pi.Frequency}");
            }

            return new MedicalRecordDto
            {
                RecordId = record.Id,
                AppointmentId = record.AppointmentId,
                PetId = record.PetId,
                PetName = record.Appointment?.Pet?.Name ?? string.Empty,
                VisitDate = record.CreatedAt,
                RecordType = record.RecordType,
                MedicalHistory = record.MedicalHistory ?? string.Empty,
                Diagnosis = record.Diagnosis ?? string.Empty,
                TreatmentPlan = record.TreatmentPlan ?? string.Empty,
                DoctorName = record.Doctor?.FullName ?? string.Empty,
                DoctorId = record.DoctorId.ToString(),
                Weight = record.Weight,
                Temperature = record.Temperature,
                ClinicalSigns = record.ClinicalSigns ?? string.Empty,
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
    }
}
