using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace WebApi.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { status = "API is running", timestamp = System.DateTime.UtcNow });
        }
    }
