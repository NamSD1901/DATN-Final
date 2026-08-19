using MyPetClinic.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IReceptionistAppointmentService
    {
        Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, Guid createdBy);
        Task<long> CreateAppointmentWithNewCustomerAsync(AppointmentWithNewCustomerDto dto, Guid createdBy);
        Task<IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId);
        Task<bool> UpdateAppointmentStatusAsync(long id, string status, string? reason = null);
        Task<bool> RescheduleAppointmentAsync(long id, DateTime newStart, bool force = false);
        Task<bool> UpdateAppointmentDoctorAsync(long id, ChangeDoctorRequestDto request);
        Task<AppointmentStatsDto> GetAppointmentStatsAsync();
        Task<IEnumerable<AppointmentDetailDto>> GetPendingAppointmentsAsync();
        Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id);
        Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId);
        Task<IEnumerable<DoctorAvailableSlotsDto>> GetAvailableSlotsAsync(DateTime date, long? serviceId = null);
        Task<IEnumerable<EligibleDoctorDto>> GetSuitableDoctorsForAppointmentAsync(long appointmentId);
        Task<List<AppointmentDetailDto>> CheckInAsync(CheckInBulkRequestDto request);
        Task<IEnumerable<ServiceDto>> GetServicesAsync();
    }
}
