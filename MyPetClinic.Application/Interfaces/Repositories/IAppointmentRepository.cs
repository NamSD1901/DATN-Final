using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(long id);
        Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(Guid customerId);
        Task<int> SaveChangesAsync();
    }
}
