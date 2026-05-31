using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IReceptionistService
    {
        // Trả về danh sách kết hợp cả Chủ và Thú cưng khi search 1 từ khóa bất kỳ
        Task<List<OmniSearchDto>> OmniSearchAsync(string query);

        // Check-in một ca khám đã đặt trước
        Task<bool> CheckInAsync(CheckInRequestDto request);

        // Trả về danh sách thú cưng đang có mặt tại phòng khám (Waiting, InProgress, ReadyToPay)
        Task<List<QueueItemDto>> GetTodayQueueAsync();

        // Tạo nhanh Khách hàng mới + Thú cưng mới + Check-in ngay lập tức
        Task<long> CreateWalkInAsync(WalkInRequestDto request, System.Guid createdBy);
        
        // Cập nhật trạng thái kéo thả (Drag and Drop)
        Task<bool> UpdateQueueStatusAsync(long appointmentId, string newStatus);
    }
}
