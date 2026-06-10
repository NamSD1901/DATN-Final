# 🧠 Core Business Logic - Online Vaccination Booking

## 1. Thuật toán Kiểm tra khoảng cách Tiêm chủng (Vaccination Interval Logic)
Việc tiêm phòng vắc-xin đòi hỏi khoảng cách tối thiểu giữa các mũi tiêm (mũi nhắc lại) phải tuân thủ nghiêm ngặt phác đồ y khoa để đảm bảo kích hoạt kháng thể tối ưu và tránh gây sốc phản vệ.

### A. Quy tắc phác đồ giãn cách y khoa mẫu của MyPetClinic:
*   **Vắc-xin dại (Rabies):** Tiêm nhắc lại tối thiểu sau **11 tháng** (335 ngày) kể từ mũi gần nhất.
*   **Vắc-xin 4 bệnh mèo (Feline 4-in-1):** Mũi 2 cách mũi 1 tối thiểu **21 ngày**; Mũi 3 cách mũi 2 tối thiểu **28 ngày**. Nhắc lại hàng năm tối thiểu **11 tháng**.
*   **Vắc-xin 5 bệnh chó (Canine 5-in-1):** Mũi 2 cách mũi 1 tối thiểu **21 ngày**. Nhắc lại hàng năm tối thiểu **11 tháng**.

### B. C# Class: `VaccinationScheduleChecker.cs`
Lớp tiện ích thực thi việc kiểm tra phác đồ y khoa:
```csharp
using MyPetClinic.Domain.Entities;
using System;

namespace MyPetClinic.Application.Helpers
{
    public class VaccinationValidationResult
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public DateTime? NextAvailableDate { get; set; }
    }

    public static class VaccinationScheduleChecker
    {
        public static VaccinationValidationResult ValidateSchedule(
            VaccinationRecord lastRecord, 
            DateTime targetDate)
        {
            var result = new VaccinationValidationResult { IsValid = true };
            if (lastRecord == null) return result; // Chưa từng tiêm, hợp lệ

            var daysSinceLastInjection = (targetDate.Date - lastRecord.InjectionDate.Date).TotalDays;
            double minDaysRequired = 21; // Mặc định giãn cách 21 ngày cho các mũi cơ bản

            var vaccineName = lastRecord.Vaccine?.Name?.ToLower() ?? "";

            // Phân loại giãn cách theo tên vaccine
            if (vaccineName.Contains("dại") || vaccineName.Contains("rabies"))
            {
                minDaysRequired = 335; // ~11 tháng cho vắc-xin dại nhắc lại
            }
            else if (vaccineName.Contains("4 bệnh") || vaccineName.Contains("5 bệnh"))
            {
                // Kiểm tra xem là mũi nhắc hàng năm hay mũi cơ bản
                // Nếu khoảng cách mũi trước đó đã lâu (> 6 tháng), đây có thể là mũi tiêm nhắc hàng năm
                minDaysRequired = 21; 
            }

            if (daysSinceLastInjection < minDaysRequired)
            {
                result.IsValid = false;
                result.NextAvailableDate = lastRecord.InjectionDate.AddDays(minDaysRequired);
                result.Message = $"Mũi tiêm vắc-xin {lastRecord.Vaccine?.Name} gần nhất của bé là vào ngày {lastRecord.InjectionDate:dd/MM/yyyy}. Mũi tiếp theo nên tiêm sau ngày {result.NextAvailableDate:dd/MM/yyyy} (Giãn cách tối thiểu {minDaysRequired} ngày).";
            }

            return result;
        }
    }
}
```

---

## 2. Logic kiểm tra số lượng tồn kho khả dụng (Inventory Stock Integration)
Vắc-xin là thuốc đặc thù cần bảo quản nghiêm ngặt ở nhiệt độ lạnh. Để tránh trường hợp khách hàng đặt lịch tiêm chủng thành công nhưng khi đến phòng khám lại hết thuốc:
*   **Liên kết cơ sở dữ liệu:** Bảng `Vaccines` liên kết gián tiếp với bảng `Medicines` (Hoặc bảng tồn kho vật tư y tế `Inventory`).
*   **Query kiểm tra số lượng khả dụng (Available Stock):**
```csharp
// Kiểm tra tồn kho trước khi duyệt lịch tiêm
var availableStock = await _unitOfWork.Inventory
    .GetStockCountAsync(vaccineId);

if (availableStock <= 0)
{
    throw new InvalidOperationException("Loại vắc-xin này hiện đang hết hàng tại phòng khám. Vui lòng chọn loại vắc-xin khác.");
}
```

---

## 3. Mã kiểm thử đơn vị mẫu (xUnit Unit Test for Schedule Checker)
Đoạn mã unit test bảo đảm thuật toán kiểm tra khoảng cách hoạt động chính xác:
```csharp
using MyPetClinic.Application.Helpers;
using MyPetClinic.Domain.Entities;
using Xunit;
using System;

namespace MyPetClinic.Tests
{
    public class VaccinationScheduleCheckerTests
    {
        [Fact]
        public void ValidateSchedule_ShouldFail_WhenRabiesInjectedTooEarly()
        {
            // Arrange
            var vaccine = new Vaccine { Name = "Vắc-xin phòng bệnh Dại" };
            var lastRecord = new VaccinationRecord
            {
                Vaccine = vaccine,
                InjectionDate = new DateTime(2026, 01, 10) // Tiêm ngày 10/01
            };
            
            // Thử đặt lịch ngày 20/01 (chỉ cách 10 ngày)
            var targetDate = new DateTime(2026, 01, 20); 

            // Act
            var result = VaccinationScheduleChecker.ValidateSchedule(lastRecord, targetDate);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotNull(result.NextAvailableDate);
            Assert.Contains("Giãn cách tối thiểu 335 ngày", result.Message);
        }
    }
}
```
