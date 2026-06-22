using System;

namespace MyPetClinic.Domain.Entities
{
    public class MedicineBatch
    {
        public long Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        
        public long MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
    }
}
