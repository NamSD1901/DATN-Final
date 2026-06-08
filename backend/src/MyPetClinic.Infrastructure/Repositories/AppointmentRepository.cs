using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            return appointment;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(long id)
        {
            return await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Customer)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerIdAsync(Guid customerId)
        {
            return await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .Include(a => a.Invoice)
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
