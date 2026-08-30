using MyPetClinic.Application.DTOs;
using MyPetClinic.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyPetClinic.Application.Services
{
    public partial class ReceptionistAppointmentService
    {
        public async Task<long> CreateWalkInAppointmentAsync(WalkInRequestDto dto, Guid createdBy)
        {
            var utcNow = DateTime.UtcNow;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTimeZone);
            var appointmentDate = vnTime;

            var strategy = _unitOfWork.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                int retryCount = 3;
                for (int i = 0; i < retryCount; i++)
                {
                    await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                    try
                    {
                        // DoctorSchedule uses UTC midnight of the target date
                        var targetDateStart = DateTime.SpecifyKind(vnTime.Date, DateTimeKind.Utc);

                        // 1. Resolve Customer
                        var customer = _unitOfWork.Customers.Query()
                            .FirstOrDefault(c => c.Phone == dto.Phone && c.DeletedAt == null);

                        if (customer == null)
                        {
                            customer = new Customer
                            {
                                Id = Guid.NewGuid(),
                                CustomerCode = "CUS-" + DateTime.UtcNow.ToString("yyyyMMdd") + new Random().Next(100, 999).ToString(),
                                FullName = dto.FullName,
                                Phone = dto.Phone,
                                HasAccount = false,
                                Status = "Active",
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.Customers.AddAsync(customer);
                        }

                        // 2. Resolve Pet
                        var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.CustomerId == customer.Id && p.Name == dto.PetName && p.DeletedAt == null);
                        if (pet == null)
                        {
                            pet = new Pet
                            {
                                Customer = customer,
                                Name = dto.PetName,
                                Species = dto.Species ?? "Khác",
                                Weight = (decimal?)dto.Weight,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.Pets.AddAsync(pet);
                        }

                        // 3. Resolve Doctor (Auto-Assign)
                        var targetRole = "";
                        var serviceEntity = _unitOfWork.Services.Query()
                            .Where(s => s.Id == dto.ServiceId)
                            .Select(s => new { CategoryName = s.Category != null ? s.Category.Name : null })
                            .FirstOrDefault();

                        if (serviceEntity != null && serviceEntity.CategoryName != null)
                        {
                            if (serviceEntity.CategoryName.Equals("Khám bệnh", StringComparison.OrdinalIgnoreCase))
                                targetRole = "clinical_doctor";
                            else if (serviceEntity.CategoryName.Equals("Tiêm phòng", StringComparison.OrdinalIgnoreCase))
                                targetRole = "vaccination_doctor";
                        }

                        var appointmentTime = appointmentDate.TimeOfDay;

                        // Find doctors on duty
                        var doctorsWithSchedules = _unitOfWork.DoctorSchedules.Query()
                            .Where(s => s.WorkDate == targetDateStart && s.IsAvailable && s.Doctor != null && s.Doctor.IsActive == true
                                        && (string.IsNullOrEmpty(targetRole) || (s.Doctor.Role != null && s.Doctor.Role.Name.ToLower() == targetRole)))
                            .ToList();

                        var doctorsList = doctorsWithSchedules
                            .Where(s => appointmentTime >= s.StartTime && appointmentTime <= s.EndTime)
                            .Select(s => s.DoctorId)
                            .Distinct()
                            .ToList();

                        if (!doctorsList.Any())
                        {
                            throw new InvalidOperationException("Hiện tại không có bác sĩ nào đang trực cho dịch vụ này.");
                        }

                        // Check who is free
                        var allAptsForDoctors = _unitOfWork.Appointments.Query()
                            .Where(a => a.Status != "cancelled" && a.Status != "completed"
                                        && a.AppointmentDate == targetDateStart
                                        && doctorsList.Contains(a.DoctorId))
                            .Select(a => new { a.DoctorId, a.StartTime })
                            .ToList();

                        var busyDoctorIds = allAptsForDoctors
                            .Where(a => Math.Abs((a.StartTime - appointmentTime).TotalMinutes) < 30)
                            .Select(a => a.DoctorId)
                            .Distinct()
                            .ToList();

                        var targetDateEnd = targetDateStart.AddDays(1);
                        var blockTimesForDoctors = _unitOfWork.BlockTimes.Query()
                            .Where(b => doctorsList.Contains(b.DoctorId) 
                                     && b.StartTime < new DateTimeOffset(targetDateEnd)
                                     && b.EndTime > new DateTimeOffset(targetDateStart))
                            .ToList();

                        var blockedDoctorIds = blockTimesForDoctors
                            .Where(b => {
                                var blockStart = b.StartTime.LocalDateTime;
                                var blockEnd = b.EndTime.LocalDateTime;
                                var apptEnd = appointmentDate.AddMinutes(30);
                                return appointmentDate < blockEnd && apptEnd > blockStart;
                            })
                            .Select(b => b.DoctorId)
                            .Distinct()
                            .ToList();

                        var availableDoctors = doctorsList.Except(busyDoctorIds).Except(blockedDoctorIds).ToList();
                        
                        Guid finalDoctorId = Guid.Empty;
                        if (availableDoctors.Any())
                        {
                            // Pick a random free doctor, or the one with least appointments today
                            finalDoctorId = availableDoctors.OrderBy(d => allAptsForDoctors.Count(a => a.DoctorId == d)).First();
                        }
                        else
                        {
                            // TRUE WALK-IN: If all are busy, assign to the doctor with the least appointments today (overlap allowed)
                            finalDoctorId = doctorsList.Except(blockedDoctorIds).OrderBy(d => allAptsForDoctors.Count(a => a.DoctorId == d)).FirstOrDefault();
                            if (finalDoctorId == Guid.Empty)
                            {
                                throw new InvalidOperationException("Tất cả bác sĩ đều đang bận hoặc nghỉ phép. Vui lòng chờ thêm ít phút.");
                            }
                        }

                        // Generate QR Token
                        string qrToken = "QR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

                        // Generate Queue Number
                        var today = DateTime.UtcNow.Date;
                        var todayAppointments = await _unitOfWork.Appointments.FindAsync(x => x.AppointmentDate.Date == today && x.QueueNumber > 0);
                        var lastQueue = todayAppointments.Any() ? todayAppointments.Max(x => (int?)x.QueueNumber) ?? 0 : 0;

                        // 4. Create Appointment
                        var appointment = new Appointment
                        {
                            Customer = customer,
                            Pet = pet,
                            ServiceId = dto.ServiceId,
                            DoctorId = finalDoctorId,
                            Symptom = dto.Symptom?.Trim(),
                            Status = "waiting", // Walk-in is immediately waiting to be seen
                            Type = "WalkIn",
                            IsWalkIn = true,
                            CreatedBy = createdBy,
                            CreatedAt = DateTime.UtcNow,
                            AppointmentDate = DateTime.SpecifyKind(appointmentDate.Date, DateTimeKind.Utc),
                            StartTime = appointmentDate.TimeOfDay,
                            QrToken = qrToken,
                            QueueNumber = lastQueue + 1,
                            CheckInTime = DateTime.UtcNow,
                            IsEmergency = dto.IsEmergency
                        };

                        await _unitOfWork.Appointments.AddAsync(appointment);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        return appointment.Id;
                    }
                    catch (Exception ex) when (IsTransientConflict(ex) && i < retryCount - 1)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        await Task.Delay(new Random().Next(50, 150));
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                }

                throw new InvalidOperationException("Không thể hoàn tất đăng ký lịch hẹn do tranh chấp dữ liệu kéo dài.");
            });
        }
    }
}
