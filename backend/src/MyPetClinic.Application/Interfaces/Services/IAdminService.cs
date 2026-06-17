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
        Task<IEnumerable<Service>> GetServicesAsync();
        Task<Service> CreateServiceAsync(CreateServiceDto dto, string currentUserId);
        Task<Service> UpdateServiceAsync(long id, CreateServiceDto dto, string currentUserId);
        Task DeleteServiceAsync(long id, string currentUserId);

        // Medicines
        Task<IEnumerable<Medicine>> GetMedicinesAsync();
        Task<object> GetMedicineWarningsAsync();
        Task<Medicine> CreateMedicineAsync(CreateMedicineDto dto, string currentUserId);
        Task<Medicine> UpdateMedicineAsync(long id, CreateMedicineDto dto, string currentUserId);
        Task DeleteMedicineAsync(long id, string currentUserId);

        // Schedules
        Task<object> GetSchedulesAsync();
        Task<DoctorSchedule> CreateScheduleAsync(CreateScheduleDto dto, string currentUserId);
        Task<DoctorSchedule> UpdateScheduleAsync(long id, CreateScheduleDto dto, string currentUserId);
        Task DeleteScheduleAsync(long id, string currentUserId);

        // Config
        SlotConfigModel GetSlotConfig();
        Task UpdateSlotConfigAsync(SlotConfigModel config, string currentUserId);
    }
}
