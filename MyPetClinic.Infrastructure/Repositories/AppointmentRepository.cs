using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;

namespace MyPetClinic.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(Guid doctorId, DateTime? date = null)
        {
            var query = _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Customer)
                .Where(a => a.DoctorId == doctorId);

            if (date.HasValue)
            {
                var targetDateUtc = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
                var nextDateUtc = targetDateUtc.AddDays(1);
                
                query = query.Where(a => a.AppointmentDate >= targetDateUtc && a.AppointmentDate < nextDateUtc);
            }

            return await query.OrderBy(a => a.AppointmentDate)
                              .ThenBy(a => a.StartTime)
                              .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(long id)
        {
            return await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }
    }
}
