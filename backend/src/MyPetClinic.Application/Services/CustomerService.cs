using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPetRepository _petRepository;

        public CustomerService(IUserRepository userRepository, IPetRepository petRepository)
        {
            _userRepository = userRepository;
            _petRepository = petRepository;
        }

        private async Task<Role?> GetCustomerRoleAsync()
        {
            return await _userRepository.GetRoleByNameAsync("customer");
        }

        public async Task<IEnumerable<User>> SearchCustomersAsync(string keyword)
        {
            var role = await GetCustomerRoleAsync();
            if (role == null) return Enumerable.Empty<User>();

            return await _userRepository.SearchUsersAsync(keyword, role.Id);
        }

        public async Task<IEnumerable<User>> GetAllCustomersAsync()
        {
            var role = await GetCustomerRoleAsync();
            if (role == null) return Enumerable.Empty<User>();

            return await _userRepository.GetUsersByRoleAsync(role.Id);
        }

        public async Task<User?> GetCustomerDetailAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<IEnumerable<Pet>> GetPetsByCustomerAsync(Guid customerId)
        {
            return await _petRepository.GetPetsByOwnerIdAsync(customerId);
        }

        public async Task<Guid> CreateCustomerWithPetsAsync(CustomerCreateDto dto)
        {
            var customerRole = await GetCustomerRoleAsync();
            if (customerRole == null)
            {
                throw new Exception("Role 'customer' not found in database.");
            }

            // Tạo mật khẩu ngẫu nhiên tạm thời (hoặc sinh hash mặc định)
            string tempPassword = BCrypt.Net.BCrypt.HashPassword("123456");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName?.Trim(),
                Email = dto.Email?.Trim().ToLower(),
                Phone = dto.Phone?.Trim(),
                Address = dto.Address?.Trim(),
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth.HasValue ? DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc) : null,
                RoleId = customerRole.Id,
                PasswordHash = tempPassword,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateUserAsync(newUser);

            if (dto.Pets != null && dto.Pets.Any())
            {
                var petsToCreate = dto.Pets.Select(p => new Pet
                {
                    OwnerId = newUser.Id,
                    Name = p.Name?.Trim(),
                    Species = p.Species?.Trim(),
                    Breed = p.Breed?.Trim(),
                    Gender = p.Gender,
                    BirthDate = p.BirthDate.HasValue ? DateTime.SpecifyKind(p.BirthDate.Value, DateTimeKind.Utc) : null,
                    Weight = p.Weight,
                    Color = p.Color?.Trim(),
                    BloodType = p.BloodType?.Trim(),
                    Sterilized = p.Sterilized,
                    MicrochipCode = p.MicrochipCode?.Trim(),
                    AllergyNote = p.AllergyNote?.Trim(),
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _petRepository.CreatePetsAsync(petsToCreate);
            }

            await _userRepository.SaveChangesAsync();

            return newUser.Id;
        }

        public async Task SoftDeleteCustomerAsync(Guid id)
        {
            await _userRepository.SoftDeleteUserAsync(id);
            await _userRepository.SaveChangesAsync();
        }
    }
}
