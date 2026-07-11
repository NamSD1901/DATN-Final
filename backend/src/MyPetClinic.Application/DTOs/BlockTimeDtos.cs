using System;
using System.ComponentModel.DataAnnotations;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.DTOs
{
    public class BlockTimeDto
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public BlockType BlockType { get; set; }
        public string? Reason { get; set; }
        public string? BackgroundColor { get; set; }
    }

    public class BlockTimeCreateDto
    {
        [Required]
        public Guid DoctorId { get; set; }
        
        [Required]
        public DateTimeOffset StartTime { get; set; }
        
        [Required]
        public DateTimeOffset EndTime { get; set; }
        
        [Required]
        public BlockType BlockType { get; set; }
        
        public string? Reason { get; set; }
        public string? BackgroundColor { get; set; }
    }

    public class BlockTimeUpdateDto
    {
        [Required]
        public DateTimeOffset StartTime { get; set; }
        
        [Required]
        public DateTimeOffset EndTime { get; set; }
        
        [Required]
        public BlockType BlockType { get; set; }
        
        public string? Reason { get; set; }
        public string? BackgroundColor { get; set; }
    }
}
