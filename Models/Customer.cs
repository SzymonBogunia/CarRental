using System.ComponentModel.DataAnnotations;

namespace WypozyczalniaSamochodowa.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(11)] 
        public string Pesel { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? PassportNumber { get; set; }

        [Required]
        [MaxLength(30)]
        public string DriverLicenseNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }
}