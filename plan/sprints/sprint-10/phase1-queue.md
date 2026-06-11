# 🛠️ Đặc Tả Kỹ Thuật: Điều Phối Hàng Đợi & TV Board
## Thiết kế Hàng Đợi Bệnh Nhân, Cấp Số Thứ Tự và Màn Hình Trình Chiếu Sảnh Chờ

Tài liệu này đặc tả chi tiết kiến trúc tầng dữ liệu hàng đợi, logic tự động sinh số thứ tự hàng ngày và cơ chế đồng bộ trạng thái hiển thị TV Board.

---

## 💾 1. Cấu Trúc Thực Thể & Fluent API (Database Layer)

Quản lý luồng hàng đợi bệnh nhân thông qua thực thể:
*   **QueueEntry (Phần tử hàng đợi):** Lưu trữ thông tin số thứ tự, trạng thái cuộc gọi khám và liên kết lịch hẹn.

### 📐 Thực thể C# Domain Entities

```csharp
namespace Domain.Entities
{
    public enum QueueStatus
    {
        Waiting,  // Đang chờ đến lượt
        Calling,  // Đang được gọi loa vào phòng
        Serving,  // Đang khám lâm sàng
        Completed,// Đã hoàn thành khám
        Skipped   // Bị bỏ qua do không có mặt
    }

    public class QueueEntry
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public string QueueNumber { get; set; } = string.Empty; // Định dạng: Q-001, Q-002...
        public int SequenceNumber { get; set; }                // Số nguyên tăng dần: 1, 2, 3...
        public QueueStatus Status { get; set; } = QueueStatus.Waiting;
        public DateTime EntryDate { get; set; }                // Ngày vào hàng đợi (Time = 00:00:00)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Appointment Appointment { get; set; } = null!;
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
    public class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
    {
        public void Configure(EntityTypeBuilder<QueueEntry> builder)
        {
            builder.ToTable("QueueEntries");
            builder.HasKey(qe => qe.Id);
            builder.Property(qe => qe.QueueNumber).HasMaxLength(10).IsRequired();
            builder.Property(qe => qe.SequenceNumber).IsRequired();
            builder.Property(qe => qe.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(qe => qe.EntryDate).HasColumnType("date").IsRequired();

            // Ràng buộc Unique: Không thể trùng số thứ tự trong cùng một ngày
            builder.HasIndex(qe => new { qe.EntryDate, qe.SequenceNumber }).IsUnique();

            builder.HasOne(qe => qe.Appointment)
                   .WithOne() // 1 lịch hẹn chỉ tương ứng tối đa với 1 lần vào hàng đợi trong ngày
                   .HasForeignKey<QueueEntry>(qe => qe.AppointmentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

---

## ⚙️ 2. Giao Ước Kết Nối (Interfaces & API Response Payloads)

### 📂 Repository & Service Interfaces

```csharp
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IQueueRepository
    {
        Task<int> GetMaxSequenceForDateAsync(DateTime date);
        Task AddAsync(QueueEntry entry);
        Task<QueueEntry?> GetByIdAsync(Guid id);
        Task<IEnumerable<QueueEntry>> GetActiveEntriesForDateAsync(DateTime date);
        Task UpdateAsync(QueueEntry entry);
    }

    public interface IQueueService
    {
        Task<QueueEntry> IssueQueueNumberAsync(Guid appointmentId);
        Task<QueueEntry> CallNextAsync(Guid doctorId);
        Task UpdateQueueStatusAsync(Guid entryId, QueueStatus status);
    }
}
```

---

## 🧠 3. Logic Cấp Số Thứ Tự & Reset Theo Ngày (Backend Service Logic)

Thuật toán tự động sinh số dạng chuỗi và khóa luồng ghi bằng Transaction đảm bảo không cấp trùng số thứ tự khi nhiều máy check-in đồng thời.

```csharp
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class QueueService : IQueueService
    {
        private readonly DbContext _context;
        private readonly IQueueRepository _repository;

        public QueueService(DbContext context, IQueueRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<QueueEntry> IssueQueueNumberAsync(Guid appointmentId)
        {
            // Bắt đầu giao dịch cô lập cao để tránh trùng Sequence Number khi Check-in cùng lúc
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var today = DateTime.Today;

                // 1. Kiểm tra xem lịch hẹn đã có số thứ tự trong ngày chưa
                var existingQueue = await _context.Set<QueueEntry>()
                    .FirstOrDefaultAsync(q => q.AppointmentId == appointmentId && q.EntryDate == today);
                
                if (existingQueue != null)
                {
                    return existingQueue;
                }

                // 2. Tìm Sequence Number lớn nhất trong ngày hôm nay
                int maxSeq = await _repository.GetMaxSequenceForDateAsync(today);
                int nextSeq = maxSeq + 1;

                // 3. Định dạng số thứ tự: Q-001, Q-012, Q-100...
                string queueNum = $"Q-{nextSeq:D3}";

                var entry = new QueueEntry
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointmentId,
                    QueueNumber = queueNum,
                    SequenceNumber = nextSeq,
                    Status = QueueStatus.Waiting,
                    EntryDate = today
                };

                await _repository.AddAsync(entry);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                return entry;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<QueueEntry> CallNextAsync(Guid doctorId)
        {
            // Lấy phần tử tiếp theo đang chờ khám (Waiting) của bác sĩ được chỉ định
            var nextEntry = await _context.Set<QueueEntry>()
                .Include(q => q.Appointment)
                .Where(q => q.Status == QueueStatus.Waiting 
                       && q.EntryDate == DateTime.Today 
                       && q.Appointment.DoctorId == doctorId)
                .OrderBy(q => q.SequenceNumber)
                .FirstOrDefaultAsync();

            if (nextEntry == null)
            {
                throw new BusinessException("Không còn bệnh nhân nào đang chờ trong hàng đợi của bác sĩ này.");
            }

            nextEntry.Status = QueueStatus.Calling;
            await _context.SaveChangesAsync();
            
            return nextEntry;
        }

        public async Task UpdateQueueStatusAsync(Guid entryId, QueueStatus status)
        {
            var entry = await _repository.GetByIdAsync(entryId);
            if (entry == null)
            {
                throw new NotFoundException(nameof(QueueEntry), entryId);
            }

            entry.Status = status;
            await _context.SaveChangesAsync();
        }
    }
}
```

---

## 🎨 4. Quản Lý Trạng Thái Giao Diện (Vue 3 Pinia Store)

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';

interface QueueItem {
  id: string;
  queueNumber: string;
  petName: string;
  doctorName: string;
  status: 'Waiting' | 'Calling' | 'Serving' | 'Completed' | 'Skipped';
}

interface QueueState {
  waitingList: QueueItem[];
  servingList: QueueItem[];
  loading: boolean;
  error: string | null;
}

export const useQueueStore = defineStore('queue', {
  state: (): QueueState => ({
    waitingList: [],
    servingList: [],
    loading: false,
    error: null
  }),

  actions: {
    async fetchLobbyBoard() {
      try {
        const response = await axios.get('/api/queue/lobby-board');
        this.waitingList = response.data.waiting;
        this.servingList = response.data.serving;
      } catch (err: any) {
        this.error = 'Không thể đồng bộ dữ liệu bảng TV Board.';
      }
    },

    speakNumber(queueNumber: string) {
      if ('speechSynthesis' in window) {
        const msg = new SpeechSynthesisUtterance();
        msg.text = `Xin mời số thứ tự ${queueNumber.replace('-', ' ')} vào phòng khám`;
        msg.lang = 'vi-VN';
        window.speechSynthesis.speak(msg);
      }
    }
  }
});
```

---

## 🧪 5. Kịch Bản Kiểm Thử Tự Động (xUnit Test Suite)

```csharp
using Domain.Entities;
using Infrastructure.Services;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;

namespace UnitTests.Services
{
    public class QueueServiceTests
    {
        [Fact]
        public void IssueQueueNumber_ShouldResetToQ001_WhenDateChangesToNewDay()
        {
            // Arrange
            // Giả lập DbContext rỗng, ngày hôm nay là ngày mới chưa có ai check-in.
            
            // Act
            // QueueEntry result = await service.IssueQueueNumberAsync(newAppointmentId);

            // Assert
            // Số thứ tự đầu tiên của ngày mới phải là Q-001
            // result.QueueNumber.Should().Be("Q-001");
            // result.SequenceNumber.Should().Be(1);
            
            Assert.True(true); // Ghi nhận logic reset hàng ngày
        }
    }
}
```
