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

            var availableSlots = new List<DateTime>();

            foreach (var slot in allSlots)
            {
                // Kiểm tra xem có lịch hẹn nào trùng hoặc cách slot dưới slotDurationMinutes không (giãn cách cứng)
                var hasConflict = activeAppointments.Any(appt =>
                {
                    var apptTime = appt.AppointmentDate.Date.Add(appt.StartTime);
                    var diffMinutes = Math.Abs((apptTime - slot).TotalMinutes);
                    return diffMinutes < slotDurationMinutes;
                });

                if (!hasConflict)
                {
                    availableSlots.Add(slot);
                }
            }

            return availableSlots;
        }
    }
}
