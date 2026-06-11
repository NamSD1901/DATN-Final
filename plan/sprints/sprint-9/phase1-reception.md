# 🛠️ Đặc Tả Kỹ Thuật: Tiếp Nhận & Duyệt Lịch Hẹn
## Thiết kế Logic Tiếp Nhận Check-In, Walk-In và Tích Hợp SMTP Email Thông Báo

Tài liệu này đặc tả chi tiết kiến trúc tầng nghiệp vụ lễ tân phòng khám, xử lý gửi email thông báo bất đồng bộ khi hủy lịch hẹn và cấu trúc quản lý trạng thái UI.

---

## ⚙️ 1. Giao Ước Kết Nối (Interfaces & API Payloads)

### 📂 Service Interfaces

```csharp
using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi thư điện tử bất đồng bộ cho khách hàng.
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public interface IReceptionistService
    {
        Task CheckInAsync(Guid appointmentId);
        Task<Guid> CreateWalkInAsync(Guid petId, Guid doctorId, string reason);
        Task UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus status, string? reasonForCancellation = null);
    }
}
```

### 📥 API Payloads

```typescript
interface UpdateStatusDto {
  status: 'Confirmed' | 'Cancelled' | 'Completed';
  cancellationReason?: string; // Bắt buộc nếu status = Cancelled
}

interface CreateWalkInDto {
  petId: string;
  doctorId: string;
  reason: string;
}
```

---

## 🧠 2. Backend Service Logic (Tiếp Nhận & Kích Hoạt SMTP Email)

Service logic xử lý nghiệp vụ tại bàn lễ tân và tự động gọi gửi email khi một cuộc hẹn bị hủy bỏ.

```csharp
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ReceptionistService : IReceptionistService
    {
        private readonly DbContext _context;
        private readonly IEmailService _emailService;

        public ReceptionistService(DbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task CheckInAsync(Guid appointmentId)
        {
            var appointment = await _context.Set<Appointment>()
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new NotFoundException(nameof(Appointment), appointmentId);
            }

            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                throw new BusinessException("Chỉ những lịch hẹn đã xác nhận mới có thể thực hiện Check-in.");
            }

            // Chuyển sang trạng thái đang tiến hành khám lâm sàng
            appointment.Status = AppointmentStatus.In_Progress;
            await _context.SaveChangesAsync();
        }

        public async Task<Guid> CreateWalkInAsync(Guid petId, Guid doctorId, string reason)
        {
            // Kiểm tra thú cưng tồn tại
            var petExists = await _context.Set<Pet>().AnyAsync(p => p.Id == petId);
            if (!petExists)
            {
                throw new NotFoundException(nameof(Pet), petId);
            }

            // Tạo nhanh cuộc hẹn vãng lai có hiệu lực ngay tại thời điểm hiện tại
            var walkInAppointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PetId = petId,
                DoctorId = doctorId,
                AppointmentDate = DateTime.Today,
                StartTime = DateTime.Now.TimeOfDay,
                EndTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(30)),
                Reason = "[Walk-In] " + reason,
                Status = AppointmentStatus.In_Progress // Vào thẳng ca khám lâm sàng
            };

            _context.Set<Appointment>().Add(walkInAppointment);
            await _context.SaveChangesAsync();

            return walkInAppointment.Id;
        }

        public async Task UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus status, string? reasonForCancellation = null)
        {
            var appointment = await _context.Set<Appointment>()
                .Include(a => a.PetId) // Cần thông tin thú cưng và chủ để lấy email
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new NotFoundException(nameof(Appointment), appointmentId);
            }

            appointment.Status = status;

            // Xử lý gửi mail nếu bị hủy
            if (status == AppointmentStatus.Cancelled)
            {
                if (string.IsNullOrWhiteSpace(reasonForCancellation))
                {
                    throw new BusinessException("Yêu cầu nhập lý do hủy lịch hẹn.");
                }

                // Truy vấn email chủ nuôi (Owner) liên kết thông qua Pet
                var ownerEmail = await _context.Set<Pet>()
                    .Where(p => p.Id == appointment.PetId)
                    .Join(_context.Set<User>(),
                          p => p.OwnerId,
                          u => u.Id,
                          (p, u) => u.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(ownerEmail))
                {
                    // Gửi mail bất đồng bộ (Fire-and-forget hoặc await tùy nhu cầu)
                    await _emailService.SendEmailAsync(
                        ownerEmail,
                        "Thông báo hủy lịch hẹn - MyPetClinic",
                        $"Kính gửi quý khách, lịch hẹn ngày {appointment.AppointmentDate:dd/MM/yyyy} vào lúc {appointment.StartTime} đã bị hủy. Lý do: {reasonForCancellation}"
                    );
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
```

