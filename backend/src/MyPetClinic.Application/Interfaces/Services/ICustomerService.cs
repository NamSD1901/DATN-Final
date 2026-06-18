using MyPetClinic.Application.DTOs;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<UserProfileDto>> SearchCustomersAsync(string keyword);
        Task<IEnumerable<UserProfileDto>> GetAllCustomersAsync();
        Task<UserProfileDto?> GetCustomerDetailAsync(Guid id);
        Task<IEnumerable<PetDto>> GetPetsByCustomerAsync(Guid customerId);
        
        /// <summary>
        /// Tạo mới khách hàng cùng với danh sách thú cưng
        /// </summary>
        /// <param name="dto">Dữ liệu khách hàng và thú cưng</param>
        /// <returns>Trả về Guid của khách hàng vừa tạo</returns>
        Task<Guid> CreateCustomerWithPetsAsync(CustomerCreateDto dto);
        
        Task SoftDeleteCustomerAsync(Guid id);
    }
}
