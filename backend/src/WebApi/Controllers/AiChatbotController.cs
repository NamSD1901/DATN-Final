using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiChatbotController : ControllerBase
    {
        private readonly IAiChatbotService _chatbotService;

        public AiChatbotController(IAiChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                var response = await _chatbotService.ChatAsync(request.Message);
                return Ok(new { reply = response });
            }
            catch (System.Exception ex)
            {
                // Return a JSON response with the error so the frontend fetch doesn't fail parsing HTML
                return StatusCode(500, new { reply = "Đã xảy ra lỗi từ server: " + ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
