using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class PostDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public DateTime? PublishedAt { get; set; }
        
        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? Keywords { get; set; }
        
        // Relationships
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }

    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Thumbnail { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }
        
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? Keywords { get; set; }
        
        public long? CategoryId { get; set; }
    }

    public class UpdatePostDto : CreatePostDto
    {
    }
}
