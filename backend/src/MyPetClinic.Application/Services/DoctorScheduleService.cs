using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorScheduleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DoctorScheduleDto>> GetSchedulesAsync(DateTime? startDate, DateTime? endDate, Guid? doctorId)
        {
            var query = _unitOfWork.DoctorSchedules.Query();

            if (startDate.HasValue)
            {
                query = query.Where(s => s.WorkDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.WorkDate <= endDate.Value.Date);
            }

            if (doctorId.HasValue)
            {
                query = query.Where(s => s.DoctorId == doctorId.Value);
            }

            var schedules = query.ToList();
            var doctorIds = schedules.Select(s => s.DoctorId).Distinct().ToList();
            
            var doctors = _unitOfWork.Users.Query()
                .Where(u => doctorIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName ?? string.Empty);

            return schedules.OrderBy(s => s.WorkDate).ThenBy(s => s.StartTime).Select(s => new DoctorScheduleDto
            {
                Id = s.Id,
                DoctorId = s.DoctorId,
                DoctorName = doctors.TryGetValue(s.DoctorId, out var name) ? name : string.Empty,
                WorkDate = s.WorkDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                MaxAppointments = s.MaxAppointments,
                IsAvailable = s.IsAvailable
            });
        }

        public async Task<DoctorScheduleDto?> GetScheduleByIdAsync(long id)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null) return null;

            var user = await _unitOfWork.Users.GetByIdAsync(schedule.DoctorId);

            return new DoctorScheduleDto
            {
                Id = schedule.Id,
                DoctorId = schedule.DoctorId,
                DoctorName = user?.FullName ?? string.Empty,
                WorkDate = schedule.WorkDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                MaxAppointments = schedule.MaxAppointments,
                IsAvailable = schedule.IsAvailable
            };
        }

        public async Task<long> CreateScheduleAsync(DoctorScheduleCreateDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
            {
                throw new InvalidOperationException("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            }

            var workDate = dto.WorkDate.Date;

            // BR02: Không phân ca quá khứ
            if (workDate < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Không được phép phân ca trực trong quá khứ.");
            }

            // BR01: Check overlap
            var overlappingSchedules = _unitOfWork.DoctorSchedules.Query()
                .Where(s => s.DoctorId == dto.DoctorId 
                         && s.WorkDate == workDate 
                         && s.IsAvailable)
                .ToList();

            var isConflict = overlappingSchedules.Any(s => 
                dto.StartTime < s.EndTime && dto.EndTime > s.StartTime);

            if (isConflict)
            {
                throw new InvalidOperationException("Bác sĩ đã có ca trực trùng giờ trong ngày này.");
            }

            var schedule = new DoctorSchedule
            {
                DoctorId = dto.DoctorId,
                WorkDate = workDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MaxAppointments = dto.MaxAppointments,
                IsAvailable = true
            };

            await _unitOfWork.DoctorSchedules.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            return schedule.Id;
        }

        public async Task<bool> UpdateScheduleAsync(long id, DoctorScheduleUpdateDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
            {
                throw new InvalidOperationException("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            }

            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null) return false;

            if (schedule.WorkDate < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Không được phép cập nhật ca trực trong quá khứ.");
            }

            // Check overlap excluding this schedule
            var overlappingSchedules = _unitOfWork.DoctorSchedules.Query()
                .Where(s => s.DoctorId == schedule.DoctorId 
                         && s.WorkDate == schedule.WorkDate 
                         && s.Id != id
                         && s.IsAvailable)
                .ToList();

            var isConflict = overlappingSchedules.Any(s => 
                dto.StartTime < s.EndTime && dto.EndTime > s.StartTime);

            if (isConflict)
            {
                throw new InvalidOperationException("Thời gian cập nhật bị trùng với ca trực khác của bác sĩ.");
            }

            schedule.StartTime = dto.StartTime;
            schedule.EndTime = dto.EndTime;
            schedule.MaxAppointments = dto.MaxAppointments;
            schedule.IsAvailable = dto.IsAvailable;

            _unitOfWork.DoctorSchedules.Update(schedule);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteScheduleAsync(long id)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null) return false;

            if (schedule.WorkDate < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Không được phép xóa ca trực trong quá khứ.");
            }

            // BR04: Check if there are associated appointments
            var appointments = _unitOfWork.Appointments.Query()
                .Where(a => a.DoctorId == schedule.DoctorId 
                         && a.AppointmentDate == schedule.WorkDate
                         && a.Status != "cancelled"
                         && a.Status != "no_show")
                .ToList();

            // Check if any appointment time falls within this schedule
            var hasAssociatedAppointments = appointments.Any(a => 
                a.StartTime >= schedule.StartTime && a.StartTime <= schedule.EndTime);

            if (hasAssociatedAppointments)
            {
                throw new InvalidOperationException("Không thể xóa ca trực này vì đã có khách hàng đặt lịch khám. Vui lòng chuyển bác sĩ trước.");
            }

            _unitOfWork.DoctorSchedules.Remove(schedule);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
