using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(Guid doctorId, DateTime? date = null)
        {
            return await _appointmentRepository.GetAppointmentsByDoctorIdAsync(doctorId, date);
        }

        public async Task<bool> UpdateAppointmentStatusAsync(long appointmentId, string status)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return false;
            }

            appointment.Status = status;
            await _appointmentRepository.UpdateAsync(appointment);
            return true;
        }
        public async Task<bool> CancelAppointmentAsync(long appointmentId, string reason, string cancelledByRole)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return false;
            }

            appointment.Status = "cancelled";
            appointment.CancellationReason = reason;
            appointment.CancelledByRole = cancelledByRole;
            await _appointmentRepository.UpdateAsync(appointment);
            return true;
        }
    }
}
