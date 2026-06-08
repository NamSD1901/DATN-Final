using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces;

public interface IAiChatbotService
{
    // Cung cấp giao diện để người dùng chat và nhận phản hồi từ AI
    Task<string> ChatAsync(string userMessage);
}
