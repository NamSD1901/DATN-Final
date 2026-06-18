using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
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
        public async Task<IActionResult> GetPublicPosts([FromQuery] string? search)
        {
            var result = await _postService.GetPublicPostsAsync(search);
            return Ok(result);
        }

        [HttpGet("posts/{slug}")]
        public async Task<IActionResult> GetPostBySlug(string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }

            return Ok(post);
        }

        // ================= ADMIN MANAGEMENT ENDPOINTS =================

        [Authorize(Roles = "admin,Admin")]
        [HttpGet("admin/posts")]
        public async Task<IActionResult> GetAdminPosts()
        {
            var result = await _postService.GetAdminPostsAsync();
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "admin,Admin")]
        [HttpPut("admin/posts/{id}")]
        public async Task<IActionResult> UpdatePost(long id, [FromBody] CreatePostDto dto)
        {
            try
            {
                var post = await _postService.UpdatePostAsync(id, dto);
                return Ok(new { success = true, post });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
