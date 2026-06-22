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
    public class OperatingHoursService : IOperatingHoursService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OperatingHoursService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ClinicOperatingDayDto>> GetOperatingHoursAsync()
        {
            var entities = await _unitOfWork.ClinicOperatingDays.FindWithIncludesAsync(
                d => true,
                d => d.Shifts!
            );

            // Default fallback if empty
            if (!entities.Any())
            {
                var defaults = new List<ClinicOperatingDayDto>();
                for (int i = 0; i < 7; i++)
                {
                    var day = (DayOfWeek)i;
                    var dto = new ClinicOperatingDayDto
                    {
                        DayOfWeek = day,
                        IsOpen = day != DayOfWeek.Sunday
                    };
                    if (dto.IsOpen)
                    {
                        dto.Shifts.Add(new ClinicOperatingShiftDto { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(12, 0, 0) });
                        dto.Shifts.Add(new ClinicOperatingShiftDto { StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(17, 0, 0) });
                    }
                    defaults.Add(dto);
                }
                return defaults;
            }

            return entities.OrderBy(e => e.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)e.DayOfWeek).Select(e => new ClinicOperatingDayDto
            {
                DayOfWeek = e.DayOfWeek,
                IsOpen = e.IsOpen,
                Shifts = e.Shifts.OrderBy(s => s.StartTime).Select(s => new ClinicOperatingShiftDto
                {
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList()
            });
        }

        public async Task<bool> UpdateOperatingHoursAsync(UpdateOperatingHoursDto dto)
        {
            // Validations
            foreach (var day in dto.Days)
            {
                if (day.IsOpen)
                {
                    if (day.Shifts == null || !day.Shifts.Any())
                    {
                        throw new InvalidOperationException($"Ngày {day.DayOfWeek} được mở nhưng không có ca làm việc nào.");
                    }

                    if (day.Shifts.Count > 3)
                    {
                        throw new InvalidOperationException($"Tối đa 3 ca làm việc trong 1 ngày ({day.DayOfWeek})."); // VR-17
                    }

                    TimeSpan totalDuration = TimeSpan.Zero;
                    var sortedShifts = day.Shifts.OrderBy(s => s.StartTime).ToList();

                    for (int i = 0; i < sortedShifts.Count; i++)
                    {
                        var shift = sortedShifts[i];

                        if (shift.StartTime >= shift.EndTime)
                        {
                            throw new InvalidOperationException($"Giờ bắt đầu phải nhỏ hơn giờ kết thúc ({day.DayOfWeek})."); // VR-01
                        }

                        var duration = shift.EndTime - shift.StartTime;
                        if (duration.TotalHours < 1)
                        {
                            throw new InvalidOperationException($"Độ dài tối thiểu của một ca làm việc là 1 giờ ({day.DayOfWeek})."); // VR-07
                        }

                        totalDuration += duration;

                        if (i > 0)
                        {
                            var prevShift = sortedShifts[i - 1];
                            if (shift.StartTime < prevShift.EndTime)
                            {
                                throw new InvalidOperationException($"Khoảng thời gian không được chồng lấp ({day.DayOfWeek})."); // VR-02, VR-03
                            }

                            if ((shift.StartTime - prevShift.EndTime).TotalMinutes < 30)
                            {
                                throw new InvalidOperationException($"Khoảng cách nghỉ giữa 2 ca tối thiểu phải là 30 phút ({day.DayOfWeek})."); // VR-09
                            }
                        }
                    }

                    if (totalDuration.TotalHours > 16)
                    {
                        throw new InvalidOperationException($"Tổng thời gian hoạt động trong ngày không vượt quá 16 giờ ({day.DayOfWeek})."); // VR-08
                    }
                }
            }

            // Xử lý Business Rule (Blocker): Check conflict với Lịch hẹn
            // Vì cấu hình áp dụng cho tương lai, kiểm tra xem có Appointments nào bị ảnh hưởng không
            var upcomingAppointments = await _unitOfWork.Appointments.FindAsync(
                a => a.AppointmentDate >= DateTime.UtcNow.Date && a.Status != "cancelled" && a.Status != "completed");

            foreach (var appt in upcomingAppointments)
            {
                var dayConfig = dto.Days.FirstOrDefault(d => d.DayOfWeek == appt.AppointmentDate.DayOfWeek);
                if (dayConfig != null)
                {
                    if (!dayConfig.IsOpen)
                    {
                        // Ngày đó bị đóng cửa
                        throw new InvalidOperationException($"Conflict: Có lịch hẹn (Mã: {appt.Id}) vào ngày {appt.AppointmentDate:dd/MM/yyyy} nhưng bạn đang muốn đóng cửa ngày này.");
                    }
                    
                    var apptEnd = appt.EndTime ?? appt.StartTime.Add(TimeSpan.FromMinutes(30));
                    bool isInShift = dayConfig.Shifts.Any(s => s.StartTime <= appt.StartTime && s.EndTime >= apptEnd);
                    if (!isInShift)
                    {
                        throw new InvalidOperationException($"Conflict: Có lịch hẹn (Mã: {appt.Id}) lúc {appt.StartTime} ngày {appt.AppointmentDate:dd/MM/yyyy} không nằm trong khung giờ mới cấu hình.");
                    }
                }
            }

            // Clear old config and insert new
            var oldDays = await _unitOfWork.ClinicOperatingDays.GetAllAsync();
            foreach (var oldDay in oldDays)
            {
                var oldShifts = await _unitOfWork.ClinicOperatingShifts.FindAsync(s => s.ClinicOperatingDayId == oldDay.Id);
                foreach (var shift in oldShifts)
                {
                    _unitOfWork.ClinicOperatingShifts.Remove(shift);
                }
                _unitOfWork.ClinicOperatingDays.Remove(oldDay);
            }

            foreach (var d in dto.Days)
            {
                var dayEntity = new ClinicOperatingDay
                {
                    DayOfWeek = d.DayOfWeek,
                    IsOpen = d.IsOpen
                };
                await _unitOfWork.ClinicOperatingDays.AddAsync(dayEntity);
                
                if (d.IsOpen)
                {
                    foreach (var s in d.Shifts)
                    {
                        dayEntity.Shifts.Add(new ClinicOperatingShift
                        {
                            StartTime = s.StartTime,
                            EndTime = s.EndTime
                        });
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ClinicHolidayDto>> GetHolidaysAsync()
        {
            var entities = await _unitOfWork.ClinicHolidays.GetAllAsync();
            return entities.OrderBy(e => e.StartDate).Select(e => new ClinicHolidayDto
            {
                Id = e.Id,
                Name = e.Name,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsActive = e.IsActive
            });
        }

        public async Task<ClinicHolidayDto> CreateHolidayAsync(CreateHolidayDto dto, Guid createdBy)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Tên ngày nghỉ không được trống."); // VR-10
            
            if (dto.EndDate < dto.StartDate)
                throw new InvalidOperationException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu."); // VR-14

            if (dto.StartDate.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException("Không thể cấu hình ngày nghỉ trong quá khứ."); // VR-13

            // Conflict Check
            var upcomingAppointments = await _unitOfWork.Appointments.FindAsync(
                a => a.AppointmentDate >= dto.StartDate.Date && a.AppointmentDate <= dto.EndDate.Date && a.Status != "cancelled" && a.Status != "completed");

            if (upcomingAppointments.Any())
            {
                throw new InvalidOperationException($"Conflict: Đang có {upcomingAppointments.Count()} lịch hẹn nằm trong khoảng ngày nghỉ này. Yêu cầu dời lịch trước khi thêm.");
            }

            var entity = new ClinicHoliday
            {
                Name = dto.Name,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ClinicHolidays.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return new ClinicHolidayDto
            {
                Id = entity.Id,
                Name = entity.Name,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsActive = entity.IsActive
            };
        }

        public async Task<bool> UpdateHolidayAsync(int id, UpdateHolidayDto dto)
        {
            var entities = await _unitOfWork.ClinicHolidays.FindAsync(h => h.Id == id);
            var entity = entities.FirstOrDefault();
            if (entity == null) return false;

            if (dto.EndDate < dto.StartDate)
                throw new InvalidOperationException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            // Conflict Check only if IsActive is true
            if (dto.IsActive)
            {
                var upcomingAppointments = await _unitOfWork.Appointments.FindAsync(
                    a => a.AppointmentDate >= dto.StartDate.Date && a.AppointmentDate <= dto.EndDate.Date && a.Status != "cancelled" && a.Status != "completed");

                if (upcomingAppointments.Any())
                {
                    throw new InvalidOperationException($"Conflict: Đang có {upcomingAppointments.Count()} lịch hẹn nằm trong khoảng ngày nghỉ này. Yêu cầu dời lịch trước khi cập nhật.");
                }
            }

            entity.Name = dto.Name;
            entity.StartDate = dto.StartDate.Date;
            entity.EndDate = dto.EndDate.Date;
            entity.IsActive = dto.IsActive;

            _unitOfWork.ClinicHolidays.Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteHolidayAsync(int id)
        {
            var entities = await _unitOfWork.ClinicHolidays.FindAsync(h => h.Id == id);
            var entity = entities.FirstOrDefault();
            if (entity == null) return false;

            _unitOfWork.ClinicHolidays.Remove(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
