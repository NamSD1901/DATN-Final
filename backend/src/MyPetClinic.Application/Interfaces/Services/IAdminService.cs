using MyPetClinic.Application.DTOs;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IAdminService
    {
        // Users
        Task<object> GetUsersAsync();
        Task UpdateUserRoleAsync(string userId, UpdateRoleDto dto, string currentUserId);
        Task ToggleUserStatusAsync(string userId, ToggleStatusDto dto, string currentUserId);

        // Services
        Task<IEnumerable<ServiceDto>> GetServicesAsync();
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, string currentUserId);
        Task<ServiceDto> UpdateServiceAsync(long id, CreateServiceDto dto, string currentUserId);
        Task DeleteServiceAsync(long id, string currentUserId);

        // Medicines
        Task<IEnumerable<MedicineDto>> GetMedicinesAsync();
        Task<object> GetMedicineWarningsAsync();
        Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto dto, string currentUserId);
        Task<MedicineDto> UpdateMedicineAsync(long id, CreateMedicineDto dto, string currentUserId);
        Task DeleteMedicineAsync(long id, string currentUserId);

        // Vaccines & Batches
        Task<IEnumerable<VaccineAdminDto>> GetVaccinesAsync();
        Task<VaccineAdminDto> CreateVaccineAsync(CreateVaccineDto dto, string currentUserId);
        Task<VaccineAdminDto> UpdateVaccineAsync(long id, CreateVaccineDto dto, string currentUserId);
        Task DeleteVaccineAsync(long id, string currentUserId);
        Task<VaccineBatchAdminDto> CreateVaccineBatchAsync(long vaccineId, CreateVaccineBatchDto dto, string currentUserId);
        Task DeleteVaccineBatchAsync(long batchId, string currentUserId);



        // Config
        SlotConfigModel GetSlotConfig();
        Task UpdateSlotConfigAsync(SlotConfigModel config, string currentUserId);
    }
}
