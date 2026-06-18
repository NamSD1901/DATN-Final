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

        // Schedules
        Task<object> GetSchedulesAsync();
        Task<DoctorScheduleDto> CreateScheduleAsync(CreateScheduleDto dto, string currentUserId);
        Task<DoctorScheduleDto> UpdateScheduleAsync(long id, CreateScheduleDto dto, string currentUserId);
        Task DeleteScheduleAsync(long id, string currentUserId);

        // Config
        SlotConfigModel GetSlotConfig();
        Task UpdateSlotConfigAsync(SlotConfigModel config, string currentUserId);
    }
}
