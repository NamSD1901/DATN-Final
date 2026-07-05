using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IUserRepository _userRepository;

        public ReviewsController(IReviewService reviewService, IUserRepository userRepository)
        {
            _reviewService = reviewService;
            _userRepository = userRepository;
        }

        private async Task<Guid> GetCurrentCustomerIdAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user != null && user.CustomerId.HasValue)
                {
                    return user.CustomerId.Value;
                }
            }
            throw new UnauthorizedAccessException("Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập với tài khoản khách hàng.");
        }

        /// <summary>
        /// Lấy danh sách đánh giá (công khai).
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? sortBy = null, [FromQuery] short? rating = null)
        {
            try
            {
                // Guest/Public chỉ thấy đánh giá chưa bị xóa mềm
                var result = await _reviewService.GetReviewsAsync(page, limit, sortBy, rating, includeDeleted: false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết một đánh giá.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetReviewById(long id)
        {
            try
            {
                var result = await _reviewService.GetReviewByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Admin xem danh sách đánh giá (bao gồm cả bị xóa mềm).
        /// </summary>
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllReviewsAdmin([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? sortBy = null, [FromQuery] short? rating = null)
        {
            try
            {
                var result = await _reviewService.GetReviewsAsync(page, limit, sortBy, rating, includeDeleted: true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Thống kê đánh giá dành cho Admin/Manager.
        /// </summary>
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _reviewService.GetReviewStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xem danh sách đánh giá của chính khách hàng.
        /// </summary>
        [Authorize(Roles = "customer")]
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var result = await _reviewService.GetMyReviewsAsync(customerId, page, limit);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Khách hàng thêm mới đánh giá.
        /// </summary>
        [Authorize(Roles = "customer")]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var result = await _reviewService.CreateReviewAsync(customerId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Khách hàng sửa đánh giá của chính mình.
        /// </summary>
        [Authorize(Roles = "customer")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateReview(long id, [FromBody] UpdateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var result = await _reviewService.UpdateReviewAsync(customerId, id, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Admin ẩn (soft delete) một đánh giá vi phạm.
        /// </summary>
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> SoftDeleteReview(long id)
        {
            try
            {
                await _reviewService.SoftDeleteReviewAsync(id);
                return Ok(new { message = "Ẩn đánh giá thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Admin khôi phục một đánh giá đã bị ẩn.
        /// </summary>
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        [HttpPatch("{id:long}/restore")]
        public async Task<IActionResult> RestoreReview(long id)
        {
            try
            {
                await _reviewService.RestoreReviewAsync(id);
                return Ok(new { message = "Khôi phục đánh giá thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
