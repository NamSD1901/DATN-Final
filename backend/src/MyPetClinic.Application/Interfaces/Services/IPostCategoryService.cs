using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IPostCategoryService
    {
        Task<List<PostCategoryDto>> GetAllAsync();
        Task<PostCategoryDto> GetByIdAsync(long id);
        Task<PostCategoryDto> CreateAsync(CreatePostCategoryDto dto);
        Task<PostCategoryDto> UpdateAsync(long id, UpdatePostCategoryDto dto);
        Task DeleteAsync(long id);
    }
}
