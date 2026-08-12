using System;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Appointment> Appointments { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<Pet> Pets { get; }
        IGenericRepository<ServiceCategory> ServiceCategories { get; }
        IGenericRepository<Service> Services { get; }
        IGenericRepository<Invoice> Invoices { get; }
        IGenericRepository<InvoiceItem> InvoiceItems { get; }
        IGenericRepository<MedicalRecord> MedicalRecords { get; }
        IGenericRepository<Medicine> Medicines { get; }
        IGenericRepository<Prescription> Prescriptions { get; }
        IGenericRepository<PrescriptionItem> PrescriptionItems { get; }
        IGenericRepository<Vaccine> Vaccines { get; }
        IGenericRepository<VaccineBatch> VaccineBatches { get; }
        IGenericRepository<MedicineCategory> MedicineCategories { get; }
        IGenericRepository<MedicineBatch> MedicineBatches { get; }
        IGenericRepository<InventoryTransaction> InventoryTransactions { get; }
        IGenericRepository<VaccinationRecord> VaccinationRecords { get; }
        IGenericRepository<DoctorSchedule> DoctorSchedules { get; }
        IGenericRepository<BlockTime> BlockTimes { get; }
        IGenericRepository<Post> Posts { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<EmployeeProfile> EmployeeProfiles { get; }
        IGenericRepository<Invitation> Invitations { get; }
        IGenericRepository<ClinicOperatingDay> ClinicOperatingDays { get; }
        IGenericRepository<ClinicOperatingShift> ClinicOperatingShifts { get; }
        IGenericRepository<ClinicHoliday> ClinicHolidays { get; }
        IGenericRepository<PostCategory> PostCategories { get; }
        IGenericRepository<ScheduleProfile> ScheduleProfiles { get; }
        IGenericRepository<ScheduleProfileShift> ScheduleProfileShifts { get; }
        IGenericRepository<DoctorScheduleProfile> DoctorScheduleProfiles { get; }
        IGenericRepository<ScheduleException> ScheduleExceptions { get; }
        IGenericRepository<Offer> Offers { get; }
        IGenericRepository<OfferService> OfferServices { get; }
        IGenericRepository<OfferUsageLog> OfferUsageLogs { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel);
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
