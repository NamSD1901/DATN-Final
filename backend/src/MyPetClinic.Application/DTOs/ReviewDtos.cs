using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class ReviewDto
    {
        public long Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerAvatarUrl { get; set; }
        public long AppointmentId { get; set; }
        public string? ServiceName { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? PetName { get; set; }
        public string? PetBreed { get; set; }
        public string? PetAge { get; set; }
        public short Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public bool IsVerified { get; set; }
        public string? ClinicReply { get; set; }
        public DateTime? RepliedAt { get; set; }
        public int HelpfulCount { get; set; }
        public int LikeCount { get; set; }
        public string? ImageUrls { get; set; }
    }

    public class CreateReviewDto
    {
        [Required(ErrorMessage = "Mã lịch hẹn không được để trống")]
        public long AppointmentId { get; set; }

        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5 sao")]
        public short Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Nội dung đánh giá không được vượt quá 1000 ký tự")]
        public string? Comment { get; set; }

        public string? ImageUrls { get; set; }
    }

    public class UpdateReviewDto
    {
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5 sao")]
        public short Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Nội dung đánh giá không được vượt quá 1000 ký tự")]
        public string? Comment { get; set; }
    }

    public class ReviewStatisticsDto
    {
        public int Total { get; set; }
        public double AvgRating { get; set; }
        public Dictionary<short, int> RatingDistribution { get; set; } = new Dictionary<short, int>
        {
            { 5, 0 },
            { 4, 0 },
            { 3, 0 },
            { 2, 0 },
            { 1, 0 }
        };
    }
}
