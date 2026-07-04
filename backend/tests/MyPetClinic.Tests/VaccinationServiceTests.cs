using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class VaccinationServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly VaccinationService _service;

        public VaccinationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _service = new VaccinationService(_unitOfWork);
        }

        [Fact]
        public async Task SubmitSoapRecordAsync_WithEligibleAssessment_ShouldDecreaseStockAndCreateInvoice()
        {
            // Arrange
            var pet = new Pet { Id = 1, CustomerId = Guid.NewGuid(), Name = "Milo" };
            var doctorId = Guid.NewGuid();
            var appointment = new Appointment
            {
                Id = 1,
                PetId = 1,
                CustomerId = Guid.NewGuid(),
                DoctorId = doctorId,
                ServiceId = 1,
                Status = "waiting",
                AppointmentDate = DateTime.UtcNow
            };

            var vaccine = new Vaccine { Id = 1, Name = "Rabies Vaccine", StockQuantity = 10, IntervalDays = 365 };
            var batch = new VaccineBatch { Id = 1, VaccineId = 1, BatchNumber = "B01", ExpirationDate = DateTime.UtcNow.AddMonths(6), StockQuantity = 5, SellingPrice = 200000 };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Vaccines.Add(vaccine);
            _context.VaccineBatches.Add(batch);
            await _context.SaveChangesAsync();

            var request = new VaccinationSoapRequestDto
            {
                ReasonForVisit = "Tiêm cơ bản",
                EatingStatus = "Bình thường",
                MentalStatus = "Linh hoạt",
                MucosaStatus = "Hồng hào",
                Weight = 5.0m,
                ClinicalAssessment = "Đủ điều kiện",
                VaccineId = 1,
                VaccineBatchId = 1,
                NextDueDate = DateTime.UtcNow.AddDays(365)
            };

            // Act
            var recordId = await _service.SubmitSoapRecordAsync(1, doctorId, request);

            // Assert
            var record = await _context.VaccinationRecords.FindAsync(recordId);
            Assert.NotNull(record);
            Assert.Equal("Đủ điều kiện", record.ClinicalAssessment);

            // Verify stock decreased
            var updatedBatch = await _context.VaccineBatches.FindAsync(1L);
            Assert.Equal(4, updatedBatch!.StockQuantity);

            var updatedVaccine = await _context.Vaccines.FindAsync(1L);
            Assert.Equal(9, updatedVaccine!.StockQuantity);

            // Verify invoice created for Vaccine
            var invoice = await _context.Invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.AppointmentId == 1);
            Assert.NotNull(invoice);
            Assert.Equal(200000, invoice.TotalAmount);
            Assert.Contains(invoice.InvoiceItems, i => i.ItemType == "Vaccine" && i.ItemId == 1);
        }

        [Fact]
        public async Task SubmitSoapRecordAsync_WithDeferAssessment_ShouldCreateConsultationFeeOnly_AndNotDecreaseStock()
        {
            // Arrange
            var pet = new Pet { Id = 2, CustomerId = Guid.NewGuid(), Name = "Milo2" };
            var doctorId = Guid.NewGuid();
            var appointment = new Appointment
            {
                Id = 2,
                PetId = 2,
                CustomerId = Guid.NewGuid(),
                DoctorId = doctorId,
                ServiceId = 1,
                Status = "waiting",
                AppointmentDate = DateTime.UtcNow
            };

            var vaccine = new Vaccine { Id = 2, Name = "Single Dose Vaccine", StockQuantity = 5 };
            var batch = new VaccineBatch { Id = 2, VaccineId = 2, BatchNumber = "B02", ExpirationDate = DateTime.UtcNow.AddMonths(6), StockQuantity = 5, SellingPrice = 200000 };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Vaccines.Add(vaccine);
            _context.VaccineBatches.Add(batch);
            await _context.SaveChangesAsync();

            var request = new VaccinationSoapRequestDto
            {
                ReasonForVisit = "Tiêm cơ bản",
                EatingStatus = "Biếng ăn",
                MentalStatus = "Lờ đờ",
                MucosaStatus = "Nhợt nhạt",
                Weight = 4.0m,
                ClinicalAssessment = "Hoãn tiêm",
                DoctorRemarks = "Sốt cao, không đủ sức khỏe"
                // No VaccineId or VaccineBatchId provided
            };

            // Act
            var recordId = await _service.SubmitSoapRecordAsync(2, doctorId, request);

            // Assert
            var record = await _context.VaccinationRecords.FindAsync(recordId);
            Assert.NotNull(record);
            Assert.Equal("Hoãn tiêm", record.ClinicalAssessment);

            // Verify stock did NOT decrease
            var updatedBatch = await _context.VaccineBatches.FindAsync(2L);
            Assert.Equal(5, updatedBatch!.StockQuantity);

            var updatedVaccine = await _context.Vaccines.FindAsync(2L);
            Assert.Equal(5, updatedVaccine!.StockQuantity);

            // Verify invoice created for Consultation Fee (100,000)
            var invoice = await _context.Invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.AppointmentId == 2);
            Assert.NotNull(invoice);
            Assert.Equal(100000, invoice.TotalAmount);
            Assert.Contains(invoice.InvoiceItems, i => i.ItemType == "Service" && i.ItemName != null && i.ItemName.Contains("Phí khám lâm sàng"));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
