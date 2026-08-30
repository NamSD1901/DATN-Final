using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyPetClinic.Application.Services
{
    public class DoctorAppointmentService : IDoctorAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVaccinationScheduleChecker _vaccinationScheduleChecker;
        private readonly INotificationService _notificationService;
        private readonly IEmailQueue _emailQueue;
        private readonly IAppointmentService _appointmentService;

        public DoctorAppointmentService(IUnitOfWork unitOfWork, IVaccinationScheduleChecker vaccinationScheduleChecker, INotificationService notificationService, IEmailQueue emailQueue, IAppointmentService appointmentService)
        {
            _unitOfWork = unitOfWork;
            _vaccinationScheduleChecker = vaccinationScheduleChecker;
            _notificationService = notificationService;
            _emailQueue = emailQueue;
            _appointmentService = appointmentService;
        }

        private bool IsTransientConflict(Exception ex)
        {
            var msg = ex.ToString().ToLower();
            return msg.Contains("serialization") || msg.Contains("deadlock") || msg.Contains("conflict") || msg.Contains("transaction failed");
        }

        public async Task<System.Collections.Generic.IEnumerable<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId)
        {
            System.Linq.Expressions.Expression<Func<Appointment, bool>> predicate;
            if (doctorId.HasValue)
            {
                predicate = a => a.AppointmentDate >= start && a.AppointmentDate <= end && a.DoctorId == doctorId.Value && a.Status != "pending";
            }
            else
            {
                predicate = a => a.AppointmentDate >= start && a.AppointmentDate <= end && a.Status != "pending";
            }

            var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(
                predicate,
                a => a.Pet!, a => a.Customer!, a => a.Doctor!, a => a.Service!
            );

            var events = new System.Collections.Generic.List<CalendarEventDto>();
            foreach (var a in appointments)
            {
                // Determine color based on status
                var color = "#6c757d"; // default gray
                if (a.Status == "pending") color = "#ffc107"; // yellow
                else if (a.Status == "confirmed") color = "#0dcaf0"; // cyan
                else if (a.Status == "waiting") color = "#0d6efd"; // blue (Kanban queue)
                else if (a.Status == "in_progress") color = "#fd7e14"; // orange
                else if (a.Status == "ready_to_pay" || a.Status == "completed") color = "#198754"; // green
                else if (a.Status == "cancelled") color = "#dc3545"; // red

                var startDateTime = a.AppointmentDate.Date.Add(a.StartTime);
                if (startDateTime.TimeOfDay == TimeSpan.Zero)
                {
                    // If time is 00:00:00 UTC (created from month view without setting time), 
                    // default to 08:00 Local Time (01:00 UTC for VN)
                    startDateTime = startDateTime.AddHours(1);
                }

                events.Add(new CalendarEventDto
                {
                    Id = a.Id.ToString(),
                    Title = $"{a.Pet?.Name} - {a.Customer?.FullName}",
                    Start = startDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    End = startDateTime.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ss"), // Default 30 min block
                    Color = color,
                    AllDay = false,
                    ExtendedProps = new
                    {
                        status = a.Status,
                        petId = a.PetId,
                        petName = a.Pet?.Name,
                        species = a.Pet?.Species,
                        breed = a.Pet?.Breed,
                        weight = a.Pet?.Weight,
                        isAggressive = a.Pet?.IsAggressive ?? false,
                        customerId = a.CustomerId,
                        customerName = a.Customer?.FullName,
                        phone = a.Customer?.Phone,
                        symptom = a.Symptom,
                        note = a.Note,
                        doctorId = a.DoctorId,
                        doctorName = a.Doctor?.FullName,
                        serviceName = a.Service?.Name,
                        qrToken = a.QrToken
                    }
                });
            }

            return events;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(long id, string status, string? reason = null)
        {
            return await _appointmentService.UpdateAppointmentStatusAsync(id, status, reason);
        }

        public async Task<AppointmentDetailDto?> GetAppointmentDetailAsync(long id)
        {
            var a = _unitOfWork.Appointments.Query().IgnoreQueryFilters()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.PetId,
                    PetName = x.Pet != null ? x.Pet.Name : null,
                    Species = x.Pet != null ? x.Pet.Species : null,
                    Breed = x.Pet != null ? x.Pet.Breed : null,
                    Weight = x.Pet != null ? x.Pet.Weight : null,
                    IsAggressive = x.Pet != null ? (bool?)x.Pet.IsAggressive : null,
                    x.CustomerId,
                    CustomerName = x.Customer != null ? x.Customer.FullName : null,
                    CustomerPhone = x.Customer != null ? x.Customer.Phone : null,
                    x.ServiceId,
                    ServiceName = x.Service != null ? x.Service.Name : null,
                    ServicePrice = x.Service != null ? (decimal?)x.Service.Price : null,
                    x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    x.AppointmentDate,
                    x.StartTime,
                    x.Symptom,
                    x.Note,
                    x.Status,
                    x.QrToken,
                    x.Type,
                    x.IsSystemGenerated,
                    x.ReferenceRecordId,
                    x.VaccineId,
                    VaccineName = x.Vaccine != null ? x.Vaccine.Name : null
                })
                .FirstOrDefault();

            if (a == null) return null;

            var mapped = new AppointmentDetailDto
            {
                Id = a.Id,
                PetId = a.PetId,
                PetName = a.PetName,
                Species = a.Species,
                Breed = a.Breed,
                Weight = a.Weight,
                IsAggressive = a.IsAggressive ?? false,
                CustomerId = a.CustomerId,
                CustomerName = a.CustomerName,
                CustomerPhone = a.CustomerPhone,
                ServiceId = a.ServiceId,
                ServiceName = a.ServiceName,
                ServicePrice = a.ServicePrice,
                DoctorId = a.DoctorId,
                DoctorName = a.DoctorName,
                AppointmentDate = a.AppointmentDate.ToLocalTime().Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                Symptom = a.Symptom,
                Note = a.Note,
                Status = a.Status,
                QrToken = a.QrToken,
                Type = a.Type,
                IsSystemGenerated = a.IsSystemGenerated,
                ReferenceRecordId = a.ReferenceRecordId,
                VaccineId = a.VaccineId,
                VaccineName = a.VaccineName
            };

            return await Task.FromResult(mapped);
        }

        public async Task<IEnumerable<AppointmentDetailDto>> GetPetAppointmentsAsync(long petId)
        {
            var rawList = _unitOfWork.Appointments.Query().IgnoreQueryFilters()
                .Where(a => a.PetId == petId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .ToList();

            var mapped = await MapToDetailDtoAsync(rawList);

            return mapped;
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid CustomerId)
        {
            var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.Id == petId && p.CustomerId == CustomerId);
            if (pet == null)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin bệnh án của thú cưng này.");
            }

            var records = _unitOfWork.MedicalRecords.Query().IgnoreQueryFilters()
                .Where(mr => mr.Appointment != null && mr.Appointment.PetId == petId)
                .OrderByDescending(mr => mr.CreatedAt)
                .Select(mr => new
                {
                    mr.Id,
                    mr.AppointmentId,
                    PetId = mr.PetId,
                    PetName = (mr.Appointment != null && mr.Appointment.Pet != null) ? mr.Appointment.Pet.Name : string.Empty,
                    VisitDate = mr.CreatedAt,
                    RecordType = mr.RecordType,
                    MedicalHistory = mr.MedicalHistory ?? string.Empty,
                    Diagnosis = mr.Diagnosis ?? string.Empty,
                    TreatmentPlan = mr.TreatmentPlan ?? string.Empty,
                    DoctorName = mr.Doctor != null ? mr.Doctor.FullName : string.Empty,
                    DoctorId = mr.DoctorId.ToString(),
                    mr.Weight,
                    mr.Temperature,
                    ClinicalSigns = mr.ClinicalSigns ?? string.Empty,
                    DoctorNotes = mr.DoctorNotes ?? string.Empty,
                    mr.FollowUpDate,
                    InvoiceId = mr.Appointment != null && mr.Appointment.Invoice != null ? (long?)mr.Appointment.Invoice.Id : null,
                    InvoiceStatus = mr.Appointment != null && mr.Appointment.Invoice != null ? mr.Appointment.Invoice.PaymentStatus : null,
                    InvoiceTotalAmount = mr.Appointment != null && mr.Appointment.Invoice != null ? (decimal?)mr.Appointment.Invoice.TotalAmount : null,
                    PrescribedMedicines = mr.Prescriptions
                        .SelectMany(p => p.PrescriptionItems)
                        .Where(pi => pi.Medicine != null)
                        .Select(pi => new PrescribedMedicineDto
                        {
                            MedicineName = pi.Medicine!.Name,
                            Dosage = pi.Dosage,
                            Frequency = pi.Frequency,
                            DurationDays = pi.DurationDays,
                            Quantity = pi.Quantity,
                            Instruction = pi.Instruction
                        })
                        .ToList()
                })
                .ToList();

            var mapped = records.Select(mr => new MedicalRecordDto
            {
                RecordId = mr.Id,
                AppointmentId = mr.AppointmentId,
                PetId = pet.Id,
                PetName = mr.PetName ?? string.Empty,
                VisitDate = mr.VisitDate,
                RecordType = mr.RecordType,
                MedicalHistory = mr.MedicalHistory,
                Diagnosis = mr.Diagnosis,
                TreatmentPlan = mr.TreatmentPlan,
                DoctorName = mr.DoctorName ?? string.Empty,
                DoctorId = mr.DoctorId,
                Weight = mr.Weight,
                Temperature = mr.Temperature,
                ClinicalSigns = mr.ClinicalSigns,
                DoctorNotes = mr.DoctorNotes,
                FollowUpDate = mr.FollowUpDate,
                PrescribedMedicines = mr.PrescribedMedicines,
                InvoiceId = mr.InvoiceId,
                InvoiceStatus = mr.InvoiceStatus,
                InvoiceTotalAmount = mr.InvoiceTotalAmount
            }).ToList();

            var vacRecords = _unitOfWork.VaccinationRecords.Query().IgnoreQueryFilters()
                .Where(v => v.PetId == petId)
                .Select(v => new
                {
                    v.Id,
                    v.AppointmentId,
                    v.DoctorId,
                    DoctorName = v.Doctor != null ? v.Doctor.FullName : string.Empty,
                    v.CreatedAt,
                    v.Weight,
                    v.Temperature,
                    v.ClinicalAssessment,
                    v.ReasonForVisit,
                    v.DoctorRemarks,
                    v.NextDueDate,
                    VaccineName = v.Vaccine != null ? v.Vaccine.Name : string.Empty,
                    v.Dose,
                    InvoiceId = v.Appointment != null && v.Appointment.Invoice != null ? (long?)v.Appointment.Invoice.Id : null,
                    InvoiceStatus = v.Appointment != null && v.Appointment.Invoice != null ? v.Appointment.Invoice.PaymentStatus : null,
                    InvoiceTotalAmount = v.Appointment != null && v.Appointment.Invoice != null ? (decimal?)v.Appointment.Invoice.TotalAmount : null
                })
                .ToList();

            var vacMapped = vacRecords.Select(v => new MedicalRecordDto
            {
                RecordId = v.Id + 1000000, 
                AppointmentId = v.AppointmentId ?? 0,
                PetId = pet.Id,
                PetName = pet.Name ?? string.Empty,
                VisitDate = v.CreatedAt,
                RecordType = "Vaccination",
                MedicalHistory = v.ReasonForVisit ?? "Tiêm phòng",
                Diagnosis = v.VaccineName ?? string.Empty,
                TreatmentPlan = v.Dose != null ? $"Tiêm {v.Dose} ml {v.VaccineName}" : $"Tiêm {v.VaccineName}",
                DoctorName = v.DoctorName ?? string.Empty,
                DoctorId = v.DoctorId.ToString(),
                Weight = v.Weight,
                Temperature = v.Temperature ?? 0m,
                ClinicalSigns = v.ClinicalAssessment ?? string.Empty,
                DoctorNotes = v.DoctorRemarks ?? string.Empty,
                FollowUpDate = v.NextDueDate,
                PrescribedMedicines = new List<PrescribedMedicineDto>(),
                InvoiceId = v.InvoiceId,
                InvoiceStatus = v.InvoiceStatus,
                InvoiceTotalAmount = v.InvoiceTotalAmount
            }).ToList();

            var combinedList = mapped.Concat(vacMapped).OrderByDescending(r => r.VisitDate).ToList();

            return await Task.FromResult(combinedList);
        }

        private async Task<List<AppointmentDetailDto>> MapToDetailDtoAsync(IEnumerable<Appointment> appointments)
        {
            if (!appointments.Any()) return new List<AppointmentDetailDto>();

            var petIds = appointments.Select(a => a.PetId).Distinct().ToList();
            var pets = await _unitOfWork.Pets.Query().IgnoreQueryFilters().Where(p => petIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            var customerIds = appointments.Select(a => a.CustomerId).Distinct().ToList();
            var customers = await _unitOfWork.Customers.Query().IgnoreQueryFilters().Where(c => customerIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);

            var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
            var doctors = await _unitOfWork.Users.Query().IgnoreQueryFilters().Where(u => doctorIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

            var serviceIds = appointments.Select(a => a.ServiceId).Distinct().ToList();
            var services = await _unitOfWork.Services.Query().IgnoreQueryFilters().Where(s => serviceIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

            var vaccineIds = appointments.Where(a => a.VaccineId.HasValue).Select(a => a.VaccineId!.Value).Distinct().ToList();
            var vaccines = await _unitOfWork.Vaccines.Query().IgnoreQueryFilters().Where(v => vaccineIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id);

            var appointmentIds = appointments.Select(a => a.Id).ToList();
            var invoices = await _unitOfWork.Invoices.Query().IgnoreQueryFilters().Where(i => appointmentIds.Contains(i.AppointmentId)).ToDictionaryAsync(i => i.AppointmentId);

            var result = new List<AppointmentDetailDto>();
            foreach (var a in appointments)
            {
                pets.TryGetValue(a.PetId, out var pet);
                customers.TryGetValue(a.CustomerId, out var customer);
                doctors.TryGetValue(a.DoctorId, out var doctor);
                services.TryGetValue(a.ServiceId, out var service);
                Vaccine? vaccine = null;
                if (a.VaccineId.HasValue) vaccines.TryGetValue(a.VaccineId.Value, out vaccine);
                invoices.TryGetValue(a.Id, out var invoice);

                result.Add(new AppointmentDetailDto
                {
                    Id = a.Id,
                    PetId = a.PetId,
                    PetName = pet?.Name ?? "Thú cưng đã xóa",
                    Species = pet?.Species ?? "Không rõ",
                    Breed = pet?.Breed,
                    Weight = pet?.Weight,
                    IsAggressive = pet?.IsAggressive ?? false,
                    CustomerId = a.CustomerId,
                    CustomerName = customer?.FullName ?? "Khách hàng",
                    CustomerPhone = customer?.Phone,
                    ServiceId = a.ServiceId,
                    ServiceName = service?.Name ?? "Dịch vụ đã xóa",
                    ServicePrice = service?.Price,
                    DoctorId = a.DoctorId,
                    DoctorName = doctor?.FullName ?? "Bác sĩ đã nghỉ",
                    AppointmentDate = a.AppointmentDate.ToLocalTime().Date.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                    Symptom = a.Symptom,
                    Note = a.Note,
                    Status = a.Status,
                    QrToken = a.QrToken,
                    Type = a.Type,
                    IsSystemGenerated = a.IsSystemGenerated,
                    ReferenceRecordId = a.ReferenceRecordId,
                    InvoiceId = invoice?.Id,
                    InvoiceStatus = invoice?.PaymentStatus,
                    InvoiceTotalAmount = invoice?.TotalAmount,
                    VaccineId = a.VaccineId,
                    VaccineName = vaccine?.Name
                });
            }

            return result;
        }

    }
}
