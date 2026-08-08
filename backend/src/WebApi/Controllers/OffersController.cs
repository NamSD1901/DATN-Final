using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs.Offer;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MyPetClinic.Application.Interfaces.Repositories;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly IUserRepository _userRepository;

        public OffersController(IOfferService offerService, IUserRepository userRepository)
        {
            _offerService = offerService;
            _userRepository = userRepository;
        }

        // ---------- ADMIN & RECEPTIONIST ENDPOINTS ----------

        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetOffers([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? status = null, [FromQuery] string? search = null)
        {
            var result = await _offerService.GetOffersAsync(page, limit, status, search);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetOfferById(Guid id)
        {
            var result = await _offerService.GetOfferByIdAsync(id);
            return Ok(new { success = true, data = result });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateOffer([FromBody] CreateOfferDto dto)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdString, out Guid currentUserId))
                {
                    return Unauthorized(new { success = false, message = "Người dùng không hợp lệ." });
                }

                var result = await _offerService.CreateOfferAsync(dto, currentUserId);
                return Ok(new { success = true, data = result, message = "Tạo mã giảm giá thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOffer(Guid id, [FromBody] UpdateOfferDto dto)
        {
            try
            {
                var result = await _offerService.UpdateOfferAsync(id, dto);
                return Ok(new { success = true, data = result, message = "Cập nhật mã giảm giá thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPatch("{id}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockOffer(Guid id)
        {
            await _offerService.LockOfferAsync(id);
            return Ok(new { success = true, message = "Đã khóa mã giảm giá." });
        }

        // ---------- CUSTOMER ENDPOINTS ----------

        [HttpGet("public")]
        public async Task<IActionResult> GetPublicOffers([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] Guid? customerId = null)
        {
            if (customerId == null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idString = User.FindFirstValue("CustomerId");
                if (Guid.TryParse(idString, out Guid parsedId))
                {
                    customerId = parsedId;
                }
                else
                {
                    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (Guid.TryParse(userIdString, out Guid userId))
                    {
                        var user = await _userRepository.GetUserByIdAsync(userId);
                        if (user != null && user.CustomerId.HasValue)
                        {
                            customerId = user.CustomerId.Value;
                        }
                    }
                }
            }

            var result = await _offerService.GetPublicOffersAsync(page, limit, customerId);
            return Ok(new { success = true, data = result });
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateOffer([FromBody] ValidateOfferRequestDto request)
        {
            Guid? customerId = request.CustomerId;
            if (customerId == null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idString = User.FindFirstValue("CustomerId"); // Assuming CustomerId claim exists
                if (Guid.TryParse(idString, out Guid parsedId))
                {
                    customerId = parsedId;
                }
                else
                {
                    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (Guid.TryParse(userIdString, out Guid userId))
                    {
                        var user = await _userRepository.GetUserByIdAsync(userId);
                        if (user != null && user.CustomerId.HasValue)
                        {
                            customerId = user.CustomerId.Value;
                        }
                    }
                }
            }

            var result = await _offerService.ValidateOfferAsync(request, customerId);
            
            if (!result.IsValid)
            {
                return BadRequest(new { success = false, message = result.Message, data = result });
            }

            return Ok(new { success = true, data = result, message = result.Message });
        }
    }
}
