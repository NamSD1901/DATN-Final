using MyPetClinic.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IMedicalRecordService
    {
        Task<long> CreateMedicalRecordAsync(CreateMedicalRecordDto dto, Guid doctorId);
        Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId);
    }
}
