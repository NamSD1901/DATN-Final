using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    /// <summary>
    /// Application Service xử lý nghiệp vụ đơn thuốc.
    /// Tầng Application chứa logic tính trạng thái đơn thuốc (active/completed)
    /// dựa trên thời gian dùng thuốc lâu nhất trong đơn.
    /// </summary>
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrescriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PrescriptionDto>> GetPetPrescriptionsAsync(long petId)
        {
            var prescriptions = await _unitOfWork.Prescriptions.Query()
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m!.Appointment)
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(pi => pi.Medicine)
                .Where(p => p.MedicalRecord != null
                         && p.MedicalRecord.Appointment != null
                         && p.MedicalRecord.Appointment.PetId == petId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = new List<PrescriptionDto>();

            foreach (var prescription in prescriptions)
            {
                var dto = MapToDto(prescription);
                result.Add(dto);
            }

            return result;
        }

        // -----------------------------------------------------------------------
        // Business Rule: Tính trạng thái đơn thuốc
        //   - "active"    : đơn còn trong thời gian dùng thuốc (chưa hết hạn)
        //   - "completed" : đơn đã hết thời gian dùng thuốc
        // Công thức: ngày kê đơn + số ngày dùng thuốc dài nhất >= hôm nay
        // -----------------------------------------------------------------------
        private static PrescriptionDto MapToDto(Domain.Entities.Prescription prescription)
        {
            var dto = new PrescriptionDto
            {
                Id        = prescription.Id,
                Diagnosis = prescription.MedicalRecord?.Diagnosis ?? string.Empty,
                Date      = prescription.CreatedAt,
                Doctor    = prescription.Doctor != null ? $"BS. {prescription.Doctor.FullName}" : "Bác sĩ thú y",
                Notes     = prescription.Note,
                Status    = "completed"
            };

            int maxDurationDays = 0;

            foreach (var item in prescription.PrescriptionItems)
            {
                if (item.DurationDays.HasValue && item.DurationDays.Value > maxDurationDays)
                    maxDurationDays = item.DurationDays.Value;

                dto.Medicines.Add(new PrescriptionItemDto
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

            // Business rule: Đơn còn hiệu lực nếu ngày hết hạn >= hôm nay (UTC)
            if (maxDurationDays > 0 && prescription.CreatedAt.AddDays(maxDurationDays) >= DateTime.UtcNow)
            {
                dto.Status = "active";
            }

            return dto;
        }
    }
}
