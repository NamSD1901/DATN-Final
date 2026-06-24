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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPetRepository _petRepository;

        public PetService(IUnitOfWork unitOfWork, IPetRepository petRepository)
        {
            _unitOfWork = unitOfWork;
            _petRepository = petRepository;
        }

        public async Task<IEnumerable<PetDto>> GetMyPetsAsync(Guid CustomerId)
        {
            var pets = await _petRepository.GetPetsByOwnerIdAsync(CustomerId);
            return pets.Select(MapToDto);
        }

        public async Task<PetDto?> GetPetByIdAsync(long id, Guid CustomerId)
        {
            var pet = await _petRepository.GetPetByIdAsync(id);
            if (pet == null || pet.CustomerId != CustomerId)
            {
                return null;
            }
            return MapToDto(pet);
        }

        public async Task<PetDto> AddPetAsync(CreatePetDto dto, Guid CustomerId)
        {
            var pet = new Pet
            {
                CustomerId = CustomerId,
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
                Avatar = dto.Avatar,
                CreatedAt = DateTime.UtcNow
            };

            await _petRepository.CreatePetAsync(pet);
            await _petRepository.SaveChangesAsync();
            return MapToDto(pet);
        }

        public async Task UpdatePetAsync(UpdatePetDto dto, Guid CustomerId)
        {
            var pet = await _petRepository.GetPetByIdAsync(dto.Id);
            if (pet == null || pet.CustomerId != CustomerId)
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
            pet.Avatar = dto.Avatar;

            await _petRepository.UpdatePetAsync(pet);
            await _petRepository.SaveChangesAsync();
        }

        public async Task UpdatePetStatusAsync(long petId, bool isDeceased, bool isAggressive)
        {
            var pet = await _petRepository.GetPetByIdAsync(petId);
            if (pet == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thú cưng.");
            }

            pet.IsDeceased = isDeceased;
            pet.IsAggressive = isAggressive;

            if (isDeceased)
            {
                // Hủy mọi lịch hẹn trong tương lai (Status = 'pending' hoặc 'confirmed')
                var pendingAppointments = await _unitOfWork.Appointments.FindAsync(a => a.PetId == petId && (a.Status == "pending" || a.Status == "confirmed") && a.AppointmentDate >= DateTime.UtcNow.Date);
                foreach (var appt in pendingAppointments)
                {
                    appt.Status = "cancelled";
                    _unitOfWork.Appointments.Update(appt);
                }
            }

            await _petRepository.UpdatePetAsync(pet);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeletePetAsync(long id, Guid CustomerId)
        {
            var pet = await _petRepository.GetPetByIdAsync(id);
            if (pet == null || pet.CustomerId != CustomerId)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thú cưng hoặc bạn không có quyền xóa.");
            }

            await _petRepository.SoftDeletePetAsync(id);
            await _petRepository.SaveChangesAsync();
        }

        private PetDto MapToDto(Pet pet)
        {
            return new PetDto
            {
                Id = pet.Id,
                CustomerId = pet.CustomerId,
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
                Avatar = pet.Avatar,
                CreatedAt = pet.CreatedAt
            };
        }
    }
}
