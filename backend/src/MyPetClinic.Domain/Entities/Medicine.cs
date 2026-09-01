using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyPetClinic.Domain.Entities
{
    public class Medicine
    {
        public long Id { get; set; }
        public string MedicineCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        
        public long CategoryId { get; set; }
        public MedicineCategory Category { get; set; } = null!;

        public string Unit { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
        public decimal SellPrice { get; set; }
        
        public int MinStockLevel { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }

        public ICollection<MedicineBatch> Batches { get; set; } = new List<MedicineBatch>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

        [NotMapped]
        public int StockQuantity { get { return Batches?.Where(b => b.ExpiryDate.ToUniversalTime() > DateTime.UtcNow).Sum(b => b.CurrentQuantity) ?? 0; } set { } }
        [NotMapped]
        public DateTime? ExpiryDate { get { return Batches?.OrderBy(b => b.ExpiryDate).FirstOrDefault()?.ExpiryDate; } set { } }
    }
}
