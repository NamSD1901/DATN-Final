using System;

namespace MyPetClinic.Domain.Entities
{
    public class BlockTime
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public BlockType BlockType { get; set; }
        public string? Reason { get; set; }
        public string? BackgroundColor { get; set; }

        public User? Doctor { get; set; }
    }
}
