using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IReceptionistService
    {
        // Trả về danh sách kết hợp cả Chủ và Thú cưng khi search 1 từ khóa bất kỳ
        Task<List<OmniSearchDto>> OmniSearchAsync(string query);

        // Lấy thông tin xem trước của Lịch hẹn thông qua QR Token
        Task<AppointmentPreviewDto> GetAppointmentPreviewByQrAsync(string qrToken);

        // Check-in một ca khám đã đặt trước
        Task<bool> CheckInAsync(CheckInRequestDto request);

        // Trả về danh sách thú cưng đang có mặt tại phòng khám (Waiting, InProgress, ReadyToPay)
        Task<List<QueueItemDto>> GetTodayQueueAsync();

        // Tạo nhanh Khách hàng mới + Thú cưng mới + Check-in ngay lập tức
        Task<long> CreateWalkInAsync(WalkInRequestDto request, System.Guid createdBy);
        
        // Cập nhật trạng thái kéo thả (Drag and Drop)
        Task<bool> UpdateQueueStatusAsync(long appointmentId, string newStatus);

        // Ghép nối ca cấp cứu ẩn danh với khách hàng thật
        Task<bool> UpdateEmergencyCustomerAsync(long appointmentId, System.Guid customerId, long petId);

        // Lấy danh sách bác sĩ đang hoạt động
        Task<List<DoctorDto>> GetActiveDoctorsAsync();

        // Tìm kiếm khách hàng theo số điện thoại (trả về cả pets)
        Task<CustomerWithPetsDto?> GetCustomerWithPetsByPhoneAsync(string phone);

        // --- Các hàm mới thêm từ việc Refactor ReceptionistController ---
        Task<IEnumerable<MyPetClinic.Domain.Entities.User>> GetAllCustomersAsync();
        Task<IEnumerable<MyPetClinic.Domain.Entities.User>> SearchCustomersAsync(string search);
        Task<CustomerDashboardDetailDto?> GetCustomerDashboardDetailAsync(System.Guid id);
        Task<PetDashboardDetailDto?> GetPetDashboardDetailAsync(long id);
        Task<System.Guid> CreateCustomerWithPetsAsync(CustomerCreateDto dto);
        Task<long> CreateAppointmentAsync(AppointmentCreateDto dto, System.Guid createdBy);
        Task<long> AddPetAsync(System.Guid customerId, MyPetClinic.Domain.Entities.Pet pet);
        Task UpdatePetAsync(long petId, MyPetClinic.Domain.Entities.Pet pet);
    }
}
