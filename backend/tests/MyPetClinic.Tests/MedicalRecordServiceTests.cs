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
    public class MedicalRecordServiceTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly MedicalRecordService _service;

        public MedicalRecordServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(_options);
            _unitOfWork = new UnitOfWork(_context);
            _service = new MedicalRecordService(_unitOfWork);
        }

        [Fact]
        public async Task CreateMedicalRecord_ShouldCompleteAppointment_WhenSuccessful()
        {
            // Arrange
            var pet = new Pet { Id = 1, OwnerId = Guid.NewGuid(), Name = "Milo" };
            var doctorId = Guid.NewGuid();
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
                Name = "Paracetamol",
                StockQuantity = 10,
                SellPrice = 5000
            };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            var dto = new CreateMedicalRecordDto
            {
                AppointmentId = 1,
                Weight = 4.5m,
                Temperature = 38.5m,
                ClinicalSigns = "Sốt nhẹ",
                Diagnosis = "Cảm cúm",
                TreatmentPlan = "Uống thuốc giải sốt",
                Prescriptions = new List<PrescriptionLineDto>
                {
                    new PrescriptionLineDto
                    {
                        MedicineId = 1,
                        Quantity = 3,
                        Dosage = "1 viên",
                        Frequency = "2 lần/ngày",
                        DurationDays = 3,
                        Instruction = "Sau ăn"
                    }
                }
            };

            // Act
            var recordId = await _service.CreateMedicalRecordAsync(dto, doctorId);

            // Assert
            using (var verifyContext = new ApplicationDbContext(_options))
            {
                var record = await verifyContext.MedicalRecords.FindAsync(recordId);
                Assert.NotNull(record);
                Assert.Equal("Cảm cúm", record.Diagnosis);
                
                var updatedAppointment = await verifyContext.Appointments.FindAsync(1L);
                Assert.Equal("completed", updatedAppointment!.Status);

                var updatedMedicine = await verifyContext.Medicines.FindAsync(1L);
                Assert.Equal(7, updatedMedicine!.StockQuantity); // 10 - 3 = 7
            }
        }

        [Fact]
        public async Task CreateMedicalRecord_ShouldRollbackAllDeductions_WhenAnyMedicineOutOfStock()
        {
            // Arrange
            var pet = new Pet { Id = 2, OwnerId = Guid.NewGuid(), Name = "Milo2" };
            var doctorId = Guid.NewGuid();
            var appointment = new Appointment
            {
                Id = 2,
                PetId = 2,
                CustomerId = Guid.NewGuid(),
                DoctorId = doctorId,
                ServiceId = 1,
                Status = "in_progress",
                AppointmentDate = DateTime.UtcNow
            };

            var medicineA = new Medicine
            {
                Id = 2,
                Name = "Medicine A",
                StockQuantity = 10,
                SellPrice = 5000
            };
            var medicineB = new Medicine
            {
                Id = 3,
                Name = "Medicine B",
                StockQuantity = 2, // Only 2 in stock
                SellPrice = 10000
            };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Medicines.AddRange(medicineA, medicineB);
            await _context.SaveChangesAsync();

            var dto = new CreateMedicalRecordDto
            {
                AppointmentId = 2,
                ClinicalSigns = "Ho khan",
                Diagnosis = "Viêm phế quản",
                TreatmentPlan = "Kê đơn kết hợp",
                Prescriptions = new List<PrescriptionLineDto>
                {
                    new PrescriptionLineDto { MedicineId = 2, Quantity = 5, Dosage = "1 viên" }, // Medicine A (enough stock)
                    new PrescriptionLineDto { MedicineId = 3, Quantity = 5, Dosage = "1 viên" }  // Medicine B (out of stock, requests 5, has 2)
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateMedicalRecordAsync(dto, doctorId)
            );

            // Verify rollback using a separate clean DbContext context instance
            using (var verifyContext = new ApplicationDbContext(_options))
            {
                var medA = await verifyContext.Medicines.FindAsync(2L);
                var medB = await verifyContext.Medicines.FindAsync(3L);
                Assert.Equal(10, medA!.StockQuantity);
                Assert.Equal(2, medB!.StockQuantity);

                // Verify appointment status did not change to completed
                var appt = await verifyContext.Appointments.FindAsync(2L);
                Assert.Equal("in_progress", appt!.Status);
            }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
