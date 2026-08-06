using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int InvoicesCount { get; set; }
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new List<DailyRevenueDto>();
        public List<ServiceRevenueDto> ServiceRevenue { get; set; } = new List<ServiceRevenueDto>();
        public List<DoctorPerformanceDto> DoctorPerformance { get; set; } = new List<DoctorPerformanceDto>();
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class ServiceRevenueDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class DoctorPerformanceDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public int CompletedAppointments { get; set; }
    }
}
