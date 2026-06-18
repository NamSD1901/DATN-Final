using System;

namespace MyPetClinic.Application.DTOs
{
    public class PostDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }

    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Thumbnail { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = "draft";
    }
}
