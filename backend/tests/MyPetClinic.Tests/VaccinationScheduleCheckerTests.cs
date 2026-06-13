using MyPetClinic.Application.Helpers;
using MyPetClinic.Domain.Entities;
using System;
using Xunit;

namespace MyPetClinic.Tests
{
    public class VaccinationScheduleCheckerTests
    {
        private readonly VaccinationScheduleChecker _checker = new VaccinationScheduleChecker();

        [Fact]
        public void ValidateInterval_ShouldReturnValid_WhenNoConstraintsViolated()
        {
            // Arrange
            var pet = new Pet { Species = "Chó", BirthDate = DateTime.Today.AddYears(-1) };
            var vaccine = new Vaccine
            {
                Name = "Nobivac DHPPi",
                TargetSpecies = "Chó",
                MinAgeWeeks = 6,
                IntervalDays = 21
            };
            var targetDate = DateTime.Today;

            // Act
            var result = _checker.ValidateInterval(null, vaccine, targetDate, pet);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.WarningMessage);
        }

        [Fact]
        public void ValidateInterval_ShouldFail_WhenSpeciesMismatch()
        {
            // Arrange
            var pet = new Pet { Species = "Mèo", BirthDate = DateTime.Today.AddYears(-1) };
            var vaccine = new Vaccine
            {
                Name = "Nobivac DHPPi",
                TargetSpecies = "Chó",
                MinAgeWeeks = 6,
                IntervalDays = 21
            };
            var targetDate = DateTime.Today;

            // Act
            var result = _checker.ValidateInterval(null, vaccine, targetDate, pet);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("chỉ dùng cho Chó", result.WarningMessage);
            Assert.False(result.RequiresDoctorOverride);
        }

        [Fact]
        public void ValidateInterval_ShouldFail_WhenPetTooYoung()
        {
            // Arrange
            var birthDate = DateTime.Today.AddDays(-28); // 4 weeks old
            var pet = new Pet { Species = "Chó", BirthDate = birthDate };
            var vaccine = new Vaccine
            {
                Name = "Nobivac DHPPi",
                TargetSpecies = "Chó",
                MinAgeWeeks = 6,
                IntervalDays = 21
            };
            var targetDate = DateTime.Today;

            // Act
            var result = _checker.ValidateInterval(null, vaccine, targetDate, pet);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("chưa đủ độ tuổi tối thiểu", result.WarningMessage);
            Assert.False(result.RequiresDoctorOverride);
        }

        [Fact]
        public void ValidateInterval_ShouldWarn_WhenIntervalTooShort()
        {
            // Arrange
            var pet = new Pet { Species = "Chó", BirthDate = DateTime.Today.AddYears(-1) };
            var vaccine = new Vaccine
            {
                Name = "Nobivac DHPPi",
                TargetSpecies = "Chó",
                MinAgeWeeks = 6,
                IntervalDays = 21
            };
            var lastRecord = new VaccinationRecord
            {
                VaccineId = 1,
                InjectionDate = DateTime.Today.AddDays(-10) // 10 days ago, which is less than 21 days
            };
            var targetDate = DateTime.Today;

            // Act
            var result = _checker.ValidateInterval(lastRecord, vaccine, targetDate, pet);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.RequiresDoctorOverride);
            Assert.Contains("vi phạm phác đồ tiêm chủng", result.WarningMessage);
            Assert.Equal(lastRecord.InjectionDate.AddDays(21), result.NextAvailableDate);
        }
    }
}
