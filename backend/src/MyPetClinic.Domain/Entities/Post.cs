using System;

namespace MyPetClinic.Domain.Entities
{
    public class Post
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = "draft"; // draft, published, archived
        public int ViewCount { get; set; } = 0;
        public DateTime? PublishedAt { get; set; }
        
        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? Keywords { get; set; }
        
        // Relationships
        public long? CategoryId { get; set; }
        public PostCategory? Category { get; set; }
        
        public Guid? AuthorId { get; set; }
        public User? Author { get; set; }
        
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        
        // Audit Logs & Concurrency
        public Guid? UpdatedBy { get; set; }
        public User? Updater { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = new byte[8];
    }
}
