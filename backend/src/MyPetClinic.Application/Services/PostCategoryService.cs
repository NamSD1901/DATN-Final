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
    public class PostCategoryService : IPostCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PostCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<PostCategoryDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.PostCategories.FindWithIncludesAsync(
                c => c.ParentId == null,
                c => c.Children
            );

            return categories.Select(MapToDto).ToList();
        }

        public async Task<PostCategoryDto> GetByIdAsync(long id)
        {
            var categories = await _unitOfWork.PostCategories.FindWithIncludesAsync(
                c => c.Id == id,
                c => c.Children,
                c => c.Parent!
            );

            var category = categories.FirstOrDefault();
            if (category == null) throw new Exception("Category not found");

            return MapToDto(category);
        }

        public async Task<PostCategoryDto> CreateAsync(CreatePostCategoryDto dto)
        {
            var existing = await _unitOfWork.PostCategories.FindAsync(c => c.Slug == dto.Slug);
            if (existing.Any())
                throw new Exception("Slug already exists");

            var category = new PostCategory
            {
                Name = dto.Name,
                Slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-"),
                Description = dto.Description,
                IsActive = dto.IsActive,
                ParentId = dto.ParentId
            };

            await _unitOfWork.PostCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task<PostCategoryDto> UpdateAsync(long id, UpdatePostCategoryDto dto)
        {
            var category = await _unitOfWork.PostCategories.GetByIdAsync(id);
            if (category == null) throw new Exception("Category not found");

            if (dto.Slug != category.Slug)
            {
                var existing = await _unitOfWork.PostCategories.FindAsync(c => c.Slug == dto.Slug);
                if (existing.Any()) throw new Exception("Slug already exists");
            }

            category.Name = dto.Name;
            if (dto.Slug != null) category.Slug = dto.Slug;
            category.Description = dto.Description;
            category.IsActive = dto.IsActive;
            category.ParentId = dto.ParentId;

            _unitOfWork.PostCategories.Update(category);
            await _unitOfWork.SaveChangesAsync();
            
            return MapToDto(category);
        }

        public async Task DeleteAsync(long id)
        {
            var categories = await _unitOfWork.PostCategories.FindWithIncludesAsync(c => c.Id == id, c => c.Posts);
            var category = categories.FirstOrDefault();
            
            if (category == null) throw new Exception("Category not found");

            if (category.Posts != null && category.Posts.Any())
                throw new Exception("Cannot delete category with posts");

            _unitOfWork.PostCategories.Remove(category);
            await _unitOfWork.SaveChangesAsync();
        }

        private PostCategoryDto MapToDto(PostCategory c)
        {
            return new PostCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive,
                ParentId = c.ParentId,
                ParentName = c.Parent?.Name,
                Children = c.Children?.Select(MapToDto).ToList() ?? new List<PostCategoryDto>()
            };
        }
    }
}
