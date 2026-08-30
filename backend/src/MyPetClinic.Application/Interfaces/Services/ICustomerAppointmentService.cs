using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface ICustomerAppointmentService
    {
        Task<object> GetAvailableVaccinesAsync();

        Task<long> BookAppointmentAsync(MyPetClinic.Application.DTOs.CustomerBookingDto dto, Guid customerId, Guid userId);
    }
}
