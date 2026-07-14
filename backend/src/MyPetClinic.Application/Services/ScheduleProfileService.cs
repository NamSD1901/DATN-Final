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
    public class ScheduleProfileService : IScheduleProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScheduleProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ScheduleProfileDto>> GetAllProfilesAsync()
        {
            var profiles = await _unitOfWork.ScheduleProfiles.Query().ToListAsync();
            return profiles.Select(p => new ScheduleProfileDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive
            });
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

        public async Task<ScheduleProfileDto> GetProfileByIdAsync(long id)
        {
            var profile = await _unitOfWork.ScheduleProfiles.Query()
                .Include(p => p.Shifts)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (profile == null) throw new Exception("Profile not found");

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

        public async Task<ScheduleProfileDto> UpdateProfileAsync(long id, UpdateScheduleProfileDto dto)
        {
            var profile = await _unitOfWork.ScheduleProfiles.Query()
                .Include(p => p.Shifts)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null) throw new Exception("Profile not found");

            profile.Name = dto.Name;
            profile.Description = dto.Description;

            // Remove old shifts
            profile.Shifts.Clear();

            // Add new shifts
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

            _unitOfWork.ScheduleProfiles.Update(profile);
            await _unitOfWork.SaveChangesAsync();

            // Logic to update all future schedules for doctors using this profile
            var activeAssignments = await _unitOfWork.DoctorScheduleProfiles.Query()
                .Where(x => x.ProfileId == id && (x.EndDate == null || x.EndDate > DateTime.UtcNow))
                .Select(x => x.DoctorId)
                .Distinct()
                .ToListAsync();

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            
            foreach (var doctorId in activeAssignments)
            {
                // Delete future auto-generated schedules
                var futureSchedules = await _unitOfWork.DoctorSchedules.Query()
                    .Where(x => x.DoctorId == doctorId && x.WorkDate >= tomorrow && x.Notes.Contains("Generated from Profile"))
                    .ToListAsync();

                foreach (var schedule in futureSchedules)
                {
                    _unitOfWork.DoctorSchedules.Remove(schedule);
                }
            }
            await _unitOfWork.SaveChangesAsync();

            // Regenerate
            foreach (var doctorId in activeAssignments)
            {
                await GenerateScheduleFromProfileAsync(doctorId, 30);
            }

            return await GetProfileByIdAsync(id);
        }

        public async Task DeleteProfileAsync(long id)
        {
            var profile = await _unitOfWork.ScheduleProfiles.GetByIdAsync(id);
            if (profile == null) throw new Exception("Profile not found");

            // Soft delete
            profile.IsActive = false;
            _unitOfWork.ScheduleProfiles.Update(profile);
            await _unitOfWork.SaveChangesAsync();
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

                // Wipe out future generated schedules from EffectiveDate onwards 
                // so the new profile takes effect immediately and cleanly.
                var futureSchedules = await _unitOfWork.DoctorSchedules.Query()
                    .Where(x => x.DoctorId == doctorId && x.WorkDate >= newAssignment.EffectiveDate.Date)
                    .ToListAsync();
                
                foreach (var schedule in futureSchedules)
                {
                    _unitOfWork.DoctorSchedules.Remove(schedule);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            
            // Immediately generate schedules for the next 30 days to reflect on UI immediately
            foreach (var doctorId in dto.DoctorIds)
            {
                await GenerateScheduleFromProfileAsync(doctorId, 30);
            }
        }

        public async Task<int> GenerateScheduleFromProfileAsync(Guid doctorId, int daysToGenerate)
        {
            // Find active or future profile for the doctor
            var activeAssignment = await _unitOfWork.DoctorScheduleProfiles.Query()
                .AsNoTracking()
                .Include(p => p.Profile)
                .ThenInclude(p => p.Shifts)
                .Where(x => x.DoctorId == doctorId && 
                            (x.EndDate == null || x.EndDate > DateTime.UtcNow))
                .OrderByDescending(x => x.Id) // Pick the most recently assigned one if there are multiple future ones
                .FirstOrDefaultAsync();

            if (activeAssignment == null || activeAssignment.Profile == null || !activeAssignment.Profile.IsActive)
            {
                return 0; // No active profile
            }

            var shifts = activeAssignment.Profile.Shifts.ToList();
            if (!shifts.Any()) return 0;

            int generatedCount = 0;
            // Start generation from today, OR from EffectiveDate if it's in the future
            var startDate = DateTime.UtcNow.Date;
            if (activeAssignment.EffectiveDate > DateTime.UtcNow)
            {
                startDate = activeAssignment.EffectiveDate.Date;
            }

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
