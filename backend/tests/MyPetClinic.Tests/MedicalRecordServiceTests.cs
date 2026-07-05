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
            var medicineRepo = new MedicineRepository(_context);
            var batchRepo = new MedicineBatchRepository(_context);
            var transactionRepo = new InventoryTransactionRepository(_context);
            var medicineService = new MedicineService(_unitOfWork, medicineRepo, batchRepo, transactionRepo);
            _service = new MedicalRecordService(_unitOfWork, medicineService);
        }

        [Fact]
        public async Task CreateMedicalRecord_ShouldCompleteAppointment_WhenSuccessful()
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
                Status = "in_progress",
                AppointmentDate = DateTime.UtcNow
            };

            var medicine = new Medicine
            {
                Id = 1,
                Name = "Paracetamol",
                SellPrice = 5000
            };
            var batch = new MedicineBatch
            {
                Id = 1,
                MedicineId = 1,
                BatchNumber = "B1",
                CurrentQuantity = 10,
                InitialQuantity = 10,
                ExpiryDate = DateTime.UtcNow.AddYears(1)
            };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Medicines.Add(medicine);
            _context.MedicineBatches.Add(batch);
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

                var updatedBatch = await verifyContext.MedicineBatches.FindAsync(1L);
                Assert.Equal(7, updatedBatch!.CurrentQuantity); // 10 - 3 = 7
            }
        }

        [Fact]
        public async Task CreateMedicalRecord_ShouldRollbackAllDeductions_WhenAnyMedicineOutOfStock()
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
                Status = "in_progress",
                AppointmentDate = DateTime.UtcNow
            };

            var medicineA = new Medicine { Id = 2, Name = "Medicine A", SellPrice = 5000 };
            var batchA = new MedicineBatch { Id = 2, MedicineId = 2, BatchNumber = "BA", CurrentQuantity = 10, ExpiryDate = DateTime.UtcNow.AddYears(1) };
            
            var medicineB = new Medicine { Id = 3, Name = "Medicine B", SellPrice = 10000 };
            var batchB = new MedicineBatch { Id = 3, MedicineId = 3, BatchNumber = "BB", CurrentQuantity = 2, ExpiryDate = DateTime.UtcNow.AddYears(1) };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Medicines.AddRange(medicineA, medicineB);
            _context.MedicineBatches.AddRange(batchA, batchB);
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
                var bA = await verifyContext.MedicineBatches.FindAsync(2L);
                var bB = await verifyContext.MedicineBatches.FindAsync(3L);
                Assert.Equal(10, bA!.CurrentQuantity);
                Assert.Equal(2, bB!.CurrentQuantity);

                // Verify appointment status did not change to completed
                var appt = await verifyContext.Appointments.FindAsync(2L);
                Assert.Equal("in_progress", appt!.Status);
            }
        }

        [Fact]
        public async Task CreateMedicalRecord_ShouldCreateFollowUpAppointment_WhenHasFollowUpIsTrue()
        {
            // Arrange
            var pet = new Pet { Id = 3, CustomerId = Guid.NewGuid(), Name = "Milo3" };
            var doctorId = Guid.NewGuid();
            var appointment = new Appointment
            {
                Id = 3,
                PetId = 3,
                CustomerId = pet.CustomerId,
                DoctorId = doctorId,
                ServiceId = 1,
                Status = "in_progress",
                AppointmentDate = DateTime.UtcNow
            };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var dto = new MedicalRecordSoapRequestDto
            {
                AppointmentId = 3,
                PetId = 3,
                Subjective = new SubjectiveDto { ChiefComplaint = "Sốt nhẹ" },
                Assessment = new AssessmentDto { DefinitiveDiagnosis = "Cảm cúm" },
                Plan = new PlanDto 
                { 
                    CreateFollowUpAppointment = true,
                    FollowUpType = "FollowUp",
                    FollowUpDate = DateTime.UtcNow.AddDays(7),
                    FollowUpNote = "Kiểm tra lại sau 1 tuần"
                }
            };

            // Act
            var recordId = await _service.CreateSoapMedicalRecordAsync(dto, doctorId);

            // Assert
            using (var verifyContext = new ApplicationDbContext(_options))
            {
                var record = await verifyContext.MedicalRecords.FindAsync(recordId);
                Assert.NotNull(record);
                
                var systemAppt = await verifyContext.Appointments
                    .FirstOrDefaultAsync(a => a.Type == "FollowUp" && a.ReferenceRecordId == recordId);
                
                Assert.NotNull(systemAppt);
                Assert.Equal(3, systemAppt!.PetId);
                Assert.Equal("pending", systemAppt.Status);
                Assert.Equal("Kiểm tra lại sau 1 tuần", systemAppt.Note);
                Assert.True(systemAppt.IsSystemGenerated);
                Assert.Equal(dto.Plan.FollowUpDate.Value.Date, systemAppt.AppointmentDate.Date);
            }
        }

        [Fact]
        public async Task CreateMedicalRecord_ShouldCreateRevaccinationAppointment_WhenHasRevaccinationIsTrue()
        {
            // Arrange
            var pet = new Pet { Id = 4, CustomerId = Guid.NewGuid(), Name = "Milo4" };
            var doctorId = Guid.NewGuid();
            var appointment = new Appointment
            {
                Id = 4,
                PetId = 4,
                CustomerId = pet.CustomerId,
                DoctorId = doctorId,
                ServiceId = 1,
                Status = "in_progress",
                AppointmentDate = DateTime.UtcNow
            };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var dto = new MedicalRecordSoapRequestDto
            {
                AppointmentId = 4,
                PetId = 4,
                Subjective = new SubjectiveDto { ChiefComplaint = "Khỏe mạnh" },
                Assessment = new AssessmentDto { DefinitiveDiagnosis = "Bình thường" },
                Plan = new PlanDto 
                {
                    CreateFollowUpAppointment = true,
                    FollowUpType = "Revaccination",
                    FollowUpDate = DateTime.UtcNow.AddMonths(1),
                    FollowUpNote = "Tiêm dại"
                }
            };

            // Act
            var recordId = await _service.CreateSoapMedicalRecordAsync(dto, doctorId);

            // Assert
            using (var verifyContext = new ApplicationDbContext(_options))
            {
                var record = await verifyContext.MedicalRecords.FindAsync(recordId);
                Assert.NotNull(record);
                
                var systemAppt = await verifyContext.Appointments
                    .FirstOrDefaultAsync(a => a.Type == "Revaccination" && a.ReferenceRecordId == recordId);
                
                Assert.NotNull(systemAppt);
                Assert.Equal(4, systemAppt!.PetId);
                Assert.Equal("pending", systemAppt.Status);
                Assert.Equal("Tiêm dại", systemAppt.Note);
                Assert.True(systemAppt.IsSystemGenerated);
                Assert.Equal(dto.Plan.FollowUpDate.Value.Date, systemAppt.AppointmentDate.Date);
            }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
