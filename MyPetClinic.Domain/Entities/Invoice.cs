using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Invoice
    {
        public long Id { get; set; }
        public long AppointmentId { get; set; }
        public decimal Subtotal { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public string PaymentStatus { get; set; } = "unpaid";
        public string? PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Appointment? Appointment { get; set; }
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
