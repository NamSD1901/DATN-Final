using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class VaccineBatchAdminDto
    {
        public long Id { get; set; }
        public long VaccineId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public DateTime ImportDate { get; set; }
        public int StockQuantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
    }

    public class CreateVaccineBatchDto
    {
        [Required]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ExpirationDate { get; set; }

        public DateTime? ImportDate { get; set; }

        [Range(0, 100000)]
        public int StockQuantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ImportPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }
    }

    public class VaccineAdminDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; } // Tổng từ các Batch
        public string? TargetSpecies { get; set; }
        public int? MinAgeWeeks { get; set; }
        public int? IntervalDays { get; set; }

        public ICollection<VaccineBatchAdminDto> Batches { get; set; } = new List<VaccineBatchAdminDto>();
    }

    public class CreateVaccineDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Description { get; set; }
        public string? TargetSpecies { get; set; }
        
        [Range(0, 1000)]
        public int? MinAgeWeeks { get; set; }
        
        [Range(0, 5000)]
        public int? IntervalDays { get; set; }
    }
}
