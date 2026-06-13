using MyPetClinic.Application.Helpers;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;

namespace MyPetClinic.Application.Helpers
{
    public class VaccinationValidationResult
    {
        public bool IsValid { get; set; }
        public string? WarningMessage { get; set; }
        public bool RequiresDoctorOverride { get; set; }
        public DateTime? NextAvailableDate { get; set; }
    }

    public class VaccinationScheduleChecker : IVaccinationScheduleChecker
    {
        public VaccinationValidationResult ValidateInterval(
            VaccinationRecord? lastRecord,
            Vaccine vaccine,
            DateTime targetDate,
            Pet pet)
        {
            var result = new VaccinationValidationResult { IsValid = true };

            // 1. So khớp loài (Species match)
            if (!string.IsNullOrEmpty(vaccine.TargetSpecies) && !vaccine.TargetSpecies.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var target = vaccine.TargetSpecies.ToLower();
                var petSpecies = pet.Species?.ToLower() ?? "";
                if (!petSpecies.Contains(target) && !target.Contains(petSpecies))
                {
                    result.IsValid = false;
                    result.WarningMessage = $"Vắc-xin {vaccine.Name} chỉ dùng cho {vaccine.TargetSpecies}, thú cưng của bạn là {pet.Species}.";
                    return result;
                }
            }

            // 2. Kiểm tra độ tuổi tối thiểu (Min age check)
            if (vaccine.MinAgeWeeks.HasValue && pet.BirthDate.HasValue)
            {
                var ageInDays = (targetDate.Date - pet.BirthDate.Value.Date).TotalDays;
                var ageInWeeks = ageInDays / 7.0;
                if (ageInWeeks < vaccine.MinAgeWeeks.Value)
                {
                    result.IsValid = false;
                    result.WarningMessage = $"Thú cưng chưa đủ độ tuổi tối thiểu để tiêm vắc-xin {vaccine.Name} (Tuổi hiện tại tại thời điểm hẹn: {Math.Round(ageInWeeks, 1)} tuần, Yêu cầu: {vaccine.MinAgeWeeks.Value} tuần).";
                    return result;
                }
            }

            // 3. Kiểm tra khoảng cách mũi tiêm (Interval check)
            if (lastRecord != null && vaccine.IntervalDays.HasValue)
            {
                var daysSinceLast = (targetDate.Date - lastRecord.InjectionDate.Date).TotalDays;
                if (daysSinceLast < vaccine.IntervalDays.Value)
                {
                    result.IsValid = false;
                    result.RequiresDoctorOverride = true;
                    result.NextAvailableDate = lastRecord.InjectionDate.AddDays(vaccine.IntervalDays.Value);
                    result.WarningMessage = $"Lịch hẹn vi phạm phác đồ tiêm chủng: Mũi gần nhất của bé được tiêm vào ngày {lastRecord.InjectionDate:dd/MM/yyyy}. Mũi tiếp theo tối thiểu cách {vaccine.IntervalDays.Value} ngày (Khuyên dùng từ ngày {result.NextAvailableDate:dd/MM/yyyy}).";
                }
            }

            return result;
        }
    }
}
