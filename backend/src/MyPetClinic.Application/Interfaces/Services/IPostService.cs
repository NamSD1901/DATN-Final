using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetPublicPostsAsync(string? search);
        Task<PostDto?> GetPostBySlugAsync(string slug);
        Task<IEnumerable<PostDto>> GetAdminPostsAsync();
        Task<PostDto> CreatePostAsync(CreatePostDto dto, Guid? authorId);
        Task<PostDto> UpdatePostAsync(long id, CreatePostDto dto);
        Task DeletePostAsync(long id);
    }
}
