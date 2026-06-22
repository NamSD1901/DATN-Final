using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class PostCategory
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public long? ParentId { get; set; }
        public PostCategory? Parent { get; set; }
        public ICollection<PostCategory> Children { get; set; } = new List<PostCategory>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
