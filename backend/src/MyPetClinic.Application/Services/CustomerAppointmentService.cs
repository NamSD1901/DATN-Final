using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.Helpers;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Application.Services
{
    public class CustomerAppointmentService : ICustomerAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppointmentService _appointmentService;

        public CustomerAppointmentService(IUnitOfWork unitOfWork, IAppointmentService appointmentService)
        {
            _unitOfWork = unitOfWork;
            _appointmentService = appointmentService;
        }

        public async Task<object> GetAvailableVaccinesAsync()
        {
            var vaccines = await _unitOfWork.Vaccines.FindAsync(v => v.StockQuantity > 0);
            return vaccines.OrderBy(v => v.Name).Select(v => new
            {
                id = v.Id,
                name = v.Name,
                description = v.Description,
                targetSpecies = v.TargetSpecies
            });
        }

        public async Task<object> ValidateVaccineAsync(Guid customerId, long petId, long vaccineId, DateTime targetDate)
        {
            var pets = await _unitOfWork.Pets.FindAsync(p => p.Id == petId && p.CustomerId == customerId);
            var pet = pets.FirstOrDefault();
            if (pet == null)
                throw new InvalidOperationException("Thú cưng không hợp lệ hoặc không thuộc về bạn.");

            var vaccines = await _unitOfWork.Vaccines.FindAsync(v => v.Id == vaccineId);
            var vaccine = vaccines.FirstOrDefault();
            if (vaccine == null)
                throw new KeyNotFoundException("Không tìm thấy vắc-xin.");

            var lastRecords = await _unitOfWork.VaccinationRecords.FindAsync(vr => vr.PetId == petId && vr.VaccineId == vaccineId);
            var lastRecord = lastRecords.OrderByDescending(vr => vr.InjectionDate).FirstOrDefault();

            var checker = new VaccinationScheduleChecker();
            return checker.ValidateInterval(lastRecord, vaccine, targetDate, pet);
        }

        public async Task<long> BookAppointmentAsync(MyPetClinic.Application.DTOs.CustomerBookingDto dto, Guid customerId, Guid userId)
        {
            if (dto.IsEmergency)
            {
                throw new InvalidOperationException("TRƯỜNG HỢP CẤP CỨU: Vui lòng KHÔNG đặt lịch online. Hãy đưa bé đến phòng khám ngay lập tức hoặc gọi Hotline khẩn cấp.");
            }

            var users = await _unitOfWork.Users.FindAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();
            
            var customers = await _unitOfWork.Customers.FindAsync(c => c.Id == customerId && c.DeletedAt == null);
            var customer = customers.FirstOrDefault();

            bool hasPhone = false;
            if (user != null && !string.IsNullOrWhiteSpace(user.Phone)) hasPhone = true;
            if (customer != null && !string.IsNullOrWhiteSpace(customer.Phone)) hasPhone = true;

            if (!hasPhone)
            {
                throw new InvalidOperationException("MISSING_PHONE: Tài khoản của bạn chưa có số điện thoại. Vui lòng cập nhật số điện thoại trong phần Hồ sơ để chúng tôi có thể liên hệ xác nhận.");
            }

            var appointmentDate = dto.AppointmentDate ?? DateTime.Now;
            
            // 2. Lead Time Check: Must book at least 15 minutes in advance
            if (appointmentDate < DateTime.Now.AddMinutes(15))
            {
                throw new InvalidOperationException("Vui lòng đặt lịch trước ít nhất 15 phút để chúng tôi có sự chuẩn bị tốt nhất.");
            }

            // 3. Operating Hours Check (08:00 - 20:00) & Giờ nghỉ trưa (12:00 - 13:30)
            var hour = appointmentDate.Hour;
            if (hour < 8 || hour >= 20)
            {
                throw new InvalidOperationException("Phòng khám đóng cửa vào thời gian này. Vui lòng chọn khung giờ trong giờ hành chính (08:00 - 20:00).");
            }
            if (hour == 12 || (hour == 13 && appointmentDate.Minute < 30))
            {
                throw new InvalidOperationException("Phòng khám đang trong giờ nghỉ trưa (12:00 - 13:30). Vui lòng chọn khung giờ khác.");
            }

            // 4. No-show limit & Cancel limit
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var recentAppointments = await _unitOfWork.Appointments.FindAsync(
                a => a.CustomerId == customerId && a.AppointmentDate >= thirtyDaysAgo);
            
            var noShowCount = recentAppointments.Count(a => a.Status.ToLower() == "no_show");
            var cancelCount = recentAppointments.Count(a => a.Status.ToLower() == "cancelled");

            if (noShowCount >= 3)
            {
                throw new InvalidOperationException("Tài khoản của bạn tạm thời bị hạn chế đặt lịch online do lịch sử vắng mặt nhiều lần. Vui lòng gọi trực tiếp Hotline để được hỗ trợ.");
            }

            // 5. Spam booking check (same pet, within 2 hours)
            var todayAppointments = await _unitOfWork.Appointments.FindAsync(
                a => a.CustomerId == customerId && a.PetId == dto.PetId && a.AppointmentDate.Date == appointmentDate.Date && a.Status != "cancelled" && a.Status != "no_show");
            
            if (todayAppointments.Any(a => Math.Abs((a.AppointmentDate - appointmentDate).TotalHours) < 2))
            {
                throw new InvalidOperationException("Bé cưng đã có lịch hẹn quá sát với thời gian này. Bạn không thể đặt thêm lịch liên tiếp (chống spam).");
            }

            // 6. Call Core Service
            var createDto = new MyPetClinic.Application.DTOs.AppointmentCreateDto
            {
                CustomerId = customerId,
                PetId = dto.PetId,
                DoctorId = Guid.Empty, // Bắt buộc hệ thống tự động phân công theo mảng dịch vụ
                ServiceId = dto.ServiceId,
                AppointmentDate = dto.AppointmentDate,
                Symptom = dto.Symptom ?? string.Empty,
                Note = dto.Note,
                VaccineId = dto.VaccineId
            };

            var appointmentId = await _appointmentService.CreateAppointmentAsync(createDto, userId);

            // 7. Update to pending_approval if Cancel Count >= 5
            // [DEMO MODE]: Tạm thời tắt chức năng phạt chờ duyệt đặc biệt để dễ test
            /*
            if (cancelCount >= 5)
            {
                var createdAppointmentList = await _unitOfWork.Appointments.FindAsync(a => a.Id == appointmentId);
                var createdAppointment = createdAppointmentList.FirstOrDefault();
                if (createdAppointment != null)
                {
                    createdAppointment.Status = "pending_approval";
                    _unitOfWork.Appointments.Update(createdAppointment);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            */

            return appointmentId;
        }
    }
}
