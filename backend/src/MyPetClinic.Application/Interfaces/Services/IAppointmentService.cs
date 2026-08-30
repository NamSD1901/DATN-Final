using MyPetClinic.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, Guid createdBy);
        Task<IEnumerable<ServiceDto>> GetServicesAsync();
        Task<bool> UpdateAppointmentStatusAsync(long id, string status, string? reason = null);
        Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id);
        Task<IEnumerable<AppointmentDetailDto>> GetCustomerAppointmentsAsync(Guid customerId);
        Task<PaginatedResultDto<AppointmentDetailDto>> GetCustomerAppointmentsPaginatedAsync(Guid customerId, string? status, int page, int pageSize);
        Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId);
        Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid ownerId);
        Task<PaginatedResultDto<MedicalRecordDto>> GetPetMedicalHistoryPaginatedAsync(long petId, Guid ownerId, int page, int pageSize);
        Task<IEnumerable<DoctorAvailableSlotsDto>> GetAvailableSlotsAsync(DateTime date, long? serviceId = null);
    }
}
