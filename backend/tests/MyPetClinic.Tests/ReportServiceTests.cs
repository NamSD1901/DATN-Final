using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class ReportServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _service = new ReportService(_unitOfWork);
        }

        [Fact]
        public async Task GetRevenueReport_ShouldSumOnlyPaidInvoices()
        {
            // Arrange
            var today = DateTime.UtcNow;

            var invoices = new List<Invoice>
            {
                new Invoice { Id = 1, AppointmentId = 1, Subtotal = 500000, TotalAmount = 500000, PaymentStatus = "Paid", CreatedAt = today },
                new Invoice { Id = 2, AppointmentId = 2, Subtotal = 300000, TotalAmount = 300000, PaymentStatus = "Pending", CreatedAt = today },
                new Invoice { Id = 3, AppointmentId = 3, Subtotal = 200000, TotalAmount = 200000, PaymentStatus = "Paid", CreatedAt = today }
            };

            var invoiceItems = new List<InvoiceItem>
            {
                new InvoiceItem { Id = 1, InvoiceId = 1, ItemType = "service", ItemName = "Khám Lâm Sàng", Quantity = 1, UnitPrice = 200000, TotalPrice = 200000 },
                new InvoiceItem { Id = 2, InvoiceId = 1, ItemType = "medicine", ItemName = "Paracetamol", Quantity = 3, UnitPrice = 100000, TotalPrice = 300000 },
                new InvoiceItem { Id = 3, InvoiceId = 3, ItemType = "service", ItemName = "Tiêm Vaccine", Quantity = 1, UnitPrice = 200000, TotalPrice = 200000 }
            };

            var doctor = new User { Id = Guid.NewGuid(), FullName = "Dr. Green", RoleId = 2 };
            _context.Users.Add(doctor);

            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, PetId = 1, CustomerId = Guid.NewGuid(), DoctorId = doctor.Id, ServiceId = 1, Status = "completed", AppointmentDate = today },
                new Appointment { Id = 3, PetId = 2, CustomerId = Guid.NewGuid(), DoctorId = doctor.Id, ServiceId = 2, Status = "completed", AppointmentDate = today }
            };

            _context.Invoices.AddRange(invoices);
            _context.InvoiceItems.AddRange(invoiceItems);
            _context.Appointments.AddRange(appointments);
            await _context.SaveChangesAsync();

            // Act
            var report = await _service.GetRevenueReportAsync(today.AddDays(-1), today.AddDays(1));

            // Assert
            Assert.Equal(700000, report.TotalRevenue); // Only Paid (500,000 + 200,000)
            Assert.Equal(2, report.InvoicesCount);     // Invoice 1 and Invoice 3
            
            // Check breakdown
            Assert.Equal(3, report.ServiceRevenue.Count); // Khám Lâm Sàng, Paracetamol, Tiêm Vaccine
            
            // Check doctor performance
            Assert.Single(report.DoctorPerformance);
            Assert.Equal("Dr. Green", report.DoctorPerformance[0].DoctorName);
            Assert.Equal(2, report.DoctorPerformance[0].CompletedAppointments);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
