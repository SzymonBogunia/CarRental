using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public int ProductionYear { get; set; }

        [Required]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(17)] 
        public string VIN { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public CarStatus Status { get; set; } = CarStatus.Available;
    }

    public enum CarStatus
    {
        Available,    
        Rented,       
        InService     
    }

}

