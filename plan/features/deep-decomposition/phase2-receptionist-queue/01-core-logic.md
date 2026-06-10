# 🧠 Core Business Logic - Receptionist Portal & Queue Management

## 1. Thuật toán Cân bằng tải Phòng khám (Clinic Room Load Balancing Algorithm)

Để phân bổ thú cưng vào các phòng khám một cách khoa học, tránh trường hợp phòng khám quá tải trong khi phòng khác đang trống bác sĩ, hệ thống áp dụng thuật toán đếm số lượng hàng chờ thời gian thực:

```csharp
public class ClinicRoomAllocator
{
    private readonly IApplicationDbContext _context;

    public ClinicRoomAllocator(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Đề xuất phòng khám có số lượng thú cưng đang xếp hàng chờ (waiting) ít nhất
    /// </summary>
    public async Task<string> SuggestOptimalRoomAsync(Guid doctorId)
    {
        // 1. Lấy danh sách các phòng khám đang hoạt động và số lượng thú cưng đang chờ
        var activeRooms = await _context.Appointments
            .Where(a => a.Status == "waiting" && a.ClinicRoom != null)
            .GroupBy(a => a.ClinicRoom)
            .Select(g => new { Room = g.Key, WaitCount = g.Count() })
            .ToListAsync();

        // Định nghĩa các phòng khám mặc định của phòng khám
        var defaultRooms = new List<string> { "Phòng khám 101", "Phòng khám 102", "Phòng khám 103" };

        // 2. Tìm phòng khám có số lượng chờ ít nhất
        string selectedRoom = defaultRooms.First();
        int minWait = int.MaxValue;

        foreach (var room in defaultRooms)
        {
            var roomStat = activeRooms.FirstOrDefault(r => r.Room == room);
            int count = roomStat?.WaitCount ?? 0;

            if (count < minWait)
            {
                minWait = count;
                selectedRoom = room;
            }
        }

        return selectedRoom;
    }
}
```

---

## 2. Mã nguồn Logic Nghiệp vụ C# - Tiếp nhận & Cấp số thứ tự (Thread-Safe Check-In)

Dưới đây là đặc tả chi tiết logic nghiệp vụ kiểm soát tiếp nhận tại `QueueService`, áp dụng cơ chế khóa an toàn luồng `SemaphoreSlim` và Database Transaction nguyên tử (Atomic Transaction):

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Hubs;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class QueueService : IQueueService
    {
        private readonly IApplicationDbContext _context;
        private readonly IHubContext<QueueHub> _hubContext;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public QueueService(IApplicationDbContext context, IHubContext<QueueHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Tiếp nhận và sinh số thứ tự khám cho khách hàng đã đặt lịch trước
        /// </summary>
        public async Task<QueueStatusDto> CheckInAppointmentAsync(Guid appointmentId, string clinicRoom)
        {
            // Sử dụng Semaphore để khóa đồng thời tiến trình cấp số thứ tự, tránh trùng lặp số khi có nhiều quầy lễ tân submit cùng lúc
            await _semaphore.WaitAsync();
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var appointment = await _context.Appointments
                        .Include(a => a.Pet)
                        .Include(a => a.Doctor)
                        .Include(a => a.Service)
                        .FirstOrDefaultAsync(a => a.Id == appointmentId);

                    if (appointment == null)
                    {
                        throw new KeyNotFoundException("Lịch hẹn không tồn tại trong hệ thống.");
                    }

                    if (appointment.Status.ToLower() != "confirmed" && appointment.Status.ToLower() != "pending")
                    {
                        throw new InvalidOperationException("Trạng thái lịch hẹn hiện tại không cho phép check-in.");
                    }

                    // Sinh số thứ tự tiếp theo trong ngày
                    string queueNumber = await GenerateNextQueueNumberAsync();

                    // Cập nhật thông tin tiếp nhận
                    appointment.Status = "waiting";
                    appointment.QueueNumber = queueNumber;
                    appointment.CheckInTime = DateTime.UtcNow;
                    appointment.ClinicRoom = string.IsNullOrEmpty(clinicRoom) ? "Phòng khám 101" : clinicRoom;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var result = new QueueStatusDto
                    {
                        QueueNumber = appointment.QueueNumber,
                        PetName = appointment.Pet.Name,
                        CustomerName = appointment.Pet.OwnerName ?? "Khách vãng lai",
                        DoctorName = appointment.Doctor?.FullName ?? "Bác sĩ trực ca",
                        ServiceName = appointment.Service?.Name ?? "Tiêm chủng",
                        Status = appointment.Status,
                        CheckInTime = appointment.CheckInTime.Value,
                        ClinicRoom = appointment.ClinicRoom
                    };

                    // Phát tín hiệu SignalR Hub cập nhật Real-time cho các màn hình sảnh chờ và dashboard
                    await _hubContext.Clients.All.SendAsync("QueueUpdated", result);

                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Sinh số thứ tự tiếp theo định dạng Q-XXX trong ngày
        /// </summary>
        private async Task<string> GenerateNextQueueNumberAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Đếm số lịch hẹn đã cấp số thứ tự trong ngày hôm nay
            var lastQueueNumber = await _context.Appointments
                .Where(a => a.CheckInTime >= today && a.CheckInTime < tomorrow && a.QueueNumber != null)
                .OrderByDescending(a => a.QueueNumber)
                .Select(a => a.QueueNumber)
                .FirstOrDefaultAsync();

            int nextNum = 1;
            if (lastQueueNumber != null && lastQueueNumber.StartsWith("Q-"))
            {
                if (int.TryParse(lastQueueNumber.Substring(2), out int currentNum))
                {
                    nextNum = currentNum + 1;
                }
            }

            return $"Q-{nextNum:D3}"; // Định dạng Q-001, Q-002,...
        }
    }
}
```
