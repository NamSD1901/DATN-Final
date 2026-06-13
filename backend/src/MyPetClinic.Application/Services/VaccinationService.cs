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

        public async Task<long> RecordVaccinationAsync(long petId, long appointmentId, long vaccineId, Guid doctorId, string? notes)
        {
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(vaccineId);
            if (vaccine == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy vắc-xin với ID {vaccineId}");
            }

            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cuộc hẹn với ID {appointmentId}");
            }

            // Tính NextDueDate dựa trên chu kỳ tiêm phòng (IntervalDays) của vaccine
            DateTime? nextDueDate = vaccine.IntervalDays.HasValue && vaccine.IntervalDays.Value > 0
                ? DateTime.Today.AddDays(vaccine.IntervalDays.Value)
                : null;

            var record = new VaccinationRecord
            {
                PetId = petId,
                VaccineId = vaccineId,
                AppointmentId = appointmentId,
                DoctorId = doctorId,
                InjectionDate = DateTime.Today,
                NextDueDate = nextDueDate,
                ReactionNote = notes
            };

            await _unitOfWork.VaccinationRecords.AddAsync(record);

            // Giảm số lượng vắc-xin trong kho
            vaccine.StockQuantity -= 1;
            if (vaccine.StockQuantity < 0) vaccine.StockQuantity = 0;
            _unitOfWork.Vaccines.Update(vaccine);

            // Đồng bộ cuộc hẹn thành Completed
            appointment.Status = "completed";
            appointment.CheckOutTime = DateTime.UtcNow;
            _unitOfWork.Appointments.Update(appointment);

            await _unitOfWork.SaveChangesAsync();

            return record.Id;
        }

        public async Task<IEnumerable<VaccinationRecordDto>> GetPetVaccinationHistoryAsync(long petId)
        {
            var records = await _unitOfWork.VaccinationRecords.FindWithIncludesAsync(
                r => r.PetId == petId,
                r => r.Pet!,
                r => r.Vaccine!,
                r => r.Doctor!
            );

            return records.OrderByDescending(r => r.InjectionDate).Select(r => new VaccinationRecordDto
            {
                RecordId = r.Id,
                PetId = r.PetId,
                PetName = r.Pet?.Name ?? string.Empty,
                VaccineId = r.VaccineId,
                VaccineName = r.Vaccine?.Name ?? string.Empty,
                DoctorName = r.Doctor?.FullName ?? string.Empty,
                InjectionDate = r.InjectionDate,
                NextDueDate = r.NextDueDate,
                ReactionNote = r.ReactionNote
            });
        }
    }
}
