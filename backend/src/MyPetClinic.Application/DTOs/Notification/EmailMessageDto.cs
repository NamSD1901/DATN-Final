namespace MyPetClinic.Application.DTOs.Notification
{
    public class EmailMessageDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
    }
}
