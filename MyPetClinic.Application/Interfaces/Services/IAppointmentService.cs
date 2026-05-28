using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(Guid doctorId, DateTime? date = null);
        Task<bool> UpdateAppointmentStatusAsync(long appointmentId, string status);
    }
}
