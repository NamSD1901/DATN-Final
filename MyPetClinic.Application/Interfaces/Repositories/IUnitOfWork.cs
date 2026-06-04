using System;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Appointment> Appointments { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<Pet> Pets { get; }
        IGenericRepository<Service> Services { get; }
        IGenericRepository<Invoice> Invoices { get; }
        IGenericRepository<InvoiceItem> InvoiceItems { get; }
        IGenericRepository<MedicalRecord> MedicalRecords { get; }
        IGenericRepository<Medicine> Medicines { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
