using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(Guid doctorId, DateTime? date = null);
        Task<Appointment?> GetByIdAsync(long id);
        Task UpdateAsync(Appointment appointment);
    }
}
