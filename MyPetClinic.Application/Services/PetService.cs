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
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        public async Task<IEnumerable<PetDto>> GetMyPetsAsync(Guid ownerId)
        {
            var pets = await _petRepository.GetPetsByOwnerIdAsync(ownerId);
            return pets.Select(MapToDto);
        }

        public async Task<PetDto?> GetPetByIdAsync(long id, Guid ownerId)
        {
            var pet = await _petRepository.GetByIdAsync(id);
            if (pet == null || pet.OwnerId != ownerId)
            {
                return null;
            }
            return MapToDto(pet);
        }

        public async Task<PetDto> AddPetAsync(CreatePetDto dto, Guid ownerId)
        {
            var pet = new Pet
            {
                OwnerId = ownerId,
                Name = dto.Name,
                Species = dto.Species,
                Breed = dto.Breed,
                Gender = dto.Gender,
                BirthDate = dto.BirthDate,
                Weight = dto.Weight,
                Color = dto.Color,
                BloodType = dto.BloodType,
                Sterilized = dto.Sterilized,
                MicrochipCode = dto.MicrochipCode,
                AllergyNote = dto.AllergyNote,
                CreatedAt = DateTime.UtcNow
            };

            await _petRepository.AddAsync(pet);
            return MapToDto(pet);
        }

        public async Task UpdatePetAsync(UpdatePetDto dto, Guid ownerId)
        {
            var pet = await _petRepository.GetByIdAsync(dto.Id);
            if (pet == null || pet.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thú cưng hoặc bạn không có quyền sửa.");
            }

            pet.Name = dto.Name;
            pet.Species = dto.Species;
            pet.Breed = dto.Breed;
            pet.Gender = dto.Gender;
            pet.BirthDate = dto.BirthDate;
            pet.Weight = dto.Weight;
            pet.Color = dto.Color;
            pet.BloodType = dto.BloodType;
            pet.Sterilized = dto.Sterilized;
            pet.MicrochipCode = dto.MicrochipCode;
            pet.AllergyNote = dto.AllergyNote;

            await _petRepository.UpdateAsync(pet);
        }

        public async Task DeletePetAsync(long id, Guid ownerId)
        {
            var pet = await _petRepository.GetByIdAsync(id);
            if (pet == null || pet.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thú cưng hoặc bạn không có quyền xóa.");
            }

            await _petRepository.DeleteAsync(pet);
        }

        private PetDto MapToDto(Pet pet)
        {
            return new PetDto
            {
                Id = pet.Id,
                OwnerId = pet.OwnerId,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Gender = pet.Gender,
                BirthDate = pet.BirthDate,
                Weight = pet.Weight,
                Color = pet.Color,
                BloodType = pet.BloodType,
                Sterilized = pet.Sterilized,
                MicrochipCode = pet.MicrochipCode,
                AllergyNote = pet.AllergyNote,
                CreatedAt = pet.CreatedAt
            };
        }
    }
}
