using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _unitOfWork.Users.FindWithIncludesAsync(
                u => u.Role != null && u.Role.Name.ToLower() == "doctor" && u.IsActive == true,
                u => u.Role!
            );

            var result = doctors.Select(d => new
            {
                Id = d.Id,
                FullName = d.FullName,
                Email = d.Email,
                Phone = d.Phone,
                Avatar = d.Avatar
            });

            return Ok(result);
        }
    }
}
