using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyPetClinic.Application.Helpers
{
    public static class SlotCalculationHelper
    {
        /// <summary>
        /// Lớp phụ trợ để biểu diễn một khối thời gian bận (Busy Block)
        /// </summary>
        private class BusyBlock
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        /// <summary>
        /// Sinh danh sách các khung giờ bắt đầu dưới dạng DateTime trong ca trực.
        /// (Giữ lại hàm này cho tương thích ngược nếu cần, nhưng không dùng trong GetAvailableSlots mới)
        /// </summary>
        public static List<DateTime> GenerateSlots(DateTime workDate, TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes = 30)
        {
            var slots = new List<DateTime>();
            if (slotDurationMinutes <= 0) return slots;

            // Xác định thời gian bắt đầu và kết thúc cụ thể của ca trực
            var startDateTime = workDate.Date.Add(startTime);
            var effectiveEndTime = endTime == new TimeSpan(23, 59, 59) ? TimeSpan.FromDays(1) : endTime;
            var endDateTime = workDate.Date.Add(effectiveEndTime);

            var current = startDateTime;
            while (current + TimeSpan.FromMinutes(slotDurationMinutes) <= endDateTime)
            {
                slots.Add(current);
                current = current.AddMinutes(slotDurationMinutes);
            }

            return slots;
        }

        /// <summary>
        /// Lọc ra các slot khả dụng (Available Slots) sau khi đối chiếu với lịch hẹn đã có của bác sĩ.
        /// Sinh slot động dựa trên thời lượng dự kiến của dịch vụ (serviceDurationMinutes).
        /// Áp dụng Hướng B: Nối đuôi lịch cũ để tránh phân mảnh.
        /// </summary>
        public static List<DateTime> GetAvailableSlots(
            DoctorSchedule schedule,
            IEnumerable<Appointment> existingAppointments,
            IEnumerable<BlockTime>? blockTimes = null,
            int serviceDurationMinutes = 30)
        {
            if (schedule == null || !schedule.IsAvailable)
            {
                return new List<DateTime>();
            }

            var startDateTime = schedule.WorkDate.Date.Add(schedule.StartTime);
            var effectiveEndTime = schedule.EndTime == new TimeSpan(23, 59, 59) ? TimeSpan.FromDays(1) : schedule.EndTime;
            var endDateTime = schedule.WorkDate.Date.Add(effectiveEndTime);

            // 1. Tính toán bước nhảy (Step) - Làm tròn lên bội số của 30 phút
            int stepMinutes = Math.Max(30, (int)Math.Ceiling(serviceDurationMinutes / 30.0) * 30);

            // 2. Thu thập tất cả các khoảng thời gian bận (Busy Blocks)
            var busyBlocks = new List<BusyBlock>();

            var activeAppointments = existingAppointments
                .Where(a => a.Status != "cancelled" && a.Status != "completed") // Bỏ qua các status không chiếm lịch
                .ToList();

            foreach (var appt in activeAppointments)
            {
                var apptStart = appt.AppointmentDate.ToLocalTime().Date.Add(appt.StartTime);
                
                // Nếu EndTime có lưu trữ chuẩn, dùng EndTime. Nếu không, mặc định lấy StartTime + 30 phút (cho an toàn).
                var apptEnd = appt.EndTime.HasValue 
                    ? appt.AppointmentDate.ToLocalTime().Date.Add(appt.EndTime.Value) 
                    : apptStart.AddMinutes(30);

                busyBlocks.Add(new BusyBlock { Start = apptStart, End = apptEnd });
            }

            var activeBlocks = blockTimes?.ToList() ?? new List<BlockTime>();
            foreach (var block in activeBlocks)
            {
                busyBlocks.Add(new BusyBlock 
                { 
                    Start = block.StartTime.LocalDateTime, 
                    End = block.EndTime.LocalDateTime 
                });
            }

            // Sắp xếp các Busy Blocks theo StartTime tăng dần
            busyBlocks = busyBlocks.OrderBy(b => b.Start).ToList();

            // 3. Thuật toán tìm slot trống (Hướng B)
            var availableSlots = new List<DateTime>();
            var currentTime = startDateTime;
            var now = DateTime.Now;

            while (currentTime + TimeSpan.FromMinutes(stepMinutes) <= endDateTime)
            {
                // Bỏ qua các slot trong quá khứ (cộng thêm 15 phút buffer để tránh book quá sát giờ)
                if (currentTime < now.AddMinutes(15))
                {
                    // Tịnh tiến currentTime đến mốc 30 phút tiếp theo
                    currentTime = currentTime.AddMinutes(30);
                    continue;
                }

                var candidateEnd = currentTime.AddMinutes(stepMinutes);

                // Kiểm tra xem candidate (currentTime -> candidateEnd) có đè lên bất kỳ BusyBlock nào không
                var overlappingBlocks = busyBlocks.Where(b => currentTime < b.End && candidateEnd > b.Start).ToList();

                if (overlappingBlocks.Any())
                {
                    // Hướng B: Nối đuôi lịch cũ. Dịch chuyển currentTime đến lúc kết thúc của Block đè lấp muộn nhất
                    currentTime = overlappingBlocks.Max(b => b.End);
                }
                else
                {
                    // Không đè lên ai -> Thêm vào list và nhảy 1 bước
                    availableSlots.Add(currentTime);
                    currentTime = candidateEnd; // Tiếp tục từ cuối slot vừa sinh
                }
            }

            return availableSlots;
        }
    }
}
