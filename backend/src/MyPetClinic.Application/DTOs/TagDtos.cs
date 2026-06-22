namespace MyPetClinic.Application.DTOs
{
    public class TagDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateTagDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTagDto : CreateTagDto
    {
    }
}
