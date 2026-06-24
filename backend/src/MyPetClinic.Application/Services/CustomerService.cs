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
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserProfileDto>> SearchCustomersAsync(string keyword)
        {
            IEnumerable<Customer> customers;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                customers = await _unitOfWork.Customers.FindAsync(c => c.DeletedAt == null);
            }
            else
            {
                var lowerKeyword = keyword.ToLower();
                customers = await _unitOfWork.Customers.FindAsync(c => 
                    c.DeletedAt == null && (
                    (c.FullName != null && c.FullName.ToLower().Contains(lowerKeyword)) ||
                    (c.Phone != null && c.Phone.Contains(keyword)) ||
                    (c.Email != null && c.Email.ToLower().Contains(lowerKeyword)) ||
                    (c.CustomerCode != null && c.CustomerCode.ToLower().Contains(lowerKeyword))
                    )
                );
            }

            return customers.Select(c => new UserProfileDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                Gender = c.Gender,
                DateOfBirth = c.DateOfBirth,
                Avatar = c.Avatar,
                RoleName = "Customer"
            });
        }

        public async Task<IEnumerable<UserProfileDto>> GetAllCustomersAsync()
        {
            var customers = await _unitOfWork.Customers.FindAsync(c => c.DeletedAt == null);

            return customers.Select(c => new UserProfileDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                Gender = c.Gender,
                DateOfBirth = c.DateOfBirth,
                Avatar = c.Avatar,
                RoleName = "Customer"
            });
        }

        public async Task<UserProfileDto?> GetCustomerDetailAsync(Guid id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null || customer.DeletedAt != null) return null;

            return new UserProfileDto
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                Gender = customer.Gender,
                DateOfBirth = customer.DateOfBirth,
                Avatar = customer.Avatar,
                RoleName = "Customer"
            };
        }

        public async Task<IEnumerable<PetDto>> GetPetsByCustomerAsync(Guid customerId)
        {
            var pets = await _unitOfWork.Pets.FindAsync(p => p.CustomerId == customerId && p.DeletedAt == null);

            return pets.Select(p => new PetDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                Gender = p.Gender,
                BirthDate = p.BirthDate,
                Weight = p.Weight,
                Color = p.Color,
                BloodType = p.BloodType,
                Sterilized = p.Sterilized,
                MicrochipCode = p.MicrochipCode,
                AllergyNote = p.AllergyNote
            });
        }

        public async Task<Guid> CreateCustomerWithPetsAsync(CustomerCreateDto dto)
        {
            // Kiểm tra email
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingEmail = await _unitOfWork.Customers.GetFirstOrDefaultWithIncludesAsync(c => c.Email == dto.Email.Trim().ToLower() && c.DeletedAt == null);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng. Vui lòng dùng email khác.");
                }
            }

            // Kiểm tra SĐT
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existingPhone = await _unitOfWork.Customers.GetFirstOrDefaultWithIncludesAsync(c => c.Phone == dto.Phone.Trim() && c.DeletedAt == null);
                if (existingPhone != null)
                {
                    throw new InvalidOperationException($"Số điện thoại '{dto.Phone}' đã tồn tại.");
                }
            }

            var newCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                CustomerCode = "CUS" + DateTime.UtcNow.ToString("yyMMddHHmmss"),
                FullName = dto.FullName?.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower(),
                Phone = dto.Phone?.Trim(),
                Address = dto.Address?.Trim(),
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth.HasValue ? DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc) : null,
                HasAccount = false,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Customers.AddAsync(newCustomer);

            if (dto.Pets != null && dto.Pets.Any())
            {
                foreach (var p in dto.Pets)
                {
                    var newPet = new Pet
                    {
                        CustomerId = newCustomer.Id,
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
                    };
                    await _unitOfWork.Pets.AddAsync(newPet);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return newCustomer.Id;
        }

        public async Task SoftDeleteCustomerAsync(Guid id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer != null && customer.DeletedAt == null)
            {
                customer.DeletedAt = DateTime.UtcNow;
                customer.Status = "Inactive";
                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
