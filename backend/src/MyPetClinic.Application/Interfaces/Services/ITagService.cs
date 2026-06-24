using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface ITagService
    {
        Task<List<TagDto>> GetAllAsync();
        Task<TagDto> GetByIdAsync(long id);
        Task<TagDto> CreateAsync(CreateTagDto dto);
        Task<TagDto> UpdateAsync(long id, UpdateTagDto dto);
        Task DeleteAsync(long id);
    }
}
