using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<PaginatedResultDto<PostDto>> GetPublicPostsAsync(int pageIndex, int pageSize, string? search, string? categorySlug, string? tagSlug)
        {
            var now = DateTime.UtcNow;
            
            var allPosts = await _unitOfWork.Posts.FindWithIncludesAsync(
                p => p.Status == "published" && (!p.PublishedAt.HasValue || p.PublishedAt <= now),
                p => p.Category!,
                p => p.Author!,
                p => p.PostTags!
            );
            
            // To get tags we need to fetch them
            // In a better approach, we'd use specialized repository method
            var tags = await _unitOfWork.Tags.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                allPosts = allPosts.Where(p => (p.Title != null && p.Title.ToLower().Contains(lowerSearch)) || (p.Slug != null && p.Slug.Contains(lowerSearch)));
            }

            if (!string.IsNullOrWhiteSpace(categorySlug))
            {
                allPosts = allPosts.Where(p => p.Category != null && p.Category.Slug == categorySlug);
            }

            if (!string.IsNullOrWhiteSpace(tagSlug))
            {
                var tagId = tags.FirstOrDefault(t => t.Slug == tagSlug)?.Id;
                if (tagId.HasValue)
                {
                    allPosts = allPosts.Where(p => p.PostTags != null && p.PostTags.Any(pt => pt.TagId == tagId.Value));
                }
                else
                {
                    allPosts = new List<Post>(); // Tag not found
                }
            }

            var totalCount = allPosts.Count();
            var pagedPosts = allPosts.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = pagedPosts.Select(p => MapToDto(p, tags.ToList())).ToList();

            return new PaginatedResultDto<PostDto>(dtos, totalCount, pageIndex, pageSize);
        }

        public async Task<PostDto> GetPostBySlugAsync(string slug)
        {
            var posts = await _unitOfWork.Posts.FindWithIncludesAsync(
                p => p.Slug == slug,
                p => p.Category!,
                p => p.Author!,
                p => p.PostTags!
            );

            var post = posts.FirstOrDefault();
            if (post == null || post.Status == "archived")
                throw new Exception("Post not found");

            var tags = await _unitOfWork.Tags.GetAllAsync();
            return MapToDto(post, tags.ToList());
        }

        public async Task<PaginatedResultDto<PostDto>> GetAdminPostsAsync(int pageIndex, int pageSize, string? search, string? status, long? categoryId)
        {
            var allPosts = await _unitOfWork.Posts.FindWithIncludesAsync(
                p => true,
                p => p.Category!,
                p => p.Author!,
                p => p.PostTags!
            );

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                allPosts = allPosts.Where(p => p.Title != null && p.Title.ToLower().Contains(lowerSearch));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                allPosts = allPosts.Where(p => p.Status == status);
            }

            if (categoryId.HasValue)
            {
                allPosts = allPosts.Where(p => p.CategoryId == categoryId.Value);
            }

            var totalCount = allPosts.Count();
            var pagedPosts = allPosts.OrderByDescending(p => p.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var tags = await _unitOfWork.Tags.GetAllAsync();
            var dtos = pagedPosts.Select(p => MapToDto(p, tags.ToList())).ToList();

            return new PaginatedResultDto<PostDto>(dtos, totalCount, pageIndex, pageSize);
        }

        public async Task<PostDto> CreatePostAsync(CreatePostDto dto, Guid? authorId)
        {
            var slug = dto.Slug ?? dto.Title.ToLower().Replace(" ", "-");
            var existing = await _unitOfWork.Posts.FindAsync(p => p.Slug == slug);
            if (existing.Any())
                throw new Exception("Slug already exists");

            var post = new Post
            {
                Title = dto.Title,
                Slug = slug,
                Thumbnail = dto.Thumbnail,
                Summary = dto.Summary,
                Content = dto.Content,
                Status = dto.Status,
                PublishedAt = dto.PublishedAt ?? (dto.Status == "published" ? DateTime.UtcNow : null),
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                Keywords = dto.Keywords,
                CategoryId = dto.CategoryId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            await SyncTags(post, dto.Tags);

            await _unitOfWork.Posts.AddAsync(post);
            await _unitOfWork.SaveChangesAsync();

            return await GetPostBySlugAsync(post.Slug);
        }

        public async Task<PostDto> UpdatePostAsync(long id, UpdatePostDto dto, Guid? updaterId)
        {
            var posts = await _unitOfWork.Posts.FindWithIncludesAsync(p => p.Id == id, p => p.PostTags!);
            var post = posts.FirstOrDefault();

            if (post == null) throw new Exception("Post not found");

            var slug = dto.Slug ?? dto.Title.ToLower().Replace(" ", "-");
            if (post.Slug != slug)
            {
                var existing = await _unitOfWork.Posts.FindAsync(p => p.Slug == slug);
                if (existing.Any()) throw new Exception("Slug already exists");
            }

            post.Title = dto.Title;
            post.Slug = slug;
            post.Thumbnail = dto.Thumbnail;
            post.Summary = dto.Summary;
            post.Content = dto.Content;
            
            if (post.Status != "published" && dto.Status == "published" && !post.PublishedAt.HasValue)
            {
                post.PublishedAt = dto.PublishedAt ?? DateTime.UtcNow;
            }
            else if (dto.Status == "published")
            {
                post.PublishedAt = dto.PublishedAt ?? post.PublishedAt;
            }

            post.Status = dto.Status;
            post.MetaTitle = dto.MetaTitle;
            post.MetaDescription = dto.MetaDescription;
            post.Keywords = dto.Keywords;
            post.CategoryId = dto.CategoryId;
            post.UpdatedBy = updaterId;
            post.UpdatedAt = DateTime.UtcNow;

            await SyncTags(post, dto.Tags);

            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();

            return await GetPostBySlugAsync(post.Slug);
        }

        public async Task DeletePostAsync(long id)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null) throw new Exception("Post not found");

            post.Status = "archived";
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task IncrementViewCountAsync(long id)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post != null)
            {
                post.ViewCount += 1;
                _unitOfWork.Posts.Update(post);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<List<PostDto>> GetRelatedPostsAsync(long postId, int count)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(postId);
            if (post == null || !post.CategoryId.HasValue) return new List<PostDto>();

            var related = await _unitOfWork.Posts.FindWithIncludesAsync(
                p => p.Id != postId && p.CategoryId == post.CategoryId && p.Status == "published",
                p => p.Category!,
                p => p.Author!,
                p => p.PostTags!
            );

            var tags = await _unitOfWork.Tags.GetAllAsync();
            
            return related.OrderByDescending(p => p.PublishedAt)
                .Take(count)
                .Select(p => MapToDto(p, tags.ToList()))
                .ToList();
        }

        private async Task SyncTags(Post post, List<string> tagNames)
        {
            if (post.PostTags == null) post.PostTags = new List<PostTag>();
            
            var allTags = await _unitOfWork.Tags.GetAllAsync();
            var existingTags = allTags.Where(t => tagNames.Contains(t.Name)).ToList();

            var newTagNames = tagNames.Except(existingTags.Select(t => t.Name)).ToList();
            
            foreach(var newTagName in newTagNames)
            {
                var newTag = new Tag
                {
                    Name = newTagName,
                    Slug = newTagName.ToLower().Replace(" ", "-")
                };
                await _unitOfWork.Tags.AddAsync(newTag);
                existingTags.Add(newTag);
            }

            post.PostTags.Clear();
            foreach(var tag in existingTags)
            {
                post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id, Post = post, Tag = tag });
            }
        }

        private PostDto MapToDto(Post p, List<Tag> allTags)
        {
            var tagNames = new List<string>();
            if (p.PostTags != null)
            {
                foreach(var pt in p.PostTags)
                {
                    var tag = allTags.FirstOrDefault(t => t.Id == pt.TagId);
                    if (tag != null) tagNames.Add(tag.Name);
                }
            }

            return new PostDto
            {
                Id = p.Id,
                Title = p.Title ?? "",
                Slug = p.Slug ?? "",
                Thumbnail = p.Thumbnail,
                Summary = p.Summary,
                Content = p.Content,
                Status = p.Status ?? "",
                ViewCount = p.ViewCount,
                PublishedAt = p.PublishedAt,
                MetaTitle = p.MetaTitle,
                MetaDescription = p.MetaDescription,
                Keywords = p.Keywords,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                AuthorName = p.Author?.FullName ?? "Bác sĩ thú y",
                CreatedAt = p.CreatedAt,
                Tags = tagNames
            };
        }
    }
}
