using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class VaccinationService : IVaccinationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VaccinationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VaccinationSoapResponseDto> GetSoapRecordByAppointmentAsync(long appointmentId, Guid currentUserId)
        {
            var record = await _unitOfWork.VaccinationRecords.GetFirstOrDefaultWithIncludesAsync(
                r => r.AppointmentId == appointmentId,
                r => r.Pet!, r => r.Vaccine!, r => r.Doctor!, r => r.VaccineBatch!);

            if (record == null)
            {
                return null!;
            }

            return MapToResponseDto(record);
        }

        public async Task<long> SubmitSoapRecordAsync(long appointmentId, Guid doctorId, VaccinationSoapRequestDto request)
        {
            var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(a => a.Id == appointmentId, a => a.Pet!);
            var appointment = appointments.FirstOrDefault();
            if (appointment == null)
                throw new KeyNotFoundException("Không tìm thấy cuộc hẹn.");

            if (appointment.Status == "completed" || appointment.Status == "cancelled")
                throw new InvalidOperationException("Cuộc hẹn đã kết thúc, không thể lưu bệnh án.");

            // 1. Tạo bản ghi SOAP
            var record = CreateVaccinationRecord(appointment, doctorId, request);
            
            // 2. Xử lý tồn kho & Hóa đơn (chỉ khi đủ điều kiện tiêm)
            if (request.ClinicalAssessment == "Đủ điều kiện")
            {
                await ProcessInventoryAndInvoiceAsync(appointment, request);
            }
            else if (request.ClinicalAssessment == "Hoãn tiêm")
            {
                // Xử lý tạo phí khám (nếu có) thay vì phí vắc-xin
                await CreateConsultationFeeOnlyAsync(appointment);
            }

            // 3. Cập nhật ngày hẹn & Trạng thái cuộc hẹn
            appointment.Status = "completed";
            appointment.CheckOutTime = DateTime.UtcNow;
            
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.VaccinationRecords.AddAsync(record);
            
            // Cập nhật ưu tiên cân nặng từ Bác sĩ
            if (appointment.Pet != null && request.Weight > 0)
            {
                appointment.Pet.Weight = request.Weight;
                _unitOfWork.Pets.Update(appointment.Pet);
            }
            
            await _unitOfWork.SaveChangesAsync();

            return record.Id;
        }

        public async Task<IEnumerable<VaccinationSoapResponseDto>> GetPetVaccinationHistoryAsync(long petId)
        {
            var records = await _unitOfWork.VaccinationRecords.FindWithIncludesAsync(
                r => r.PetId == petId,
                r => r.Pet!, r => r.Vaccine!, r => r.Doctor!, r => r.VaccineBatch!);

            return records.OrderByDescending(r => r.InjectionDate).Select(MapToResponseDto);
        }

        public async Task<IEnumerable<VaccineWithBatchesDto>> GetAvailableVaccinesAsync()
        {
            var vaccines = await _unitOfWork.Vaccines.GetAllAsync();
            var batches = await _unitOfWork.VaccineBatches.FindAsync(b => b.StockQuantity > 0 && b.ExpirationDate > DateTime.UtcNow);
            
            var result = vaccines.Select(v => new VaccineWithBatchesDto
            {
                Id = v.Id,
                Name = v.Name,
                Manufacturer = v.Manufacturer,
                TargetSpecies = v.TargetSpecies,
                MinAgeWeeks = v.MinAgeWeeks,
                IntervalDays = v.IntervalDays,
                Batches = batches.Where(b => b.VaccineId == v.Id).Select(b => new VaccineBatchDto
                {
                    Id = b.Id,
                    VaccineId = b.VaccineId,
                    BatchNumber = b.BatchNumber,
                    ExpirationDate = b.ExpirationDate,
                    StockQuantity = b.StockQuantity,
                    SellingPrice = b.SellingPrice
                }).ToList()
            }).Where(v => v.Batches.Any()).ToList();

            return result;
        }

        #region Private Helper Methods

        private VaccinationRecord CreateVaccinationRecord(Appointment appointment, Guid doctorId, VaccinationSoapRequestDto request)
        {
            return new VaccinationRecord
            {
                AppointmentId = appointment.Id,
                PetId = appointment.PetId,
                DoctorId = doctorId,
                InjectionDate = DateTime.UtcNow,
                
                ReasonForVisit = request.ReasonForVisit,
                PreviousVaccineHistory = request.PreviousVaccineHistory,
                IsAllergic = request.IsAllergic,
                AllergyDetails = request.AllergyDetails,
                HasPreviousReaction = request.HasPreviousReaction,
                PreviousReactionDetails = request.PreviousReactionDetails,
                IsUnderTreatment = request.IsUnderTreatment,
                TreatmentDetails = request.TreatmentDetails,
                EatingStatus = request.EatingStatus,
                HasVomitingOrDiarrhea = request.HasVomitingOrDiarrhea,
                HasCoughOrSneeze = request.HasCoughOrSneeze,
                OwnerNotes = request.OwnerNotes,

                Weight = request.Weight,
                Temperature = request.Temperature,
                HeartRate = request.HeartRate,
                RespiratoryRate = request.RespiratoryRate,
                MentalStatus = request.MentalStatus,
                MucosaStatus = request.MucosaStatus,
                EyeNoseEarStatus = request.EyeNoseEarStatus,
                LymphNodeStatus = request.LymphNodeStatus,
                DehydrationPercent = request.DehydrationPercent,

                VaccineId = request.VaccineId,
                VaccineBatchId = request.VaccineBatchId,
                Dose = request.Dose,
                Route = request.Route,
                InjectionSite = request.InjectionSite,

                ClinicalAssessment = request.ClinicalAssessment,
                DoctorRemarks = request.DoctorRemarks,

                NextDueDate = request.NextDueDate,
                FollowUpInstructions = request.FollowUpInstructions,
                ReactionNote = request.ReactionNote
            };
        }

        private async Task ProcessInventoryAndInvoiceAsync(Appointment appointment, VaccinationSoapRequestDto request)
        {
            if (!request.VaccineId.HasValue || !request.VaccineBatchId.HasValue)
                throw new InvalidOperationException("Vui lòng chọn vắc-xin và lô hàng.");

            var batch = await _unitOfWork.VaccineBatches.GetByIdAsync(request.VaccineBatchId.Value);
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(request.VaccineId.Value);

            if (batch == null || vaccine == null)
                throw new KeyNotFoundException("Lô vắc-xin không tồn tại.");

            if (batch.StockQuantity <= 0 || batch.ExpirationDate < DateTime.UtcNow)
                throw new InvalidOperationException("Lô vắc-xin đã hết hạn hoặc hết hàng.");

            // Trừ tồn kho lô
            batch.StockQuantity -= 1;
            _unitOfWork.VaccineBatches.Update(batch);
            
            // Trừ tồn kho tổng
            vaccine.StockQuantity -= 1;
            _unitOfWork.Vaccines.Update(vaccine);

            // Sinh hóa đơn
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(i => i.AppointmentId == appointment.Id);
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    AppointmentId = appointment.Id,
                    Subtotal = batch.SellingPrice,
                    TotalAmount = batch.SellingPrice,
                    PaymentStatus = "unpaid"
                };
                await _unitOfWork.Invoices.AddAsync(invoice);
            }
            else 
            {
                invoice.Subtotal += batch.SellingPrice;
                invoice.TotalAmount += batch.SellingPrice;
                _unitOfWork.Invoices.Update(invoice);
            }
            
            var invoiceItem = new InvoiceItem
            {
                Invoice = invoice,
                ItemType = "Vaccine",
                ItemId = vaccine.Id,
                ItemName = $"Tiêm phòng: {vaccine.Name} (Lô: {batch.BatchNumber})",
                Quantity = 1,
                UnitPrice = batch.SellingPrice,
                TotalPrice = batch.SellingPrice
            };
            
            await _unitOfWork.InvoiceItems.AddAsync(invoiceItem);
        }

        private async Task CreateConsultationFeeOnlyAsync(Appointment appointment)
        {
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(i => i.AppointmentId == appointment.Id);
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    AppointmentId = appointment.Id,
                    Subtotal = 100000, // Phí khám lâm sàng
                    TotalAmount = 100000,
                    PaymentStatus = "unpaid"
                };
                await _unitOfWork.Invoices.AddAsync(invoice);
            }
            
            var invoiceItem = new InvoiceItem
            {
                Invoice = invoice,
                ItemType = "Service",
                ItemId = appointment.ServiceId,
                ItemName = "Phí khám lâm sàng (Hoãn tiêm)",
                Quantity = 1,
                UnitPrice = 100000,
                TotalPrice = 100000
            };
            await _unitOfWork.InvoiceItems.AddAsync(invoiceItem);
        }

        private VaccinationSoapResponseDto MapToResponseDto(VaccinationRecord r)
        {
            return new VaccinationSoapResponseDto
            {
                Id = r.Id,
                AppointmentId = r.AppointmentId ?? 0,
                PetId = r.PetId,
                DoctorId = r.DoctorId,
                DoctorName = r.Doctor?.FullName ?? string.Empty,
                CreatedAt = r.CreatedAt,
                
                ReasonForVisit = r.ReasonForVisit,
                PreviousVaccineHistory = r.PreviousVaccineHistory,
                IsAllergic = r.IsAllergic,
                AllergyDetails = r.AllergyDetails,
                HasPreviousReaction = r.HasPreviousReaction,
                PreviousReactionDetails = r.PreviousReactionDetails,
                IsUnderTreatment = r.IsUnderTreatment,
                TreatmentDetails = r.TreatmentDetails,
                EatingStatus = r.EatingStatus,
                HasVomitingOrDiarrhea = r.HasVomitingOrDiarrhea,
                HasCoughOrSneeze = r.HasCoughOrSneeze,
                OwnerNotes = r.OwnerNotes,

                Weight = r.Weight,
                Temperature = r.Temperature,
                HeartRate = r.HeartRate,
                RespiratoryRate = r.RespiratoryRate,
                MentalStatus = r.MentalStatus,
                MucosaStatus = r.MucosaStatus,
                EyeNoseEarStatus = r.EyeNoseEarStatus,
                LymphNodeStatus = r.LymphNodeStatus,
                DehydrationPercent = r.DehydrationPercent,

                VaccineId = r.VaccineId,
                VaccineName = r.Vaccine?.Name,
                VaccineBatchId = r.VaccineBatchId,
                BatchNumber = r.VaccineBatch?.BatchNumber,
                Dose = r.Dose,
                Route = r.Route,
                InjectionSite = r.InjectionSite,

                ClinicalAssessment = r.ClinicalAssessment,
                DoctorRemarks = r.DoctorRemarks,

                InjectionDate = r.InjectionDate,
                NextDueDate = r.NextDueDate,
                FollowUpInstructions = r.FollowUpInstructions,
                ReactionNote = r.ReactionNote
            };
        }

        #endregion
    }
}
