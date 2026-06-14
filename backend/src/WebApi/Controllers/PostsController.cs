using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [ApiController]
    [Route("api")]
    public class PostsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PostsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= PUBLIC ENDPOINTS =================

        [HttpGet("posts")]
        public async Task<IActionResult> GetPublicPosts([FromQuery] string? search)
        {
            var posts = await _unitOfWork.Posts.FindWithIncludesAsync(
                p => p.Status.ToLower() == "published",
                p => p.Author!
            );

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                posts = posts.Where(p => p.Title.ToLower().Contains(lowerSearch) || (p.Content != null && p.Content.ToLower().Contains(lowerSearch)));
            }

            var result = posts.OrderByDescending(p => p.CreatedAt).Select(p => new
            {
                id = p.Id,
                title = p.Title,
                slug = p.Slug,
                thumbnail = p.Thumbnail ?? "https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop",
                content = p.Content,
                status = p.Status,
                createdAt = p.CreatedAt,
                authorName = p.Author?.FullName ?? "Bác sĩ thú y"
            });

            return Ok(result);
        }

        [HttpGet("posts/{slug}")]
        public async Task<IActionResult> GetPostBySlug(string slug)
        {
            var posts = await _unitOfWork.Posts.FindWithIncludesAsync(
                p => p.Slug == slug && p.Status.ToLower() == "published",
                p => p.Author!
            );

            var post = posts.FirstOrDefault();
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }

            return Ok(new
            {
                id = post.Id,
                title = post.Title,
                slug = post.Slug,
                thumbnail = post.Thumbnail ?? "https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop",
                content = post.Content,
                status = post.Status,
                createdAt = post.CreatedAt,
                authorName = post.Author?.FullName ?? "Bác sĩ thú y"
            });
        }

        // ================= ADMIN MANAGEMENT ENDPOINTS =================

        [Authorize(Roles = "admin,Admin")]
        [HttpGet("admin/posts")]
        public async Task<IActionResult> GetAdminPosts()
        {
            var posts = await _unitOfWork.Posts.FindWithIncludesAsync(p => true, p => p.Author!);
            var result = posts.OrderByDescending(p => p.CreatedAt).Select(p => new
            {
                id = p.Id,
                title = p.Title,
                slug = p.Slug,
                thumbnail = p.Thumbnail,
                content = p.Content,
                status = p.Status,
                createdAt = p.CreatedAt,
                authorName = p.Author?.FullName ?? "Bác sĩ thú y"
            });
            return Ok(result);
        }

        [Authorize(Roles = "admin,Admin")]
        [HttpPost("admin/posts")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { message = "Tiêu đề bài viết không được để trống." });
            }

            var authorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? authorId = null;
            if (Guid.TryParse(authorIdStr, out var authorGuid))
            {
                authorId = authorGuid;
            }

            var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : GenerateSlug(dto.Slug);

            // Ensure unique slug
            var existing = await _unitOfWork.Posts.FindAsync(p => p.Slug == slug);
            if (existing.Any())
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks % 1000}";
            }

            var post = new Post
            {
                Title = dto.Title,
                Slug = slug,
                Thumbnail = dto.Thumbnail,
                Content = dto.Content,
                AuthorId = authorId,
                Status = dto.Status.ToLower(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Posts.AddAsync(post);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, post });
        }

        [Authorize(Roles = "admin,Admin")]
        [HttpPut("admin/posts/{id}")]
        public async Task<IActionResult> UpdatePost(long id, [FromBody] CreatePostDto dto)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { message = "Tiêu đề bài viết không được để trống." });
            }

            var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : GenerateSlug(dto.Slug);

            // Ensure unique slug
            var existing = await _unitOfWork.Posts.FindAsync(p => p.Slug == slug && p.Id != id);
            if (existing.Any())
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks % 1000}";
            }

            post.Title = dto.Title;
            post.Slug = slug;
            post.Thumbnail = dto.Thumbnail;
            post.Content = dto.Content;
            post.Status = dto.Status.ToLower();

            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, post });
        }

        [Authorize(Roles = "admin,Admin")]
        [HttpDelete("admin/posts/{id}")]
        public async Task<IActionResult> DeletePost(long id)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }

            _unitOfWork.Posts.Remove(post);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // ================= HELPERS =================

        private string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower().Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in str)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            str = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            str = str.Replace('đ', 'd').Replace('Đ', 'd');

            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 60 ? str.Length : 60).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }

    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Thumbnail { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = "draft";
    }
}
