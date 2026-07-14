using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IScheduleProfileService
    {
        Task<IEnumerable<ScheduleProfileDto>> GetAllProfilesAsync();
        Task<ScheduleProfileDto> GetProfileByIdAsync(long id);
        Task<ScheduleProfileDto> CreateProfileAsync(CreateScheduleProfileDto dto);
        Task<ScheduleProfileDto> UpdateProfileAsync(long id, UpdateScheduleProfileDto dto);
        Task DeleteProfileAsync(long id);
        Task AssignProfileToDoctorsAsync(AssignProfileDto dto);
        Task<int> GenerateScheduleFromProfileAsync(Guid doctorId, int daysToGenerate);
        Task<int> AutoGenerateSchedulesForAllDoctorsAsync(int targetDaysAhead);
    }
}
