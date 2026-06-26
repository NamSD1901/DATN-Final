using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var result = await _doctorService.GetDoctorsAsync();
            return Ok(result);
        }
    }
}
