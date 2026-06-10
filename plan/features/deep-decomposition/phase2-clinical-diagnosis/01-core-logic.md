# 🧠 Core Business Logic - Clinical Diagnosis & Treatment

## 🔗 Skills Liên Quan
- **BE-F01 (C# Fundamentals):** Phát hiện biệt dược trùng lặp trong đơn thuốc bằng HashSet/Linq.
- **BE-F03 (Async/Await):** Lưu trữ dữ liệu bất đồng bộ với cơ chế khóa hàng (Row Lock) trong DB tránh Race Condition.

---

## 1. C# Logic: Xử lý Giao dịch Bệnh án & Đơn thuốc (Atomic Transaction)

```csharp
public class MedicalRecordService
{
    private readonly MyPetClinicDbContext _context;

    public MedicalRecordService(MyPetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> CreateRecordAndDeductInventoryAsync(
        MedicalRecordCreationDto dto, 
        Guid doctorId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Tạo Medical Record
            var record = new MedicalRecord
            {
                AppointmentId = dto.AppointmentId,
                PetId = dto.PetId,
                DoctorId = doctorId,
                Symptoms = dto.Symptoms,
                Diagnosis = dto.Diagnosis,
                CreatedAt = DateTime.UtcNow
            };
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            // 2. Nếu có đơn thuốc, tiến hành lưu và kiểm tra kho
            if (dto.PrescriptionItems != null && dto.PrescriptionItems.Any())
            {
                var prescription = new Prescription
                {
                    MedicalRecordId = record.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                foreach (var item in dto.PrescriptionItems)
                {
                    // Truy vấn và khóa dòng dữ liệu của Medicine để cập nhật tồn kho an toàn
                    var medicine = await _context.Medicines
                        .FirstOrDefaultAsync(m => m.Id == item.MedicineId);

                    if (medicine == null)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Thuốc có ID {item.MedicineId} không tồn tại.");
                    }

                    if (medicine.StockQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Thuốc '{medicine.Name}' không đủ tồn kho (Yêu cầu: {item.Quantity}, Tồn: {medicine.StockQuantity}).");
                    }

                    // Trừ tồn kho
                    medicine.StockQuantity -= item.Quantity;

                    var prescriptionItem = new PrescriptionItem
                    {
                        PrescriptionId = prescription.Id,
                        MedicineId = item.MedicineId,
                        Quantity = item.Quantity,
                        DosageInstructions = item.DosageInstructions
                    };
                    _context.PrescriptionItems.Add(prescriptionItem);
                }
                await _context.SaveChangesAsync();
            }

            // 3. Cập nhật trạng thái lịch hẹn thành Completed
            var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Completed;
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Lỗi hệ thống: {ex.Message}");
        }
    }
}
```
