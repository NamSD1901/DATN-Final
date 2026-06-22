using System;

namespace MyPetClinic.Domain.Entities
{
    public class InventoryTransaction
    {
        public long Id { get; set; }
        
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        public InventoryTransactionType Type { get; set; }
        
        public long MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public long? BatchId { get; set; }
        public MedicineBatch? Batch { get; set; }

        public int QuantityChange { get; set; }
        
        public Guid CreatedByUserId { get; set; }
        
        public string? ReferenceCode { get; set; }
        public string? Notes { get; set; }
    }
}
