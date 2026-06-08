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
        Task<bool> UpdateAppointmentStatusAsync(long id, string status);
        Task<bool> RescheduleAppointmentAsync(long id, DateTime newStart);
        Task<IEnumerable<ServiceDto>> GetServicesAsync();
        Task<AppointmentStatsDto> GetAppointmentStatsAsync();
        Task<IEnumerable<AppointmentDetailDto>> GetPendingAppointmentsAsync();
        Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id);
        Task<IEnumerable<AppointmentDetailDto>> GetCustomerAppointmentsAsync(Guid customerId);
        Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId);
    }
}
