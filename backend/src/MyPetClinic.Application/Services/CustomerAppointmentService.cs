using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.Helpers;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Application.Services
{
    public class CustomerAppointmentService : ICustomerAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerAppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<object> GetAvailableVaccinesAsync()
        {
            var vaccines = await _unitOfWork.Vaccines.FindAsync(v => v.StockQuantity > 0);
            return vaccines.OrderBy(v => v.Name).Select(v => new
            {
                id = v.Id,
                name = v.Name,
                description = v.Description,
                targetSpecies = v.TargetSpecies
            });
        }

        public async Task<object> ValidateVaccineAsync(Guid customerId, long petId, long vaccineId, DateTime targetDate)
        {
            var pets = await _unitOfWork.Pets.FindAsync(p => p.Id == petId && p.OwnerId == customerId);
            var pet = pets.FirstOrDefault();
            if (pet == null)
                throw new InvalidOperationException("Thú cưng không hợp lệ hoặc không thuộc về bạn.");

            var vaccines = await _unitOfWork.Vaccines.FindAsync(v => v.Id == vaccineId);
            var vaccine = vaccines.FirstOrDefault();
            if (vaccine == null)
                throw new KeyNotFoundException("Không tìm thấy vắc-xin.");

            var lastRecords = await _unitOfWork.VaccinationRecords.FindAsync(vr => vr.PetId == petId && vr.VaccineId == vaccineId);
            var lastRecord = lastRecords.OrderByDescending(vr => vr.InjectionDate).FirstOrDefault();

            var checker = new VaccinationScheduleChecker();
            return checker.ValidateInterval(lastRecord, vaccine, targetDate, pet);
        }
    }
}
