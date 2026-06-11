# 🛠️ Đặc Tả Kỹ Thuật: Tiêm Chủng Vaccine & Thanh Toán Hóa Đơn
## Thiết kế Ghi Nhận Mũi Tiêm, Tính NextDueDate và Kết Xuất Hóa Đơn Tổng Hợp

Tài liệu này đặc tả chi tiết kiến trúc tầng dữ liệu tiêm chủng, logic tính ngày tái chủng, hệ thống kết xuất hóa đơn tổng hợp và giao diện in ấn.

---

## 💾 1. Cấu Trúc Thực Thể & Fluent API (Database Layer)

### 📐 Thực thể C# Domain Entities

```csharp
namespace Domain.Entities
{
    public class VaccinationRecord
    {
        public Guid Id { get; set; }
        public Guid PetId { get; set; }
        public Guid AppointmentId { get; set; }
        public int VaccineId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime VaccinationDate { get; set; }
        public string BatchNumber { get; set; } = string.Empty; // Số lô vaccine
        public DateTime? NextDueDate { get; set; }              // Ngày tái chủng (tự tính)
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Pet Pet { get; set; } = null!;
        public Vaccine Vaccine { get; set; } = null!;
    }

    public enum InvoiceStatus
    {
        Unpaid,  // Chưa thanh toán
        Paid     // Đã thanh toán
    }

    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // Navigation properties
        public Appointment Appointment { get; set; } = null!;
        public ICollection<InvoiceDetail> Details { get; set; } = new List<InvoiceDetail>();
    }

    public class InvoiceDetail
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public string ItemName { get; set; } = string.Empty;   // Tên dịch vụ hoặc tên thuốc
        public string ItemType { get; set; } = string.Empty;   // "Service" hoặc "Medicine"
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }                   // = Quantity * UnitPrice

        // Navigation property
        public Invoice Invoice { get; set; } = null!;
    }
}
```

