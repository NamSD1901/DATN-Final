using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public class InventoryIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        
        private readonly MedicineService _medicineService;
        private readonly MedicalRecordService _medicalRecordService;

        public InventoryIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(_options);
            _unitOfWork = new UnitOfWork(_context);
            
            var medicineRepo = new MedicineRepository(_context);
            var batchRepo = new MedicineBatchRepository(_context);
            var transactionRepo = new InventoryTransactionRepository(_context);
            
            _medicineService = new MedicineService(_unitOfWork, medicineRepo, batchRepo, transactionRepo);
            _medicalRecordService = new MedicalRecordService(_unitOfWork, _medicineService);
        }

        [Fact]
        public async Task InventoryFlow_Import_Prescribe_Export_Audit_ShouldWorkCorrectly()
        {
            // 1. Arrange - Seed basic data
            var doctorId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            var pet = new Pet { Id = 1, CustomerId = Guid.NewGuid(), Name = "Luna" };
            var appointment = new Appointment
            {
                Id = 1,
                PetId = 1,
                CustomerId = Guid.NewGuid(),
                DoctorId = doctorId,
                ServiceId = 1,
                Status = "in_progress",
                AppointmentDate = DateTime.UtcNow
            };
            var medicine = new Medicine
            {
                Id = 1,
                Name = "Amoxicillin",
                SellPrice = 15000,
                Unit = "Viên"
            };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            // 2. Action 1: Nhập Kho (Import)
            var importDto = new ImportMedicineDto
            {
                MedicineId = 1,
                BatchNumber = "LOT001",
                ManufactureDate = DateTime.UtcNow.AddMonths(-1),
                ExpiryDate = DateTime.UtcNow.AddMonths(12),
                Quantity = 100, // Nhập 100 viên
                ReferenceCode = "INV-001"
            };
            await _medicineService.ImportMedicineAsync(importDto, adminId);

            // Verify Import
            using (var verifyCtx1 = new ApplicationDbContext(_options))
            {
                var med = await verifyCtx1.Medicines.Include(m => m.Batches).FirstOrDefaultAsync(m => m.Id == 1);
                Assert.NotNull(med);
                Assert.Single(med.Batches);
                Assert.Equal(100, med.StockQuantity);
                
                var txs = await verifyCtx1.InventoryTransactions.ToListAsync();
                Assert.Single(txs);
                Assert.Equal(InventoryTransactionType.GoodsReceipt, txs[0].Type);
                Assert.Equal(100, txs[0].QuantityChange);
            }

            // 3. Action 2: Bác sĩ kê đơn (Prescribe -> Export via FEFO)
            var medicalRecordDto = new CreateMedicalRecordDto
            {
                AppointmentId = 1,
                Weight = 5.0m,
                Temperature = 38.0m,
                ClinicalSigns = "Sốt nhẹ",
                Diagnosis = "Nhiễm trùng hô hấp",
                TreatmentPlan = "Dùng kháng sinh 5 ngày",
                Prescriptions = new List<PrescriptionLineDto>
                {
                    new PrescriptionLineDto
                    {
                        MedicineId = 1,
                        Quantity = 10, // Kê 10 viên
                        Dosage = "1 viên",
                        Frequency = "2 lần/ngày",
                        DurationDays = 5,
                        Instruction = "Sau ăn"
                    }
                }
            };

            await _medicalRecordService.CreateMedicalRecordAsync(medicalRecordDto, doctorId);

            // Verify Export
            using (var verifyCtx2 = new ApplicationDbContext(_options))
            {
                var med = await verifyCtx2.Medicines.Include(m => m.Batches).FirstOrDefaultAsync(m => m.Id == 1);
                Assert.Equal(90, med!.StockQuantity); // 100 - 10 = 90
                
                var batch = med.Batches.First();
                Assert.Equal(90, batch.CurrentQuantity);

                var txs = await verifyCtx2.InventoryTransactions.OrderBy(t => t.TransactionDate).ToListAsync();
                Assert.Equal(2, txs.Count);
                Assert.Equal(InventoryTransactionType.PrescriptionDispense, txs[1].Type);
                Assert.Equal(-10, txs[1].QuantityChange);
            }

            // 4. Action 3: Xem báo cáo cảnh báo (GetExpiringMedicinesAsync)
            // Nhập 1 lô sắp hết hạn
            var importExpiringDto = new ImportMedicineDto
            {
                MedicineId = 1,
                BatchNumber = "LOT-EXPIRING",
                ManufactureDate = DateTime.UtcNow.AddMonths(-12),
                ExpiryDate = DateTime.UtcNow.AddDays(15), // Hết hạn trong 15 ngày
                Quantity = 50
            };
            await _medicineService.ImportMedicineAsync(importExpiringDto, adminId);

            var expiringReport = await _medicineService.GetExpiringMedicinesAsync(30); // Cảnh báo 30 ngày
            Assert.Single(expiringReport); // Nên tìm thấy LOT-EXPIRING
            Assert.Equal("LOT-EXPIRING", expiringReport.First().BatchNumber);
            Assert.Equal(50, expiringReport.First().CurrentQuantity);

            // 5. Action 4: Kiểm kê kho (Audit) - Chênh lệch
            var auditDto = new AuditMedicineDto
            {
                MedicineId = 1,
                BatchId = expiringReport.First().BatchId,
                ActualQuantity = 45, // Thất thoát 5 viên
                Notes = "Kiểm kê định kỳ thấy mất 5 viên"
            };
            await _medicineService.AuditMedicineBatchAsync(auditDto, adminId);

            using (var verifyCtx3 = new ApplicationDbContext(_options))
            {
                var batch = await verifyCtx3.MedicineBatches.FindAsync(auditDto.BatchId);
                Assert.Equal(45, batch!.CurrentQuantity);

                var txs = await verifyCtx3.InventoryTransactions.OrderByDescending(t => t.TransactionDate).FirstOrDefaultAsync();
                Assert.NotNull(txs);
                Assert.Equal(InventoryTransactionType.Adjustment, txs.Type);
                Assert.Equal(-5, txs.QuantityChange);
            }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
