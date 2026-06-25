using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IDoctorScheduleService
    {
        Task<IEnumerable<DoctorScheduleDto>> GetSchedulesAsync(DateTime? startDate, DateTime? endDate, Guid? doctorId);
        Task<DoctorScheduleDto?> GetScheduleByIdAsync(long id);
        Task<long> CreateScheduleAsync(DoctorScheduleCreateDto dto);
        Task<bool> UpdateScheduleAsync(long id, DoctorScheduleUpdateDto dto);
        Task<bool> DeleteScheduleAsync(long id);
    }
}
