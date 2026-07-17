using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyPetClinic.Application.Helpers
{
    public static class SlotCalculationHelper
    {
        /// <summary>
        /// Sinh danh sách các khung giờ bắt đầu dưới dạng DateTime trong ca trực.
        /// </summary>
        public static List<DateTime> GenerateSlots(DateTime workDate, TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes = 30)
        {
            var slots = new List<DateTime>();
            if (slotDurationMinutes <= 0) return slots;

            // Xác định thời gian bắt đầu và kết thúc cụ thể của ca trực
            var startDateTime = workDate.Date.Add(startTime);
            var endDateTime = workDate.Date.Add(endTime);

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
        /// </summary>
        public static List<DateTime> GetAvailableSlots(
            DoctorSchedule schedule,
            IEnumerable<Appointment> existingAppointments,
            IEnumerable<BlockTime> blockTimes = null,
            int slotDurationMinutes = 30)
        {
            if (schedule == null || !schedule.IsAvailable)
            {
                return new List<DateTime>();
            }

            // Sinh toàn bộ các slot trong ca trực của ngày đó
            var allSlots = GenerateSlots(schedule.WorkDate, schedule.StartTime, schedule.EndTime, slotDurationMinutes);

            // Chỉ lấy các lịch hẹn không bị hủy (Status != "cancelled")
            var activeAppointments = existingAppointments
                .Where(a => a.Status != "cancelled")
                .ToList();

            var activeBlocks = blockTimes?.ToList() ?? new List<BlockTime>();

            var availableSlots = new List<DateTime>();

            var now = DateTime.Now;

            foreach (var slot in allSlots)
            {
                // Bỏ qua các slot trong quá khứ (cộng thêm 15 phút buffer để tránh book quá sát giờ)
                if (slot < now.AddMinutes(15))
                {
                    continue;
                }

                var slotEndTime = slot.AddMinutes(slotDurationMinutes);

                // Kiểm tra xem có lịch hẹn nào trùng hoặc cách slot dưới slotDurationMinutes không (giãn cách cứng)
                var hasConflict = activeAppointments.Any(appt =>
                {
                    var apptTime = appt.AppointmentDate.ToLocalTime().Date.Add(appt.StartTime);
                    var diffMinutes = Math.Abs((apptTime - slot).TotalMinutes);
                    return diffMinutes < slotDurationMinutes;
                });

                // Kiểm tra xem slot có bị đè bởi BlockTime nào không
                var isBlocked = activeBlocks.Any(b =>
                {
                    // BlockTime dùng DateTimeOffset, chuyển về DateTime Local để so sánh
                    var blockStart = b.StartTime.LocalDateTime;
                    var blockEnd = b.EndTime.LocalDateTime;

                    // Nếu slot bắt đầu trước khi block kết thúc VÀ slot kết thúc sau khi block bắt đầu -> Chồng lấp
                    return slot < blockEnd && slotEndTime > blockStart;
                });

                if (!hasConflict && !isBlocked)
                {
                    availableSlots.Add(slot);
                }
            }

            return availableSlots;
        }
    }
}
