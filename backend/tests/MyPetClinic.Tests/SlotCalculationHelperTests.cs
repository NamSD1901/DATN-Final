using MyPetClinic.Application.Helpers;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace MyPetClinic.Tests
{
    public class SlotCalculationHelperTests
    {
        [Fact]
        public void GenerateSlots_ShouldReturnCorrectSlots_ForStandardShift()
        {
            // Arrange
            var workDate = new DateTime(2026, 6, 15);
            var startTime = new TimeSpan(8, 0, 0);       // 08:00
            var endTime = new TimeSpan(12, 0, 0);       // 12:00
            int duration = 30;

            // Act
            var slots = SlotCalculationHelper.GenerateSlots(workDate, startTime, endTime, duration);

            // Assert
            // 8:00, 8:30, 9:00, 9:30, 10:00, 10:30, 11:00, 11:30 (8 slots)
            Assert.Equal(8, slots.Count);
            Assert.Equal(workDate.AddHours(8), slots[0]);
            Assert.Equal(workDate.AddHours(11).AddMinutes(30), slots[7]);
        }

        [Fact]
        public void GetAvailableSlots_ShouldReturnAllSlots_WhenNoAppointmentsExist()
        {
            // Arrange
            var schedule = new DoctorSchedule
            {
                WorkDate = new DateTime(2026, 6, 15),
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(10, 0, 0), // 4 slots: 8:00, 8:30, 9:00, 9:30
                IsAvailable = true
            };

            var appointments = new List<Appointment>();

            // Act
            var availableSlots = SlotCalculationHelper.GetAvailableSlots(schedule, appointments, null, 30);

            // Assert
            Assert.Equal(4, availableSlots.Count);
        }

        [Fact]
        public void GetAvailableSlots_ShouldRemoveBusySlots_WhenMatchingAppointmentsExist()
        {
            // Arrange
            var workDate = new DateTime(2026, 6, 15);
            var schedule = new DoctorSchedule
            {
                WorkDate = workDate,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(10, 0, 0), // Slots: 8:00, 8:30, 9:00, 9:30
                IsAvailable = true
            };

            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentDate = workDate, StartTime = new TimeSpan(8, 30, 0), Status = "confirmed" } // Busy at 8:30
            };

            // Act
            var availableSlots = SlotCalculationHelper.GetAvailableSlots(schedule, appointments, null, 30);

            // Assert
            // Remaining: 8:00, 9:00, 9:30 (8:30 is removed)
            Assert.Equal(3, availableSlots.Count);
            Assert.Contains(workDate.AddHours(8), availableSlots);
            Assert.DoesNotContain(workDate.AddHours(8).AddMinutes(30), availableSlots);
            Assert.Contains(workDate.AddHours(9), availableSlots);
        }

        [Fact]
        public void GetAvailableSlots_ShouldExcludeCancelledAppointments()
        {
            // Arrange
            var workDate = new DateTime(2026, 6, 15);
            var schedule = new DoctorSchedule
            {
                WorkDate = workDate,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(10, 0, 0), // Slots: 8:00, 8:30, 9:00, 9:30
                IsAvailable = true
            };

            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentDate = workDate, StartTime = new TimeSpan(8, 30, 0), Status = "cancelled" } // Cancelled
            };

            // Act
            var availableSlots = SlotCalculationHelper.GetAvailableSlots(schedule, appointments, null, 30);

            // Assert
            // All 4 slots should be available because the appointment is cancelled
            Assert.Equal(4, availableSlots.Count);
        }

        [Fact]
        public void GetAvailableSlots_ShouldBlockSlots_WithConflictUnderThreshold()
        {
            // Arrange
            var workDate = new DateTime(2026, 6, 15);
            var schedule = new DoctorSchedule
            {
                WorkDate = workDate,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(10, 0, 0), // Slots: 8:00, 8:30, 9:00, 9:30
                IsAvailable = true
            };

            // Appointment is at 8:45, which is less than 30 minutes from 8:30 (diff = 15m) and 9:00 (diff = 15m)
            var appointments = new List<Appointment>
            {
                new Appointment { AppointmentDate = workDate, StartTime = new TimeSpan(8, 45, 0), Status = "pending" }
            };

            // Act
            var availableSlots = SlotCalculationHelper.GetAvailableSlots(schedule, appointments, null, 30);

            // Assert
            // 8:00 (diff = 45m -> Ok)
            // 8:30 (diff = 15m -> Blocked)
            // 9:00 (diff = 15m -> Blocked)
            // 9:30 (diff = 45m -> Ok)
            Assert.Equal(2, availableSlots.Count);
            Assert.Contains(workDate.AddHours(8), availableSlots);
            Assert.DoesNotContain(workDate.AddHours(8).AddMinutes(30), availableSlots);
            Assert.DoesNotContain(workDate.AddHours(9), availableSlots);
            Assert.Contains(workDate.AddHours(9).AddMinutes(30), availableSlots);
        }
    }
}
