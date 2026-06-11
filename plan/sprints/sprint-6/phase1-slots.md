# 🛠️ Đặc Tả Kỹ Thuật: Ca Làm Việc & Khung Giờ Bác Sĩ
## Thiết kế Lớp, Cơ sở dữ liệu và Thuật toán Tính toán Slot Khả dụng

Tài liệu này đặc tả chi tiết kiến trúc cơ sở dữ liệu, mã nguồn triển khai mẫu và kịch bản unit test cho hệ thống ca làm việc và tính toán khung giờ khám (slots) rảnh của bác sĩ thú y.

---

## 💾 1. Cấu Trúc Thực Thể & Fluent API (Database Layer)

Hệ thống quản lý lịch trực của bác sĩ thông qua hai thực thể chính:
1.  **WorkShift (Ca làm việc):** Định nghĩa cấu hình thời gian bắt đầu và kết thúc cố định (ví dụ: Ca Sáng từ 08:00 đến 12:00).
2.  **DoctorSchedule (Lịch trực của Bác sĩ):** Gán một Bác sĩ thú y (User có Role là Vet) vào một ca làm việc (`WorkShift`) vào một ngày cụ thể (`Date`).

### 📐 Thực thể C# Domain Entities

```csharp
namespace Domain.Entities
{
    public class WorkShift
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Ví dụ: "Ca Sáng", "Ca Chiều"
        public TimeSpan StartTime { get; set; } // Ví dụ: 08:00:00
        public TimeSpan EndTime { get; set; }   // Ví dụ: 12:00:00
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
    }

    public class DoctorSchedule
    {
        public int Id { get; set; }
        public Guid DoctorId { get; set; } // Khóa ngoại liên kết bảng User (Vet)
        public int WorkShiftId { get; set; }
        public DateTime Date { get; set; } // Ngày đăng ký ca trực (chỉ lưu phần Date, Time = 00:00:00)
        
        // Navigation properties
        public WorkShift WorkShift { get; set; } = null!;
    }
}
```

