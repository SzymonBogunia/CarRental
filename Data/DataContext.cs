using CarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<PasswordHistory> PasswordHistory { get; set; }
        public DbSet<SecuritySettings> SecuritySettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<SecuritySettings>()
                .Property(s => s.Id)
                .ValueGeneratedNever();
        }
    }
}
