using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DoctorDto>> GetDoctorsAsync()
        {
            var doctors = await _unitOfWork.Users.FindWithIncludesAsync(
                u => u.Role != null && (u.Role.Name.ToLower() == "clinical_doctor" || u.Role.Name.ToLower() == "vaccination_doctor") && u.IsActive == true,
                u => u.Role!
            );

            return doctors.Select(d => new DoctorDto
            {
                Id = d.Id,
                FullName = d.FullName ?? string.Empty,
                Email = d.Email,
                Phone = d.Phone,
                Avatar = d.Avatar
            });
        }
    }
}
