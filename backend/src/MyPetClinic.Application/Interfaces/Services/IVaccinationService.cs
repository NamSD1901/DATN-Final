using MyPetClinic.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IVaccinationService
    {
        Task<VaccinationSoapResponseDto> GetSoapRecordByAppointmentAsync(long appointmentId, Guid currentUserId);
        Task<long> SubmitSoapRecordAsync(long appointmentId, Guid doctorId, VaccinationSoapRequestDto request);
        Task<IEnumerable<VaccinationSoapResponseDto>> GetPetVaccinationHistoryAsync(long petId);
        Task<IEnumerable<VaccineWithBatchesDto>> GetAvailableVaccinesAsync();
    }
}
