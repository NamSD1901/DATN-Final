using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        public IGenericRepository<Appointment> Appointments { get; private set; }
        public IGenericRepository<User> Users { get; private set; }
        public IGenericRepository<Customer> Customers { get; private set; }
        public IGenericRepository<Role> Roles { get; private set; }
        public IGenericRepository<Pet> Pets { get; private set; }
        public IGenericRepository<ServiceCategory> ServiceCategories { get; private set; }
        public IGenericRepository<Service> Services { get; private set; }
        public IGenericRepository<Invoice> Invoices { get; private set; }
        public IGenericRepository<InvoiceItem> InvoiceItems { get; private set; }
        public IGenericRepository<MedicalRecord> MedicalRecords { get; private set; }
        public IGenericRepository<Medicine> Medicines { get; private set; }
        public IGenericRepository<Prescription> Prescriptions { get; private set; }
        public IGenericRepository<PrescriptionItem> PrescriptionItems { get; private set; }
        public IGenericRepository<Vaccine> Vaccines { get; private set; }
        public IGenericRepository<VaccineBatch> VaccineBatches { get; private set; }
        public IGenericRepository<MedicineCategory> MedicineCategories { get; private set; }
        public IGenericRepository<MedicineBatch> MedicineBatches { get; private set; }
        public IGenericRepository<InventoryTransaction> InventoryTransactions { get; private set; }
        public IGenericRepository<VaccinationRecord> VaccinationRecords { get; private set; }
        public IGenericRepository<DoctorSchedule> DoctorSchedules { get; private set; }
        public IGenericRepository<BlockTime> BlockTimes { get; private set; }
        public IGenericRepository<Post> Posts { get; private set; }
        public IGenericRepository<Review> Reviews { get; private set; }
        public IGenericRepository<Notification> Notifications { get; private set; }
        public IGenericRepository<EmployeeProfile> EmployeeProfiles { get; private set; }
        public IGenericRepository<Invitation> Invitations { get; private set; }
        public IGenericRepository<ClinicOperatingDay> ClinicOperatingDays { get; private set; }
        public IGenericRepository<ClinicOperatingShift> ClinicOperatingShifts { get; private set; }
        public IGenericRepository<ClinicHoliday> ClinicHolidays { get; private set; }
        public IGenericRepository<PostCategory> PostCategories { get; private set; }
        public IGenericRepository<ScheduleProfile> ScheduleProfiles { get; private set; }
        public IGenericRepository<ScheduleProfileShift> ScheduleProfileShifts { get; private set; }
        public IGenericRepository<DoctorScheduleProfile> DoctorScheduleProfiles { get; private set; }
        public IGenericRepository<ScheduleException> ScheduleExceptions { get; private set; }
        public IGenericRepository<Offer> Offers { get; private set; }
        public IGenericRepository<OfferService> OfferServices { get; private set; }
        public IGenericRepository<OfferUsageLog> OfferUsageLogs { get; private set; }
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Appointments = new GenericRepository<Appointment>(_context);
            Users = new GenericRepository<User>(_context);
            Customers = new GenericRepository<Customer>(_context);
            Roles = new GenericRepository<Role>(_context);
            Pets = new GenericRepository<Pet>(_context);
            ServiceCategories = new GenericRepository<ServiceCategory>(_context);
            Services = new GenericRepository<Service>(_context);
            Invoices = new GenericRepository<Invoice>(_context);
            InvoiceItems = new GenericRepository<InvoiceItem>(_context);
            MedicalRecords = new GenericRepository<MedicalRecord>(_context);
            Medicines = new GenericRepository<Medicine>(_context);
            Prescriptions = new GenericRepository<Prescription>(_context);
            PrescriptionItems = new GenericRepository<PrescriptionItem>(_context);
            Vaccines = new GenericRepository<Vaccine>(_context);
            VaccineBatches = new GenericRepository<VaccineBatch>(_context);
            MedicineCategories = new GenericRepository<MedicineCategory>(_context);
            MedicineBatches = new GenericRepository<MedicineBatch>(_context);
            InventoryTransactions = new GenericRepository<InventoryTransaction>(_context);
            VaccinationRecords = new GenericRepository<VaccinationRecord>(_context);
            DoctorSchedules = new GenericRepository<DoctorSchedule>(_context);
            BlockTimes = new GenericRepository<BlockTime>(_context);
            Posts = new GenericRepository<Post>(_context);
            Reviews = new GenericRepository<Review>(_context);
            Notifications = new GenericRepository<Notification>(_context);
            EmployeeProfiles = new GenericRepository<EmployeeProfile>(_context);
            Invitations = new GenericRepository<Invitation>(_context);
            ClinicOperatingDays = new GenericRepository<ClinicOperatingDay>(_context);
            ClinicOperatingShifts = new GenericRepository<ClinicOperatingShift>(_context);
            ClinicHolidays = new GenericRepository<ClinicHoliday>(_context);
            PostCategories = new GenericRepository<PostCategory>(_context);
            ScheduleProfiles = new GenericRepository<ScheduleProfile>(_context);
            ScheduleProfileShifts = new GenericRepository<ScheduleProfileShift>(_context);
            DoctorScheduleProfiles = new GenericRepository<DoctorScheduleProfile>(_context);
            ScheduleExceptions = new GenericRepository<ScheduleException>(_context);
            Offers = new GenericRepository<Offer>(_context);
            OfferServices = new GenericRepository<OfferService>(_context);
            OfferUsageLogs = new GenericRepository<OfferUsageLog>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel)
        {
            if (_currentTransaction != null)
            {
                return;
            }
            _currentTransaction = await _context.Database.BeginTransactionAsync(isolationLevel);
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                }
                _context.ChangeTracker.Clear();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