### 🛢️ Cấu hình Fluent API (EF Core Configuration)

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
    {
        public void Configure(EntityTypeBuilder<WorkShift> builder)
        {
            builder.ToTable("WorkShifts");
            builder.HasKey(ws => ws.Id);
            builder.Property(ws => ws.Name).HasMaxLength(50).IsRequired();
            builder.Property(ws => ws.StartTime).IsRequired();
            builder.Property(ws => ws.EndTime).IsRequired();
        }
    }

    public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
    {
        public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
        {
            builder.ToTable("DoctorSchedules");
            builder.HasKey(ds => ds.Id);
            builder.Property(ds => ds.DoctorId).IsRequired();
            builder.Property(ds => ds.Date).HasColumnType("date").IsRequired();

            // Ràng buộc Unique Composite Index: 1 Bác sĩ không thể đăng ký trùng ca trong cùng 1 ngày
            builder.HasIndex(ds => new { ds.DoctorId, ds.Date, ds.WorkShiftId }).IsUnique();

            builder.HasOne(ds => ds.WorkShift)
                   .WithMany(ws => ws.DoctorSchedules)
                   .HasForeignKey(ds => ds.WorkShiftId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
```

---

## ⚙️ 2. Giao Ước Kết Nối (Interfaces Repository & Helper)

### 📂 Repository Interface

```csharp
using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface IDoctorScheduleRepository
    {
        Task<IEnumerable<DoctorSchedule>> GetSchedulesByDoctorAndDateAsync(Guid doctorId, DateTime date);
        Task<IEnumerable<DoctorSchedule>> GetSchedulesByDateAsync(DateTime date);
        Task AddScheduleAsync(DoctorSchedule schedule);
        Task DeleteScheduleAsync(int scheduleId);
    }
}
```

### 🧩 Slot Calculation Helper Interface

```csharp
using System;
using System.Collections.Generic;

namespace Application.Common.Interfaces
{
    public class SlotInfo
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public interface ISlotCalculationHelper
    {
        /// <summary>
        /// Tính toán danh sách các slot khám và trạng thái khả dụng của bác sĩ trong một ngày cụ thể.
        /// </summary>
        /// <param name="shiftStart">Giờ bắt đầu ca trực</param>
        /// <param name="shiftEnd">Giờ kết thúc ca trực</param>
        /// <param name="bookedAppointments">Danh sách các khoảng thời gian đã được đặt lịch khám (StartTime, EndTime)</param>
        /// <param name="slotDurationMinutes">Thời gian mỗi ca khám (mặc định 30 phút)</param>
        /// <returns>Danh sách các slot khám rảnh/bận</returns>
        List<SlotInfo> CalculateAvailableSlots(
            TimeSpan shiftStart, 
            TimeSpan shiftEnd, 
            IEnumerable<(TimeSpan Start, TimeSpan End)> bookedAppointments,
            int slotDurationMinutes = 30);
    }
}
```

---

## 🧠 3. Thuật Toán Tính Toán Slot Khả Dụng (C# Service Logic)

Dưới đây là mã nguồn thuật toán tối ưu hóa để chia nhỏ ca làm việc thành các slot nhỏ, sau đó đối chiếu chéo để gắn nhãn `IsAvailable = false` đối với các slot bị trùng lịch hẹn.

```csharp
using Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Services
{
    public class SlotCalculationHelper : ISlotCalculationHelper
    {
        public List<SlotInfo> CalculateAvailableSlots(
            TimeSpan shiftStart, 
            TimeSpan shiftEnd, 
            IEnumerable<(TimeSpan Start, TimeSpan End)> bookedAppointments,
            int slotDurationMinutes = 30)
        {
            var slots = new List<SlotInfo>();
            
            if (shiftStart >= shiftEnd || slotDurationMinutes <= 0)
            {
                return slots;
            }

            var duration = TimeSpan.FromMinutes(slotDurationMinutes);
            var currentStart = shiftStart;

            // 1. Phân rã ca trực thành các slot nhỏ 30 phút
            while (currentStart + duration <= shiftEnd)
            {
                var currentEnd = currentStart + duration;
                
                // 2. Kiểm tra xem slot hiện tại có bị đè (overlap) bởi bất kỳ lịch hẹn nào không
                bool isOverlap = bookedAppointments.Any(appointment => 
                    // Công thức overlap giữa 2 khoảng thời gian [A, B] và [C, D]:
                    // Overlap xảy ra khi và chỉ khi: A < D và C < B
                    currentStart < appointment.End && appointment.Start < currentEnd
                );

                slots.Add(new SlotInfo
                {
                    StartTime = currentStart,
                    EndTime = currentEnd,
                    IsAvailable = !isOverlap
                });

                // Chuyển sang slot tiếp theo
                currentStart = currentEnd;
            }

            return slots;
        }
    }
}
```

---

## 🧪 4. Kế Hoạch Kiểm Thử Tự Động (xUnit Test Suite)

Dưới đây là bộ kịch bản kiểm thử viết bằng xUnit dùng để chứng minh thuật toán phân tách ca trực và gán trạng thái rảnh/bận hoạt động chính xác tuyệt đối.

```csharp
using Application.Common.Interfaces;
using Infrastructure.Services;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;

namespace UnitTests.Infrastructure
{
    public class SlotCalculationHelperTests
    {
        private readonly ISlotCalculationHelper _helper;

        public SlotCalculationHelperTests()
        {
            _helper = new SlotCalculationHelper();
        }

        [Fact]
        public void CalculateAvailableSlots_ShouldReturnAllSlotsAvailable_WhenNoAppointmentsBooked()
        {
            // Arrange
            var shiftStart = new TimeSpan(8, 0, 0); // 08:00
            var shiftEnd = new TimeSpan(10, 0, 0);   // 10:00
            var bookedAppointments = new List<(TimeSpan Start, TimeSpan End)>();

            // Act
            var result = _helper.CalculateAvailableSlots(shiftStart, shiftEnd, bookedAppointments, 30);

            // Assert
            // 8:00 -> 10:00 chia 30 phút sẽ được 4 slots:
            // 8:00-8:30, 8:30-9:00, 9:00-9:30, 9:30-10:00
            result.Should().HaveCount(4);
            result.Should().AllSatisfy(slot => slot.IsAvailable.Should().BeTrue());
            result[0].StartTime.Should().Be(new TimeSpan(8, 0, 0));
            result[3].EndTime.Should().Be(new TimeSpan(10, 0, 0));
        }

        [Fact]
        public void CalculateAvailableSlots_ShouldMarkSlotAsUnavailable_WhenExactlyMatchesAppointment()
        {
            // Arrange
            var shiftStart = new TimeSpan(8, 0, 0);
            var shiftEnd = new TimeSpan(10, 0, 0);
            var bookedAppointments = new List<(TimeSpan Start, TimeSpan End)>
            {
                (new TimeSpan(8, 30, 0), new TimeSpan(9, 0, 0)) // Lịch hẹn từ 8:30 đến 9:00
            };

            // Act
            var result = _helper.CalculateAvailableSlots(shiftStart, shiftEnd, bookedAppointments, 30);

            // Assert
            result.Should().HaveCount(4);
            result[0].IsAvailable.Should().BeTrue();  // 8:00 - 8:30 rảnh
            result[1].IsAvailable.Should().BeFalse(); // 8:30 - 9:00 bận do trùng lịch hẹn
            result[2].IsAvailable.Should().BeTrue();  // 9:00 - 9:30 rảnh
            result[3].IsAvailable.Should().BeTrue();  // 9:30 - 10:00 rảnh
        }

        [Fact]
        public void CalculateAvailableSlots_ShouldMarkMultipleSlotsAsUnavailable_WhenAppointmentSpansMultipleSlots()
        {
            // Arrange
            var shiftStart = new TimeSpan(8, 0, 0);
            var shiftEnd = new TimeSpan(11, 0, 0); // Ca trực 3 tiếng -> 6 slots
            var bookedAppointments = new List<(TimeSpan Start, TimeSpan End)>
            {
                (new TimeSpan(8, 45, 0), new TimeSpan(10, 15, 0)) // Lịch hẹn kéo dài đè qua nhiều slots
            };

            // Act
            var result = _helper.CalculateAvailableSlots(shiftStart, shiftEnd, bookedAppointments, 30);

            // Assert
            // Slots: 
            // 0: 08:00 - 08:30 (Rảnh)
            // 1: 08:30 - 09:00 (Bận - trùng phần 08:45-09:00)
            // 2: 09:00 - 09:30 (Bận - trùng toàn bộ)
            // 3: 09:30 - 10:00 (Bận - trùng toàn bộ)
            // 4: 10:00 - 10:30 (Bận - trùng phần 10:00-10:15)
            // 5: 10:30 - 11:00 (Rảnh)
            result.Should().HaveCount(6);
            result[0].IsAvailable.Should().BeTrue();
            result[1].IsAvailable.Should().BeFalse();
            result[2].IsAvailable.Should().BeFalse();
            result[3].IsAvailable.Should().BeFalse();
            result[4].IsAvailable.Should().BeFalse();
            result[5].IsAvailable.Should().BeTrue();
        }
    }
}
```
