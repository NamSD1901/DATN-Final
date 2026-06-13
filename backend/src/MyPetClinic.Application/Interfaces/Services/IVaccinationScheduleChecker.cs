using MyPetClinic.Application.Helpers;
using MyPetClinic.Domain.Entities;
using System;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IVaccinationScheduleChecker
    {
        VaccinationValidationResult ValidateInterval(
            VaccinationRecord? lastRecord,
            Vaccine vaccine,
            DateTime targetDate,
            Pet pet);
    }
}
