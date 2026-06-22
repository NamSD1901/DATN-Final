using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class PostCategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public long? ParentId { get; set; }
        public string? ParentName { get; set; }
        public List<PostCategoryDto> Children { get; set; } = new List<PostCategoryDto>();
    }

    public class CreatePostCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public long? ParentId { get; set; }
    }

    public class UpdatePostCategoryDto : CreatePostCategoryDto
    {
    }
}
