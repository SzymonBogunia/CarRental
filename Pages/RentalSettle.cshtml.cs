using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
// Zmieñ poni¿sze usingi na zgodne z Twoim projektem (lokalizacja modeli i bazy)
using CarRental.Models;

namespace CarRental.Pages
{
    public class RentalSettleModel : PageModel
    {
        private readonly CarRental.Data.DataContext _context; // Twój DBContext

        public RentalSettleModel(CarRental.Data.DataContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RentalSettleViewModel SettleData { get; set; } = new();

        public Rental Rental { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // ladujemy oryginaln¹ rezerwacjê z bazy RAZEM z powi¹zanym pojazdem (Include)
            var rentalToUpdate = await _context.Rentals
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == SettleData.RentalId);

            if (rentalToUpdate == null)
            {
                return NotFound();
            }

            // dodatkowe oplaty
            decimal extraFees = SettleData.CleaningFee + SettleData.FuelDeficitFee + SettleData.DamageFee;
            rentalToUpdate.TotalPrice += extraFees;


            rentalToUpdate.Status = RentalStatus.Completed;

            rentalToUpdate.Comments = SettleData.Comments;
            rentalToUpdate.CleaningFee = SettleData.CleaningFee;
            rentalToUpdate.FuelDeficitFee = SettleData.FuelDeficitFee;
            rentalToUpdate.DamageFee = SettleData.DamageFee;

            if (rentalToUpdate.Car != null)
            {
                rentalToUpdate.Car.Status = CarStatus.Available;
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Pomyœlnie rozliczono umowê #{rentalToUpdate.Id} i zwolniono pojazd {rentalToUpdate.Car?.Brand}.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Wyst¹pi³ b³¹d bazy danych przy zamykaniu umowy.";
                return RedirectToPage("/RentalsList");
            }

            return RedirectToPage("/RentalsList");
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Nie podano ID rezerwacji do rozliczenia.";
                return RedirectToPage("/RentalsList");
            }

            Rental = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Rental == null)
            {
                TempData["ErrorMessage"] = $"Nie znaleziono w bazie rezerwacji o ID #{id}.";
                return RedirectToPage("/RentalsList");
            }

            SettleData.RentalId = Rental.Id;

            return Page();
        }

        public class RentalSettleViewModel
        {
            public int RentalId { get; set; }
            public string? Comments { get; set; }
            public decimal CleaningFee { get; set; }
            public decimal FuelDeficitFee { get; set; }
            public decimal DamageFee { get; set; }
        }
    }
}