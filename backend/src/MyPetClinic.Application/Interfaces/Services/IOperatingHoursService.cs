using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IOperatingHoursService
    {
        Task<IEnumerable<ClinicOperatingDayDto>> GetOperatingHoursAsync();
        Task<bool> UpdateOperatingHoursAsync(UpdateOperatingHoursDto dto);
        
        Task<IEnumerable<ClinicHolidayDto>> GetHolidaysAsync();
        Task<ClinicHolidayDto> CreateHolidayAsync(CreateHolidayDto dto, Guid createdBy);
        Task<bool> UpdateHolidayAsync(int id, UpdateHolidayDto dto);
        Task<bool> DeleteHolidayAsync(int id);
    }
}
