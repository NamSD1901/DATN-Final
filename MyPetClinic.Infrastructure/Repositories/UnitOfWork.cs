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
        public IGenericRepository<Role> Roles { get; private set; }
        public IGenericRepository<Pet> Pets { get; private set; }
        public IGenericRepository<Service> Services { get; private set; }
        public IGenericRepository<Invoice> Invoices { get; private set; }
        public IGenericRepository<InvoiceItem> InvoiceItems { get; private set; }
        public IGenericRepository<MedicalRecord> MedicalRecords { get; private set; }
        public IGenericRepository<Medicine> Medicines { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Appointments = new GenericRepository<Appointment>(_context);
            Users = new GenericRepository<User>(_context);
            Roles = new GenericRepository<Role>(_context);
            Pets = new GenericRepository<Pet>(_context);
            Services = new GenericRepository<Service>(_context);
            Invoices = new GenericRepository<Invoice>(_context);
            InvoiceItems = new GenericRepository<InvoiceItem>(_context);
            MedicalRecords = new GenericRepository<MedicalRecord>(_context);
            Medicines = new GenericRepository<Medicine>(_context);
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
