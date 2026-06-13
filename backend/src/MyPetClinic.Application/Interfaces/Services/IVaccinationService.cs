using MyPetClinic.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IVaccinationService
    {
        Task<long> RecordVaccinationAsync(long petId, long appointmentId, long vaccineId, Guid doctorId, string? notes);
        Task<IEnumerable<VaccinationRecordDto>> GetPetVaccinationHistoryAsync(long petId);
    }
}
