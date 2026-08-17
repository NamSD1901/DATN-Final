using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class MedicalRecord
    {
        public long Id { get; set; }
        
        // Liên kết
        public long AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public long PetId { get; set; }
        
        // Loại hồ sơ
        public string RecordType { get; set; } = "Consultation"; // "Consultation" hoặc "Vaccination"

        // S - Subjective (Chủ quan)
        public string? MedicalHistory { get; set; } 

        // O - Objective (Khách quan)
        public decimal Weight { get; set; }
        public decimal Temperature { get; set; }
        public string ClinicalSigns { get; set; } = string.Empty; // Triệu chứng hoặc Kết quả sàng lọc
        public string? Attachments { get; set; } // Danh sách URL ảnh cận lâm sàng (JSON string)

        // A - Assessment (Đánh giá)
        public string Diagnosis { get; set; } = string.Empty; // Chẩn đoán (hoặc "Đủ ĐK tiêm"/"Không đủ ĐK tiêm")
        
        // P - Plan (Kế hoạch)
        public string TreatmentPlan { get; set; } = string.Empty; // Hướng xử lý
        public string? DoctorNotes { get; set; } // Lời dặn dò
        public DateTime? FollowUpDate { get; set; } // Tái khám hoặc Tiêm nhắc lại
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Appointment? Appointment { get; set; }
        public User? Doctor { get; set; }
        public Pet? Pet { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
