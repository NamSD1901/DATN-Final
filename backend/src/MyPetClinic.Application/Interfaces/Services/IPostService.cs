using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IPostService
    {
        Task<PaginatedResultDto<PostDto>> GetPublicPostsAsync(int pageIndex, int pageSize, string? search, string? categorySlug, string? tagSlug);
        Task<PostDto> GetPostBySlugAsync(string slug);
        Task<PaginatedResultDto<PostDto>> GetAdminPostsAsync(int pageIndex, int pageSize, string? search, string? status, long? categoryId);
        Task<PostDto> CreatePostAsync(CreatePostDto dto, Guid? authorId);
        Task<PostDto> UpdatePostAsync(long id, UpdatePostDto dto, Guid? updaterId);
        Task DeletePostAsync(long id);
        Task IncrementViewCountAsync(long id);
        Task<List<PostDto>> GetRelatedPostsAsync(long postId, int count);
    }
}
