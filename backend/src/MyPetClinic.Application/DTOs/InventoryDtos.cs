using System;
using System.Collections.Generic;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.DTOs
{
    public class MedicineBatchDto
    {
        public long Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public long MedicineId { get; set; }
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
    }

    public class InventoryTransactionDto
    {
        public long Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public InventoryTransactionType Type { get; set; }
        public string TypeName => Type.ToString();
        public long MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public long? BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string? ReferenceCode { get; set; }
        public string? Notes { get; set; }
    }

    public class ImportMedicineDto
    {
        public long MedicineId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public string? ReferenceCode { get; set; }
    }

    public class ExportMedicineDto
    {
        public long MedicineId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public string? ReferenceCode { get; set; }
    }

    public class AdjustMedicineDto
    {
        public long MedicineId { get; set; }
        public long BatchId { get; set; }
        public int QuantityChange { get; set; } // Positive for addition, Negative for subtraction
        public string? Notes { get; set; }
        public string? ReferenceCode { get; set; }
    }

    public class MedicineDetailDto : MedicineDto
    {
        public string MedicineCode { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int MinStockLevel { get; set; }
        public bool IsActive { get; set; }
        public new IEnumerable<MedicineBatchDto> Batches { get; set; } = new List<MedicineBatchDto>();
    }

    public class AuditMedicineDto
    {
        public long MedicineId { get; set; }
        public long BatchId { get; set; }
        public int ActualQuantity { get; set; }
        public string? Notes { get; set; }
        public string? ReferenceCode { get; set; }
    }

    public class ExpiringMedicineDto
    {
        public long MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public long BatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int CurrentQuantity { get; set; }
        public int DaysUntilExpiry { get; set; }
    }
}