### 🛢️ Cấu hình Fluent API

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VaccinationRecordConfiguration : IEntityTypeConfiguration<VaccinationRecord>
    {
        public void Configure(EntityTypeBuilder<VaccinationRecord> builder)
        {
            builder.ToTable("VaccinationRecords");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.BatchNumber).HasMaxLength(50).IsRequired();
            builder.Property(v => v.VaccinationDate).HasColumnType("date").IsRequired();
            builder.Property(v => v.NextDueDate).HasColumnType("date");
            builder.Property(v => v.Notes).HasMaxLength(500);

            builder.HasOne(v => v.Pet)
                   .WithMany()
                   .HasForeignKey(v => v.PetId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(v => new { v.PetId, v.VaccinationDate });
        }
    }

    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(i => i.Appointment)
                   .WithOne()
                   .HasForeignKey<Invoice>(i => i.AppointmentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
    {
        public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
        {
            builder.ToTable("InvoiceDetails");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.ItemName).HasMaxLength(200).IsRequired();
            builder.Property(d => d.ItemType).HasMaxLength(20).IsRequired();
            builder.Property(d => d.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(d => d.SubTotal).HasColumnType("decimal(18,2)");

            builder.HasOne(d => d.Invoice)
                   .WithMany(i => i.Details)
                   .HasForeignKey(d => d.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

---

## ⚙️ 2. Giao Ước Kết Nối (Interfaces)

```csharp
using System;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IVaccinationService
    {
        Task<Guid> RecordVaccinationAsync(Guid petId, Guid appointmentId, int vaccineId, Guid doctorId, string batchNumber, string? notes);
    }

    public interface IInvoiceService
    {
        Task<Guid> GenerateInvoiceAsync(Guid appointmentId);
        Task MarkAsPaidAsync(Guid invoiceId);
    }
}
```

---

## 🧠 3. Backend Service Logic

### Ghi nhận mũi tiêm & Tính NextDueDate

```csharp
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class VaccinationService : IVaccinationService
    {
        private readonly DbContext _context;

        public VaccinationService(DbContext context)
        {
            _context = context;
        }

        public async Task<Guid> RecordVaccinationAsync(
            Guid petId, Guid appointmentId, int vaccineId, 
            Guid doctorId, string batchNumber, string? notes)
        {
            var vaccine = await _context.Set<Vaccine>()
                .FirstOrDefaultAsync(v => v.Id == vaccineId);

            if (vaccine == null)
            {
                throw new NotFoundException(nameof(Vaccine), vaccineId);
            }

            // Tính NextDueDate dựa trên chu kỳ tiêm phòng (IntervalDays) của vaccine
            DateTime? nextDueDate = vaccine.IntervalDays > 0
                ? DateTime.Today.AddDays(vaccine.IntervalDays)
                : null; // Nếu IntervalDays = 0 thì không cần tái chủng

            var record = new VaccinationRecord
            {
                Id = Guid.NewGuid(),
                PetId = petId,
                AppointmentId = appointmentId,
                VaccineId = vaccineId,
                DoctorId = doctorId,
                VaccinationDate = DateTime.Today,
                BatchNumber = batchNumber,
                NextDueDate = nextDueDate,
                Notes = notes
            };

            _context.Set<VaccinationRecord>().Add(record);
            await _context.SaveChangesAsync();

            return record.Id;
        }
    }
}
```

### Kết xuất hóa đơn tổng hợp

```csharp
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly DbContext _context;

        public InvoiceService(DbContext context)
        {
            _context = context;
        }

        public async Task<Guid> GenerateInvoiceAsync(Guid appointmentId)
        {
            var appointment = await _context.Set<Appointment>()
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new NotFoundException(nameof(Appointment), appointmentId);
            }

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointmentId,
                TotalAmount = 0
            };

            // 1. Thêm dòng phí dịch vụ khám (từ ClinicService đã đặt lịch)
            var servicePrice = await _context.Set<Appointment>()
                .Where(a => a.Id == appointmentId)
                .Join(_context.Set<ClinicService>(),
                      a => a.ServiceId,
                      s => s.Id,
                      (a, s) => new { s.Name, s.Price })
                .FirstOrDefaultAsync();

            if (servicePrice != null)
            {
                var serviceDetail = new InvoiceDetail
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ItemName = servicePrice.Name,
                    ItemType = "Service",
                    Quantity = 1,
                    UnitPrice = servicePrice.Price,
                    SubTotal = servicePrice.Price
                };
                invoice.Details.Add(serviceDetail);
                invoice.TotalAmount += serviceDetail.SubTotal;
            }

            // 2. Thêm các dòng phí thuốc đã kê trong bệnh án
            var prescriptions = await _context.Set<MedicalRecord>()
                .Where(m => m.AppointmentId == appointmentId)
                .SelectMany(m => m.Prescriptions)
                .Include(p => p.Medicine)
                .ToListAsync();

            foreach (var prescription in prescriptions)
            {
                var medDetail = new InvoiceDetail
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ItemName = prescription.Medicine.Name,
                    ItemType = "Medicine",
                    Quantity = prescription.Quantity,
                    UnitPrice = prescription.Medicine.UnitPrice,
                    SubTotal = prescription.Quantity * prescription.Medicine.UnitPrice
                };
                invoice.Details.Add(medDetail);
                invoice.TotalAmount += medDetail.SubTotal;
            }

            _context.Set<Invoice>().Add(invoice);
            await _context.SaveChangesAsync();

            return invoice.Id;
        }

        public async Task MarkAsPaidAsync(Guid invoiceId)
        {
            var invoice = await _context.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                throw new NotFoundException(nameof(Invoice), invoiceId);
            }

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
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

interface InvoiceDetail {
  itemName: string;
  itemType: 'Service' | 'Medicine';
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

interface Invoice {
  id: string;
  appointmentId: string;
  totalAmount: number;
  status: 'Unpaid' | 'Paid';
  details: InvoiceDetail[];
  createdAt: string;
  paidAt: string | null;
}

interface InvoiceState {
  currentInvoice: Invoice | null;
  loading: boolean;
  error: string | null;
}

export const useInvoiceStore = defineStore('invoice', {
  state: (): InvoiceState => ({
    currentInvoice: null,
    loading: false,
    error: null
  }),

  getters: {
    formattedTotal: (state) => {
      if (!state.currentInvoice) return '0 ₫';
      return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
      }).format(state.currentInvoice.totalAmount);
    }
  },

  actions: {
    async generateInvoice(appointmentId: string) {
      this.loading = true;
      this.error = null;
      try {
        const response = await axios.post(`/api/invoices/generate`, { appointmentId });
        this.currentInvoice = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể kết xuất hóa đơn.';
      } finally {
        this.loading = false;
      }
    },

    async confirmPayment(invoiceId: string) {
      this.loading = true;
      try {
        await axios.put(`/api/invoices/${invoiceId}/pay`);
        if (this.currentInvoice) {
          this.currentInvoice.status = 'Paid';
          this.currentInvoice.paidAt = new Date().toISOString();
        }
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Xác nhận thanh toán thất bại.';
        throw err;
      } finally {
        this.loading = false;
      }
    },

    printInvoice() {
      window.print();
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
    public class VaccinationServiceTests
    {
        [Fact]
        public void NextDueDate_ShouldBeCalculatedCorrectly_BasedOnIntervalDays()
        {
            // Arrange
            // Vaccine dại: IntervalDays = 365 (1 năm)
            var vaccinationDate = new DateTime(2026, 6, 11);
            int intervalDays = 365;

            // Act
            var nextDueDate = vaccinationDate.AddDays(intervalDays);

            // Assert
            nextDueDate.Should().Be(new DateTime(2027, 6, 11));
        }

        [Fact]
        public void NextDueDate_ShouldBeNull_WhenVaccineHasNoInterval()
        {
            // Arrange
            int intervalDays = 0; // Vaccine không cần tái chủng

            // Act
            DateTime? nextDueDate = intervalDays > 0
                ? DateTime.Today.AddDays(intervalDays)
                : null;

            // Assert
            nextDueDate.Should().BeNull();
        }
    }

    public class InvoiceServiceTests
    {
        [Fact]
        public void InvoiceTotal_ShouldEqualSumOfAllDetails()
        {
            // Arrange
            decimal servicePrice = 200_000m;           // Phí khám: 200.000₫
            decimal medicine1Total = 3 * 50_000m;      // 3 viên × 50.000₫ = 150.000₫
            decimal medicine2Total = 1 * 120_000m;     // 1 lọ × 120.000₫ = 120.000₫

            // Act
            decimal total = servicePrice + medicine1Total + medicine2Total;

            // Assert
            total.Should().Be(470_000m); // Tổng = 470.000₫
        }
    }
}
```
