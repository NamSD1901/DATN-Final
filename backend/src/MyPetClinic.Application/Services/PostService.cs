using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PostDto>> GetPublicPostsAsync(string? search)
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

            return posts.OrderByDescending(p => p.CreatedAt).Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title ?? string.Empty,
                Slug = p.Slug ?? string.Empty,
                Thumbnail = p.Thumbnail ?? "https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop",
                Content = p.Content,
                Status = p.Status ?? string.Empty,
                CreatedAt = p.CreatedAt,
                AuthorName = p.Author?.FullName ?? "Bác sĩ thú y"
            });
        }

        public async Task<PostDto?> GetPostBySlugAsync(string slug)
        {
            var posts = await _unitOfWork.Posts.FindWithIncludesAsync(
                p => p.Slug == slug && p.Status.ToLower() == "published",
                p => p.Author!
            );

            var post = posts.FirstOrDefault();
            if (post == null) return null;

            return new PostDto
            {
                Id = post.Id,
                Title = post.Title ?? string.Empty,
                Slug = post.Slug ?? string.Empty,
                Thumbnail = post.Thumbnail ?? "https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop",
                Content = post.Content,
                Status = post.Status ?? string.Empty,
                CreatedAt = post.CreatedAt,
                AuthorName = post.Author?.FullName ?? "Bác sĩ thú y"
            };
        }

        public async Task<IEnumerable<PostDto>> GetAdminPostsAsync()
        {
            var posts = await _unitOfWork.Posts.FindWithIncludesAsync(p => true, p => p.Author!);
            return posts.OrderByDescending(p => p.CreatedAt).Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title ?? string.Empty,
                Slug = p.Slug ?? string.Empty,
                Thumbnail = p.Thumbnail,
                Content = p.Content,
                Status = p.Status ?? string.Empty,
                CreatedAt = p.CreatedAt,
                AuthorName = p.Author?.FullName ?? "Bác sĩ thú y"
            });
        }

        public async Task<PostDto> CreatePostAsync(CreatePostDto dto, Guid? authorId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Tiêu đề bài viết không được để trống.");

            var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : GenerateSlug(dto.Slug);

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

            return new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Thumbnail = post.Thumbnail,
                Content = post.Content,
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                AuthorName = "Bác sĩ thú y" // Simplify since we don't fetch Author entity right away
            };
        }

        public async Task<PostDto> UpdatePostAsync(long id, CreatePostDto dto)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy bài viết.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Tiêu đề bài viết không được để trống.");

            var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : GenerateSlug(dto.Slug);

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

            return new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Thumbnail = post.Thumbnail,
                Content = post.Content,
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                AuthorName = "Bác sĩ thú y" // Simplify
            };
        }

        public async Task DeletePostAsync(long id)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy bài viết.");

            _unitOfWork.Posts.Remove(post);
            await _unitOfWork.SaveChangesAsync();
        }

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
}
