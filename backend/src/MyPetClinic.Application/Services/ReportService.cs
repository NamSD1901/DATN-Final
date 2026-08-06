using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    /// <summary>
    /// Application Service xử lý nghiệp vụ báo cáo doanh thu.
    /// Tầng Application tổng hợp dữ liệu từ nhiều nguồn (hóa đơn, lịch hẹn)
    /// thông qua IUnitOfWork mà không phụ thuộc trực tiếp vào EF DbContext.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            // Chuẩn hóa khoảng thời gian về UTC, bao phủ toàn bộ ngày kết thúc
            var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var endUtc   = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            // 1. Lọc hóa đơn đã thanh toán trong khoảng thời gian
            var paidInvoicesQuery = _unitOfWork.Invoices.Query()
                .Where(i => i.PaymentStatus.ToLower() == "paid"
                         && i.CreatedAt >= startUtc
                         && i.CreatedAt <= endUtc);

            var totalRevenue  = await paidInvoicesQuery.SumAsync(i => i.TotalAmount);
            var invoicesCount = await paidInvoicesQuery.CountAsync();

            // 2. Doanh thu theo ngày (GroupBy trên DB Server)
            var dailyRevenue = await paidInvoicesQuery
                .GroupBy(i => i.CreatedAt.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date   = g.Key,
                    Amount = g.Sum(i => i.TotalAmount),
                    Count  = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // 3. Doanh thu theo dịch vụ / thuốc (GroupBy trên DB Server)
            var invoiceIds = await paidInvoicesQuery.Select(i => i.Id).ToListAsync();

            var serviceRevenue = await _unitOfWork.InvoiceItems.Query()
                .Where(ii => invoiceIds.Contains(ii.InvoiceId))
                .GroupBy(ii => new { ii.ItemType, ii.ItemName })
                .Select(g => new ServiceRevenueDto
                {
                    ServiceName = g.Key.ItemName ?? string.Empty,
                    ItemType    = g.Key.ItemType ?? string.Empty,
                    Amount      = g.Sum(ii => ii.TotalPrice),
                    Count       = g.Sum(ii => ii.Quantity)
                })
                .OrderByDescending(s => s.Amount)
                .ToListAsync();

            // 4. Hiệu suất bác sĩ: đếm lịch khám hoàn thành trong kỳ
            var doctorPerformance = await _unitOfWork.Appointments.Query()
                .Where(a => a.Status.ToLower() == "completed"
                         && a.AppointmentDate >= startUtc
                         && a.AppointmentDate <= endUtc)
                .GroupBy(a => a.Doctor!.FullName)
                .Select(g => new DoctorPerformanceDto
                {
                    DoctorName            = g.Key ?? "Bác sĩ thú y",
                    CompletedAppointments = g.Count()
                })
                .OrderByDescending(d => d.CompletedAppointments)
                .ToListAsync();

            return new RevenueReportDto
            {
                TotalRevenue      = totalRevenue,
                InvoicesCount     = invoicesCount,
                DailyRevenue      = dailyRevenue,
                ServiceRevenue    = serviceRevenue,
                DoctorPerformance = doctorPerformance
            };
        }
    }
}
