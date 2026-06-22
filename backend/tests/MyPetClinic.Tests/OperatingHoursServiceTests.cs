using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using Xunit;

namespace MyPetClinic.Tests
{
    public class OperatingHoursServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OperatingHoursService _service;

        public OperatingHoursServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _service = new OperatingHoursService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task UpdateOperatingHours_ShouldThrow_When_StartTime_Greater_Than_EndTime()
        {
            // VR-01: Giờ bắt đầu phải nhỏ hơn giờ kết thúc trong cùng một ca.
            var dto = new UpdateOperatingHoursDto
            {
                Days = new List<ClinicOperatingDayDto>
                {
                    new ClinicOperatingDayDto
                    {
                        DayOfWeek = DayOfWeek.Monday,
                        IsOpen = true,
                        Shifts = new List<ClinicOperatingShiftDto>
                        {
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(9, 0, 0) }
                        }
                    }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateOperatingHoursAsync(dto));
            Assert.Contains("Giờ bắt đầu phải nhỏ hơn giờ kết thúc", ex.Message);
        }

        [Fact]
        public async Task UpdateOperatingHours_ShouldThrow_When_TimeRanges_Overlap()
        {
            // VR-02, VR-03: Không được trùng lấp hoặc chồng lấn thời gian
            var dto = new UpdateOperatingHoursDto
            {
                Days = new List<ClinicOperatingDayDto>
                {
                    new ClinicOperatingDayDto
                    {
                        DayOfWeek = DayOfWeek.Monday,
                        IsOpen = true,
                        Shifts = new List<ClinicOperatingShiftDto>
                        {
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(12, 0, 0) },
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(15, 0, 0) }
                        }
                    }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateOperatingHoursAsync(dto));
            Assert.Contains("Khoảng thời gian không được chồng lấp", ex.Message);
        }

        [Fact]
        public async Task UpdateOperatingHours_ShouldThrow_When_Shift_Duration_Less_Than_1_Hour()
        {
            // VR-07
            var dto = new UpdateOperatingHoursDto
            {
                Days = new List<ClinicOperatingDayDto>
                {
                    new ClinicOperatingDayDto
                    {
                        DayOfWeek = DayOfWeek.Monday,
                        IsOpen = true,
                        Shifts = new List<ClinicOperatingShiftDto>
                        {
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 30, 0) }
                        }
                    }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateOperatingHoursAsync(dto));
            Assert.Contains("Độ dài tối thiểu của một ca làm việc là 1 giờ", ex.Message);
        }

        [Fact]
        public async Task UpdateOperatingHours_ShouldThrow_When_Total_Duration_Exceeds_16_Hours()
        {
            // VR-08
            var dto = new UpdateOperatingHoursDto
            {
                Days = new List<ClinicOperatingDayDto>
                {
                    new ClinicOperatingDayDto
                    {
                        DayOfWeek = DayOfWeek.Monday,
                        IsOpen = true,
                        Shifts = new List<ClinicOperatingShiftDto>
                        {
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(10, 0, 0) },
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(20, 0, 0) }
                        }
                    }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateOperatingHoursAsync(dto));
            Assert.Contains("vượt quá 16 giờ", ex.Message);
        }

        [Fact]
        public async Task UpdateOperatingHours_ShouldThrow_When_Gap_Between_Shifts_Less_Than_30_Mins()
        {
            // VR-09
            var dto = new UpdateOperatingHoursDto
            {
                Days = new List<ClinicOperatingDayDto>
                {
                    new ClinicOperatingDayDto
                    {
                        DayOfWeek = DayOfWeek.Monday,
                        IsOpen = true,
                        Shifts = new List<ClinicOperatingShiftDto>
                        {
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(12, 0, 0) },
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(12, 10, 0), EndTime = new TimeSpan(16, 0, 0) }
                        }
                    }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateOperatingHoursAsync(dto));
            Assert.Contains("Khoảng cách nghỉ giữa 2 ca tối thiểu phải là 30 phút", ex.Message);
        }

        [Fact]
        public async Task UpdateOperatingHours_ShouldThrow_When_More_Than_3_Shifts_Per_Day()
        {
            // VR-17
            var dto = new UpdateOperatingHoursDto
            {
                Days = new List<ClinicOperatingDayDto>
                {
                    new ClinicOperatingDayDto
                    {
                        DayOfWeek = DayOfWeek.Monday,
                        IsOpen = true,
                        Shifts = new List<ClinicOperatingShiftDto>
                        {
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(10, 0, 0) },
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(10, 30, 0), EndTime = new TimeSpan(12, 0, 0) },
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(12, 30, 0), EndTime = new TimeSpan(14, 0, 0) },
                            new ClinicOperatingShiftDto { StartTime = new TimeSpan(14, 30, 0), EndTime = new TimeSpan(16, 0, 0) }
                        }
                    }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateOperatingHoursAsync(dto));
            Assert.Contains("Tối đa 3 ca làm việc trong 1 ngày", ex.Message);
        }

        [Fact]
        public async Task UpdateOperatingHours_ShouldThrow_When_Conflicts_With_Existing_Appointments()
        {
            var apptRepoMock = new Mock<IGenericRepository<Appointment>>();
            
            // Mock appointments where one is scheduled for Monday 10:00 - 11:00
            apptRepoMock.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .ReturnsAsync(new List<Appointment>
                {
                    new Appointment
                    {
                        Id = 1,
                        AppointmentDate = new DateTime(2026, 6, 22), // Monday
                        StartTime = new TimeSpan(10, 0, 0),
                        EndTime = new TimeSpan(11, 0, 0),
                        Status = "confirmed"
                    }
                });

            _unitOfWorkMock.Setup(uow => uow.Appointments).Returns(apptRepoMock.Object);

            // Try to close Monday
            var dto = new UpdateOperatingHoursDto
            {
                Days = new List<ClinicOperatingDayDto>
                {
                    new ClinicOperatingDayDto
                    {
                        DayOfWeek = DayOfWeek.Monday,
                        IsOpen = false
                    }
                }
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateOperatingHoursAsync(dto));
            Assert.Contains("Conflict: Có lịch hẹn", ex.Message);
        }
    }
}
