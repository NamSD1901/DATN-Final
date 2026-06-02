using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class InvoiceDto
    {
        public long Id { get; set; }
        public long AppointmentId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Client & Patient details
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public string PetName { get; set; } = null!;
        public string? PetSpecies { get; set; }
        public string? PetBreed { get; set; }
        public string? DoctorName { get; set; }

        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
    }
}
