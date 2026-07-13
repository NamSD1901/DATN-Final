using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class ScheduleExceptionService : IScheduleExceptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScheduleExceptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ScheduleExceptionDto> CreateExceptionAsync(CreateScheduleExceptionDto dto)
        {
            var exception = new ScheduleException
            {
                DoctorId = dto.DoctorId,
                Type = dto.Type,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.ToUniversalTime(),
                SubstituteDoctorId = dto.SubstituteDoctorId,
                Reason = dto.Reason,
                Status = "Pending" // Set initial status to Pending
            };

            await _unitOfWork.ScheduleExceptions.AddAsync(exception);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(exception);
        }

        public async Task ApproveExceptionAsync(long exceptionId, ApproveScheduleExceptionDto dto)
        {
            var exception = await _unitOfWork.ScheduleExceptions.GetByIdAsync(exceptionId);
            if (exception == null) throw new Exception("Exception not found");

            exception.Status = dto.Status;
            _unitOfWork.ScheduleExceptions.Update(exception);

            if (dto.Status == "Approved")
            {
                // Process the schedule change
                var schedules = await _unitOfWork.DoctorSchedules.Query()
                    .Where(x => x.DoctorId == exception.DoctorId && 
                                x.WorkDate >= exception.StartDate.Date && 
                                x.WorkDate <= exception.EndDate.Date)
                    .ToListAsync();

                foreach (var schedule in schedules)
                {
                    if (exception.Type == "TimeOff")
                    {
                        schedule.IsAvailable = false;
                        schedule.Notes = $"Time-off approved: {exception.Reason}";
                    }
                    else if (exception.Type == "ShiftSwap" && exception.SubstituteDoctorId.HasValue)
                    {
                        schedule.IsAvailable = false;
                        schedule.Notes = $"Shift swapped to Doctor {exception.SubstituteDoctorId}";

                        // Create a new schedule for substitute doctor
                        var substituteSchedule = new DoctorSchedule
                        {
                            DoctorId = exception.SubstituteDoctorId.Value,
                            WorkDate = schedule.WorkDate,
                            StartTime = schedule.StartTime,
                            EndTime = schedule.EndTime,
                            IsAvailable = true,
                            MaxAppointments = schedule.MaxAppointments,
                            Notes = $"Covering shift for Doctor {exception.DoctorId}"
                        };
                        await _unitOfWork.DoctorSchedules.AddAsync(substituteSchedule);
                    }
                    _unitOfWork.DoctorSchedules.Update(schedule);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ScheduleExceptionDto>> GetAllPendingExceptionsAsync()
        {
            var exceptions = await _unitOfWork.ScheduleExceptions.Query()
                .Where(x => x.Status == "Pending")
                .ToListAsync();

            return exceptions.Select(MapToDto).ToList();
        }

        private ScheduleExceptionDto MapToDto(ScheduleException exception)
        {
            return new ScheduleExceptionDto
            {
                Id = exception.Id,
                DoctorId = exception.DoctorId,
                Type = exception.Type,
                StartDate = exception.StartDate,
                EndDate = exception.EndDate,
                SubstituteDoctorId = exception.SubstituteDoctorId,
                Reason = exception.Reason,
                Status = exception.Status
            };
        }
    }
}
