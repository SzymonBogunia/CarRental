using CarRental.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WypozyczalniaSamochodowa.Models;

namespace CarRental.Models
{
    public class Rental
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }
        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public RentalStatus Status { get; set; } = RentalStatus.Planned;

        public bool IsOverdue => Status == RentalStatus.Active && EndDate < DateTime.Now;

        public string? Comments { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CleaningFee { get; set; } = 0.00m;
        [Column(TypeName = "decimal(18,2)")]
        public decimal FuelDeficitFee { get; set; } = 0.00m;
        [Column(TypeName = "decimal(18,2)")]
        public decimal DamageFee { get; set; } = 0.00m;

    }

    public enum RentalStatus
    {
        Planned,    
        Active,
        ToBeSettled,
        Completed,  
        Cancelled   
    }
}