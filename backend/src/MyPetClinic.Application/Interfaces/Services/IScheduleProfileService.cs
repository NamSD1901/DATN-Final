using System;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IScheduleProfileService
    {
        Task<ScheduleProfileDto> CreateProfileAsync(CreateScheduleProfileDto dto);
        Task AssignProfileToDoctorsAsync(AssignProfileDto dto);
        Task<int> GenerateScheduleFromProfileAsync(Guid doctorId, int daysToGenerate);
        Task<int> AutoGenerateSchedulesForAllDoctorsAsync(int targetDaysAhead);
    }
}
