using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs.Notification
{
    public class UpdateNotificationStatusRequest
    {
        [Required(ErrorMessage = "Mã thông báo không được để trống")]
        public Guid NotificationId { get; set; }

        [Required(ErrorMessage = "Trạng thái đã đọc bắt buộc truyền")]
        public bool IsRead { get; set; }
    }
}
