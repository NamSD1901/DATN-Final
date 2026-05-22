using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class ServiceCategory
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
