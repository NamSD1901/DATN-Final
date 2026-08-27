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
        Task<IEnumerable<MedicalRecordDto>> GetCustomerMedicalHistoryAsync(Guid customerId);
        Task<MedicalRecordDto?> GetMedicalRecordByAppointmentAsync(long appointmentId);
        Task UpdateMedicalRecordAsync(long id, UpdateMedicalRecordDto dto, Guid doctorId);
        
        // SOAP APIs
        Task<long> CreateSoapMedicalRecordAsync(MedicalRecordSoapRequestDto dto, Guid doctorId);
        Task<MedicalRecordSoapResponseDto?> GetSoapMedicalRecordByAppointmentAsync(long appointmentId);
        string ExtractReadableSoap(string? jsonStr, string fieldType);
    }
}
