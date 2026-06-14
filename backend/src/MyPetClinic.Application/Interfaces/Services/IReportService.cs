using MyPetClinic.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
    }
}
