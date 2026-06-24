using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IBannerService
    {
        Task<List<BannerDto>> GetAllAsync(bool onlyActive = false);
        Task<BannerDto> GetByIdAsync(long id);
        Task<BannerDto> CreateAsync(CreateBannerDto dto);
        Task<BannerDto> UpdateAsync(long id, UpdateBannerDto dto);
        Task DeleteAsync(long id);
    }
}
