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
    public class BannerService : IBannerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BannerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<BannerDto>> GetAllAsync(bool onlyActive = false)
        {
            var now = DateTime.UtcNow;
            var banners = await _unitOfWork.Banners.FindAsync(b => 
                !onlyActive || (b.IsActive && 
                (!b.StartDate.HasValue || b.StartDate <= now) &&
                (!b.EndDate.HasValue || b.EndDate >= now))
            );

            return banners.OrderBy(b => b.Order).Select(MapToDto).ToList();
        }

        public async Task<BannerDto> GetByIdAsync(long id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null) throw new Exception("Banner not found");

            return MapToDto(banner);
        }

        public async Task<BannerDto> CreateAsync(CreateBannerDto dto)
        {
            var banner = new Banner
            {
                Title = dto.Title,
                ImageUrl = dto.ImageUrl,
                LinkUrl = dto.LinkUrl,
                Order = dto.Order,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Banners.AddAsync(banner);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(banner);
        }

        public async Task<BannerDto> UpdateAsync(long id, UpdateBannerDto dto)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null) throw new Exception("Banner not found");

            banner.Title = dto.Title;
            banner.ImageUrl = dto.ImageUrl;
            banner.LinkUrl = dto.LinkUrl;
            banner.Order = dto.Order;
            banner.StartDate = dto.StartDate;
            banner.EndDate = dto.EndDate;
            banner.IsActive = dto.IsActive;

            _unitOfWork.Banners.Update(banner);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(banner);
        }

        public async Task DeleteAsync(long id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null) throw new Exception("Banner not found");

            _unitOfWork.Banners.Remove(banner);
            await _unitOfWork.SaveChangesAsync();
        }

        private BannerDto MapToDto(Banner b)
        {
            return new BannerDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl,
                Order = b.Order,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                IsActive = b.IsActive
            };
        }
    }
}
