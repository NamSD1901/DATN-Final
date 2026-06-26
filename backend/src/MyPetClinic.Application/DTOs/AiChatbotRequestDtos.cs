namespace MyPetClinic.Application.DTOs
{
    /// <summary>
    /// Request DTO cho AI Chatbot — chứa nội dung tin nhắn của người dùng.
    /// </summary>
    public class AiChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
    }
}
