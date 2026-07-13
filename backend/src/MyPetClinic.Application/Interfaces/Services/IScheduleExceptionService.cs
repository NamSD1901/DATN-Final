using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IScheduleExceptionService
    {
        Task<ScheduleExceptionDto> CreateExceptionAsync(CreateScheduleExceptionDto dto);
        Task ApproveExceptionAsync(long exceptionId, ApproveScheduleExceptionDto dto);
        Task<List<ScheduleExceptionDto>> GetAllPendingExceptionsAsync();
    }
}
