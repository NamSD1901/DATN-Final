using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IReceptionistService _receptionistService;

        public QueueController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }

        /// <summary>
        /// API công khai lấy thông tin hàng chờ sảnh chính cho TV Queue Board.
        /// </summary>
        [HttpGet("lobby-board")]
        public async Task<IActionResult> GetLobbyBoard()
        {
            try
            {
                var queue = await _receptionistService.GetTodayQueueAsync();
                
                var waiting = queue
                    .Where(q => q.Status == "waiting")
                    .Select(q => new
                    {
                        appointmentId = q.AppointmentId,
                        queueNumber = $"Q-{q.QueueNumber:D3}",
                        petName = q.PetName ?? "Thú cưng",
                        doctorName = q.DoctorName ?? "Bác sĩ trực",
                        status = "Waiting"
                    });

                var serving = queue
                    .Where(q => q.Status == "in_progress")
                    .Select(q => new
                    {
                        appointmentId = q.AppointmentId,
                        queueNumber = $"Q-{q.QueueNumber:D3}",
                        petName = q.PetName ?? "Thú cưng",
                        doctorName = q.DoctorName ?? "Bác sĩ trực",
                        status = "Serving"
                    });

                return Ok(new { waiting, serving });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
