using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        // ================= PUBLIC ENDPOINTS =================

        [HttpGet("posts")]
        public async Task<IActionResult> GetPublicPosts([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? categorySlug = null)
        {
            var result = await _postService.GetPublicPostsAsync(pageIndex, pageSize, search, categorySlug);
            return Ok(result);
        }

        [HttpGet("posts/{slug}")]
        public async Task<IActionResult> GetPostBySlug(string slug)
        {
            try
            {
                var post = await _postService.GetPostBySlugAsync(slug);
                return Ok(post);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        [HttpPost("posts/{id}/view")]
        public async Task<IActionResult> IncrementViewCount(long id)
        {
            await _postService.IncrementViewCountAsync(id);
            return Ok();
        }

        [HttpGet("posts/{id}/related")]
        public async Task<IActionResult> GetRelatedPosts(long id, [FromQuery] int count = 3)
        {
            var result = await _postService.GetRelatedPostsAsync(id, count);
            return Ok(result);
        }

        // ================= ADMIN MANAGEMENT ENDPOINTS =================

        [Authorize(Roles = "admin,Admin")]
        [HttpGet("admin/posts")]
        public async Task<IActionResult> GetAdminPosts([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] long? categoryId = null)
        {
            var result = await _postService.GetAdminPostsAsync(pageIndex, pageSize, search, status, categoryId);
            return Ok(result);
        }

        [Authorize(Roles = "admin,Admin")]
        [HttpPost("admin/posts")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            try
            {
                var authorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? authorId = null;
                if (Guid.TryParse(authorIdStr, out var authorGuid))
                {
                    authorId = authorGuid;
                }

                var post = await _postService.CreatePostAsync(dto, authorId);
                return Ok(new { success = true, post });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "admin,Admin")]
        [HttpPut("admin/posts/{id}")]
        public async Task<IActionResult> UpdatePost(long id, [FromBody] UpdatePostDto dto)
        {
            try
            {
                var updaterIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? updaterId = null;
                if (Guid.TryParse(updaterIdStr, out var updaterGuid))
                {
                    updaterId = updaterGuid;
                }

                var post = await _postService.UpdatePostAsync(id, dto, updaterId);
                return Ok(new { success = true, post });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "admin,Admin")]
        [HttpDelete("admin/posts/{id}")]
        public async Task<IActionResult> DeletePost(long id)
        {
            try
            {
                await _postService.DeletePostAsync(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
