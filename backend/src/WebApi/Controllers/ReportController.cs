using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize(Roles = "admin,Admin")]
    [ApiController]
    [Route("api/admin/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] string startDate, [FromQuery] string endDate)
        {
            if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
            {
                return BadRequest(new { message = "Định dạng ngày bắt đầu hoặc ngày kết thúc không hợp lệ." });
            }

            if (start > end)
            {
                return BadRequest(new { message = "Ngày bắt đầu không thể sau ngày kết thúc." });
            }

            var report = await _reportService.GetRevenueReportAsync(start, end);
            return Ok(report);
        }
    }
}
