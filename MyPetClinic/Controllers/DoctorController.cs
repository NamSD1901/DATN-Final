using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "Doctor,Admin")]
    public class DoctorController : Controller
    {
        private readonly IReceptionistService _receptionistService;

        public DoctorController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyQueue()
        {
            var queue = await _receptionistService.GetTodayQueueAsync();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdStr)) return Json(new List<object>());

            var myQueue = queue.Where(q => q.DoctorId.ToString().Equals(userIdStr, StringComparison.OrdinalIgnoreCase)).ToList();
            return Json(myQueue);
        }

        [HttpPost]
        public async Task<IActionResult> StartTreatment(long appointmentId)
        {
            // Tương đương với việc kéo thẻ sang cột "Đang khám"
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, "in_progress");
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> FinishTreatment(long appointmentId)
        {
            // Tương đương với việc kéo thẻ sang cột "Chờ thanh toán"
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, "ready_to_pay");
            return Json(new { success });
        }
    }
}