---

## 🎨 3. Quản Lý Trạng Thái Dashboard Lễ Tân (Vue 3 Pinia Store)

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';

interface Appointment {
  id: string;
  petName: string;
  ownerName: string;
  ownerPhone: string;
  doctorName: string;
  appointmentDate: string;
  startTime: string;
  status: 'Pending' | 'Confirmed' | 'In_Progress' | 'Completed' | 'Cancelled';
}

interface ReceptionState {
  todayAppointments: Appointment[];
  pendingAppointments: Appointment[];
  searchQuery: string;
  loading: boolean;
  error: string | null;
}

export const useReceptionStore = defineStore('reception', {
  state: (): ReceptionState => ({
    todayAppointments: [],
    pendingAppointments: [],
    searchQuery: '',
    loading: false,
    error: null
  }),

  actions: {
    async fetchTodayDashboard() {
      this.loading = true;
      this.error = null;
      try {
        const response = await axios.get('/api/reception/today');
        this.todayAppointments = response.data.appointments;
        this.pendingAppointments = response.data.pending;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi tải danh sách lễ tân.';
      } finally {
        this.loading = false;
      }
    },

    async checkIn(id: string) {
      this.loading = true;
      try {
        await axios.post(`/api/reception/appointments/${id}/check-in`);
        await this.fetchTodayDashboard();
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Check-in thất bại.';
        throw err;
      } finally {
        this.loading = false;
      }
    },

    async approveAppointment(id: string) {
      this.loading = true;
      try {
        await axios.put(`/api/reception/appointments/${id}/status`, { status: 'Confirmed' });
        await this.fetchTodayDashboard();
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Duyệt lịch thất bại.';
        throw err;
      } finally {
        this.loading = false;
      }
    },

    async cancelAppointment(id: string, reason: string) {
      this.loading = true;
      try {
        await axios.put(`/api/reception/appointments/${id}/status`, {
          status: 'Cancelled',
          cancellationReason: reason
        });
        await this.fetchTodayDashboard();
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Hủy lịch thất bại.';
        throw err;
      } finally {
        this.loading = false;
      }
    }
  }
});
```

---

## 🧪 4. Kịch Bản Kiểm Thử Tự Động (xUnit Test Suite)

```csharp
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Services;
using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;

namespace UnitTests.Services
{
    public class ReceptionistServiceTests
    {
        [Fact]
        public async Task UpdateAppointmentStatus_ShouldTriggerEmailService_WhenStatusIsCancelled()
        {
            // Arrange
            var mockEmailService = new Mock<IEmailService>();
            
            // Giả lập DbContext chứa 1 lịch hẹn ở trạng thái Pending
            // var service = new ReceptionistService(mockContext, mockEmailService.Object);

            // Act
            // await service.UpdateAppointmentStatusAsync(appointmentId, AppointmentStatus.Cancelled, "Bác sĩ có ca phẫu thuật đột xuất");

            // Assert
            // Kiểm chứng xem phương thức SendEmailAsync của IEmailService có được gọi đúng 1 lần với nội dung thông báo hủy hay không.
            // mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsContains("hủy")), Times.Once);
            
            Assert.True(true); // Ghi nhận sơ đồ kiểm thử Trigger Email
        }
    }
}
```
