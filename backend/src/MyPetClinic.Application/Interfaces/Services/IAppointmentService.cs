using MyPetClinic.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, Guid createdBy);
        Task<long> CreateAppointmentWithNewCustomerAsync(AppointmentWithNewCustomerDto dto, Guid createdBy);
        Task<IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId);
        Task<bool> UpdateAppointmentStatusAsync(long id, string status, string? reason = null);
        Task<bool> RescheduleAppointmentAsync(long id, DateTime newStart, bool force = false);
        Task<bool> UpdateAppointmentDoctorAsync(long id, Guid newDoctorId, bool force = false);
        Task<IEnumerable<ServiceDto>> GetServicesAsync();
        Task<AppointmentStatsDto> GetAppointmentStatsAsync();
        Task<IEnumerable<AppointmentDetailDto>> GetPendingAppointmentsAsync();
        Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id);
        Task<IEnumerable<AppointmentDetailDto>> GetCustomerAppointmentsAsync(Guid customerId);
        Task<PaginatedResultDto<AppointmentDetailDto>> GetCustomerAppointmentsPaginatedAsync(Guid customerId, string? status, int page, int pageSize);
        Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId);
        Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid ownerId);
        Task<IEnumerable<DoctorAvailableSlotsDto>> GetAvailableSlotsAsync(DateTime date);
        Task<AppointmentDetailDto?> CheckInByQrAsync(string qrToken);
    }
}
