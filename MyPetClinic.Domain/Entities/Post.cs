using System;

namespace MyPetClinic.Domain.Entities
{
    public class Post
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Thumbnail { get; set; }
        public string? Content { get; set; }
        public Guid? AuthorId { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? Author { get; set; }
    }
}
