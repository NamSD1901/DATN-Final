using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
