using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface ICustomerAppointmentService
    {
        Task<object> GetAvailableVaccinesAsync();
        Task<object> ValidateVaccineAsync(Guid customerId, long petId, long vaccineId, DateTime targetDate);
    }
}
