using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class MedicineCategory
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    }
}
