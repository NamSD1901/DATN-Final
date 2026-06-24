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
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TagService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<TagDto>> GetAllAsync()
        {
            var tags = await _unitOfWork.Tags.GetAllAsync();
            return tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                IsActive = t.IsActive
            }).ToList();
        }

        public async Task<TagDto> GetByIdAsync(long id)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null) throw new Exception("Tag not found");

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                IsActive = tag.IsActive
            };
        }

        public async Task<TagDto> CreateAsync(CreateTagDto dto)
        {
            var existing = await _unitOfWork.Tags.FindAsync(t => t.Slug == dto.Slug);
            if (existing.Any())
                throw new Exception("Tag slug already exists");

            var tag = new Tag
            {
                Name = dto.Name,
                Slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-"),
                IsActive = dto.IsActive
            };

            await _unitOfWork.Tags.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                IsActive = tag.IsActive
            };
        }

        public async Task<TagDto> UpdateAsync(long id, UpdateTagDto dto)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null) throw new Exception("Tag not found");

            if (dto.Slug != tag.Slug)
            {
                var existing = await _unitOfWork.Tags.FindAsync(t => t.Slug == dto.Slug);
                if (existing.Any()) throw new Exception("Tag slug already exists");
            }

            tag.Name = dto.Name;
            if (dto.Slug != null) tag.Slug = dto.Slug;
            tag.IsActive = dto.IsActive;

            _unitOfWork.Tags.Update(tag);
            await _unitOfWork.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug,
                IsActive = tag.IsActive
            };
        }

        public async Task DeleteAsync(long id)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null) throw new Exception("Tag not found");

            _unitOfWork.Tags.Remove(tag);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
