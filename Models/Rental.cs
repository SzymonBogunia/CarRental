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

        // Relacja z klientem
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
    }

    public enum RentalStatus
    {
        Planned,    
        Active,     
        Completed,  
        Cancelled   
    }
}