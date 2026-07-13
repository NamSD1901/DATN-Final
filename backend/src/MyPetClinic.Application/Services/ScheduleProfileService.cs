using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class ScheduleProfileService : IScheduleProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScheduleProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ScheduleProfileDto> CreateProfileAsync(CreateScheduleProfileDto dto)
        {
            var profile = new ScheduleProfile
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var shiftDto in dto.Shifts)
            {
                profile.Shifts.Add(new ScheduleProfileShift
                {
                    DayOfWeek = shiftDto.DayOfWeek,
                    StartTime = shiftDto.StartTime,
                    EndTime = shiftDto.EndTime,
                    IsDayOff = shiftDto.IsDayOff
                });
            }

            await _unitOfWork.ScheduleProfiles.AddAsync(profile);
            await _unitOfWork.SaveChangesAsync();

            return new ScheduleProfileDto
            {
                Id = profile.Id,
                Name = profile.Name,
                Description = profile.Description,
                IsActive = profile.IsActive,
                Shifts = profile.Shifts.Select(s => new ScheduleProfileShiftDto
                {
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsDayOff = s.IsDayOff
                }).ToList()
            };
        }

        public async Task AssignProfileToDoctorsAsync(AssignProfileDto dto)
        {
            var profile = await _unitOfWork.ScheduleProfiles.GetByIdAsync(dto.ProfileId);
            if (profile == null) throw new Exception("Profile not found");

            foreach (var doctorId in dto.DoctorIds)
            {
                // Deactivate current active assignments by setting EndDate
                var currentAssignments = await _unitOfWork.DoctorScheduleProfiles.FindAsync(
                    x => x.DoctorId == doctorId && (x.EndDate == null || x.EndDate > DateTime.UtcNow));

                foreach (var assignment in currentAssignments)
                {
                    assignment.EndDate = DateTime.UtcNow;
                    _unitOfWork.DoctorScheduleProfiles.Update(assignment);
                }

                // Add new assignment
                var newAssignment = new DoctorScheduleProfile
                {
                    DoctorId = doctorId,
                    ProfileId = dto.ProfileId,
                    EffectiveDate = dto.EffectiveDate.ToUniversalTime(),
                    EndDate = dto.EndDate?.ToUniversalTime()
                };
                await _unitOfWork.DoctorScheduleProfiles.AddAsync(newAssignment);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GenerateScheduleFromProfileAsync(Guid doctorId, int daysToGenerate)
        {
            // Find active profile for the doctor
            var activeAssignment = await _unitOfWork.DoctorScheduleProfiles.Query()
                .Include(p => p.Profile)
                .ThenInclude(p => p.Shifts)
                .Where(x => x.DoctorId == doctorId && 
                            x.EffectiveDate <= DateTime.UtcNow && 
                            (x.EndDate == null || x.EndDate > DateTime.UtcNow))
                .FirstOrDefaultAsync();

            if (activeAssignment == null || activeAssignment.Profile == null || !activeAssignment.Profile.IsActive)
            {
                return 0; // No active profile
            }

            var shifts = activeAssignment.Profile.Shifts.ToList();
            if (!shifts.Any()) return 0;

            int generatedCount = 0;
            var startDate = DateTime.UtcNow.Date;

            // Get existing schedules to avoid duplicates
            var existingSchedules = await _unitOfWork.DoctorSchedules.Query()
                .Where(x => x.DoctorId == doctorId && x.WorkDate >= startDate && x.WorkDate <= startDate.AddDays(daysToGenerate))
                .ToListAsync();
                
            var existingDates = existingSchedules.Select(x => x.WorkDate.Date).ToHashSet();

            for (int i = 0; i < daysToGenerate; i++)
            {
                var currentDate = startDate.AddDays(i);

                if (existingDates.Contains(currentDate))
                {
                    continue; // Skip if already has schedule
                }

                var dayOfWeek = currentDate.DayOfWeek;
                var dailyShifts = shifts.Where(s => s.DayOfWeek == dayOfWeek).ToList();

                foreach (var shift in dailyShifts)
                {
                    if (shift.IsDayOff) continue;

                    var doctorSchedule = new DoctorSchedule
                    {
                        DoctorId = doctorId,
                        WorkDate = currentDate,
                        StartTime = shift.StartTime,
                        EndTime = shift.EndTime,
                        IsAvailable = true,
                        MaxAppointments = 10,
                        Notes = $"Generated from Profile: {activeAssignment.Profile.Name}"
                    };
                    await _unitOfWork.DoctorSchedules.AddAsync(doctorSchedule);
                    generatedCount++;
                }
            }

            if (generatedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return generatedCount;
        }

        public async Task<int> AutoGenerateSchedulesForAllDoctorsAsync(int targetDaysAhead)
        {
            var activeProfiles = await _unitOfWork.DoctorScheduleProfiles.Query()
                .Where(x => x.EffectiveDate <= DateTime.UtcNow && 
                            (x.EndDate == null || x.EndDate > DateTime.UtcNow))
                .Select(x => x.DoctorId)
                .Distinct()
                .ToListAsync();

            int totalGenerated = 0;
            foreach (var doctorId in activeProfiles)
            {
                totalGenerated += await GenerateScheduleFromProfileAsync(doctorId, targetDaysAhead);
            }
            return totalGenerated;
        }
    }
}
