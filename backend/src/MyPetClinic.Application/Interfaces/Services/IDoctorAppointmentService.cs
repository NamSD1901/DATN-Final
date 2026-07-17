using MyPetClinic.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IDoctorAppointmentService
    {
        Task<IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId);
        Task<bool> UpdateAppointmentStatusAsync(long id, string status, string? reason = null);
        Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id);
        Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId);
        Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid ownerId);
    }
}
