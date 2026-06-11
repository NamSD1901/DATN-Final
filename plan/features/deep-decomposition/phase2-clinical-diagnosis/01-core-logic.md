# 🧠 Core Business Logic - Clinical Diagnosis & Treatment

## 1. Thuật toán Cảnh báo trùng lặp Hoạt chất (Drug Interaction & Duplicate Active Ingredient Check)

Để đảm bảo an toàn y tế cho thú cưng, tránh trường hợp bác sĩ vô tình kê hai loại thuốc khác tên thương mại nhưng có cùng hoạt chất dược lý (Active Ingredient) dẫn đến quá liều gây nguy hiểm:

```csharp
public class PrescriptionSafetyChecker
{
    private readonly IApplicationDbContext _context;

    public PrescriptionSafetyChecker(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Kiểm tra và đưa ra cảnh báo nếu đơn thuốc có chứa các biệt dược trùng hoạt chất
    /// </summary>
    public async Task<(bool HasWarning, string WarningMessage)> VerifyPrescriptionSafetyAsync(List<PrescriptionItemDto> items)
    {
        var medicineIds = items.Select(i => i.MedicineId).Distinct().ToList();
        
        // Tải thông tin hoạt chất của các thuốc kê đơn
        var medicines = await _context.Medicines
            .Where(m => medicineIds.Contains(m.Id))
            .Select(m => new { m.Id, m.Name, m.ActiveIngredient })
            .ToListAsync();

        // Kiểm tra trùng lặp hoạt chất
        var activeIngredientGroups = medicines
            .Where(m => !string.IsNullOrEmpty(m.ActiveIngredient))
            .GroupBy(m => m.ActiveIngredient.ToLower().Trim())
            .Where(g => g.Count() > 1)
            .ToList();

        if (activeIngredientGroups.Any())
        {
            var duplicateDetails = string.Join("; ", activeIngredientGroups.Select(g => 
                $"Hoạt chất '{g.Key}' có trong các thuốc: {string.Join(", ", g.Select(m => m.Name))}"
            ));
            
            return (true, $"CẢNH BÁO Y TẾ: Đơn thuốc chứa hoạt chất trùng lặp có thể gây quá liều: {duplicateDetails}");
        }

        return (false, string.Empty);
    }
}
```

---

## 2. C# Logic: Xử lý Giao dịch Bệnh án & Đơn thuốc (Atomic Transaction)

Dưới đây là đặc tả logic nghiệp vụ xử lý tại Application Layer, thực hiện các kiểm tra y tế, kiểm kho, trừ kho thuốc bi quan và tự động phát sinh hóa đơn nháp trong một transaction duy nhất để đảm bảo tính ACID:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Domain.Exceptions;

namespace MyPetClinic.Application.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IApplicationDbContext _context;
        private readonly PrescriptionSafetyChecker _safetyChecker;

        public MedicalRecordService(IApplicationDbContext context, PrescriptionSafetyChecker safetyChecker)
        {
            _context = context;
            _safetyChecker = safetyChecker;
        }

        /// <summary>
        /// Tạo hồ sơ bệnh án, trừ kho thuốc thực tế và sinh hóa đơn nháp
        /// </summary>
        public async Task<long> CreateMedicalRecordAsync(CreateMedicalRecordDto dto, Guid currentDoctorId)
        {
            // 1. Kiểm tra an toàn đơn thuốc trước khi thực thi ghi DB
            if (dto.PrescriptionItems.Any())
            {
                var (hasWarning, warningMessage) = await _safetyChecker.VerifyPrescriptionSafetyAsync(dto.PrescriptionItems);
                if (hasWarning)
                {
                    throw new InvalidOperationException(warningMessage);
                }
            }

            // Bắt đầu Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Tạo bản ghi Bệnh án lâm sàng (MedicalRecord)
                var medicalRecord = new MedicalRecord
                {
                    AppointmentId = dto.AppointmentId,
                    PetId = dto.PetId,
                    Diagnosis = dto.Diagnosis,
                    TreatmentPlan = dto.TreatmentPlan,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MedicalRecords.Add(medicalRecord);
                await _context.SaveChangesAsync(); // Lưu để lấy ID tự sinh

                decimal totalMedicineAmount = 0;

                // 3. Nếu có đơn thuốc, lưu và trừ kho
                if (dto.PrescriptionItems.Any())
                {
                    var prescription = new Prescription
                    {
                        MedicalRecordId = medicalRecord.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Prescriptions.Add(prescription);
                    await _context.SaveChangesAsync();

                    foreach (var item in dto.PrescriptionItems)
                    {
                        // Truy vấn với Row Lock (FOR UPDATE) để ngăn chặn Race Condition khi nhiều bác sĩ kê cùng loại thuốc
                        var medicine = await _context.Medicines
                            .FromSqlRaw("SELECT * FROM \"Medicines\" WHERE \"Id\" = {0} FOR UPDATE", item.MedicineId)
                            .FirstOrDefaultAsync();

                        if (medicine == null || !medicine.IsActive)
                        {
                            throw new KeyNotFoundException($"Dược phẩm có ID {item.MedicineId} không tồn tại hoặc đã ngừng cung cấp.");
                        }

                        // Kiểm tra số lượng tồn kho khả dụng
                        if (medicine.StockQuantity < item.Quantity)
                        {
                            throw new InsufficientStockException($"Thuốc '{medicine.Name}' trong kho hiện chỉ còn {medicine.StockQuantity} liều, không đủ số lượng yêu cầu: {item.Quantity}.");
                        }

                        // Trừ tồn kho thực tế
                        medicine.StockQuantity -= item.Quantity;

                        // Tạo PrescriptionItem
                        var prescriptionItem = new PrescriptionItem
                        {
                            PrescriptionId = prescription.Id,
                            MedicineId = item.MedicineId,
                            Quantity = item.Quantity,
                            DosageInstructions = item.DosageInstructions,
                            UnitPrice = medicine.Price
                        };

                        _context.PrescriptionItems.Add(prescriptionItem);
                        totalMedicineAmount += (medicine.Price * item.Quantity);
                    }
                }

                // 4. Cập nhật trạng thái lịch hẹn khám (Appointment) thành Completed
                var appointment = await _context.Appointments
                    .Include(a => a.Service)
                    .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

                if (appointment == null)
                {
                    throw new KeyNotFoundException("Lịch hẹn chỉ định khám không tồn tại.");
                }

                appointment.Status = "completed";
                appointment.EndExamTime = DateTime.UtcNow;

                // 5. Tự động sinh hóa đơn nháp (Draft Invoice) cho Thu ngân
                decimal servicePrice = appointment.Service?.Price ?? 150000; // Giá dịch vụ mặc định nếu null
                decimal totalInvoiceAmount = servicePrice + totalMedicineAmount;

                var invoice = new Invoice
                {
                    AppointmentId = appointment.Id,
                    TotalAmount = totalInvoiceAmount,
                    Status = "draft", // Chờ Lễ tân duyệt thu ngân ở sảnh
                    CreatedAt = DateTime.UtcNow
                };
                _context.Invoices.Add(invoice);

                // Lưu toàn bộ thay đổi
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return medicalRecord.Id;
            }
            catch (Exception)
            {
                // Rollback toàn bộ nếu có lỗi để bảo toàn kho dược
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
```
