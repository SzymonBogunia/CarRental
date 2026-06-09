using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Pages
{
    public class RentalsListModel : PageModel
    {
        private readonly DataContext _context;

        public RentalsListModel(DataContext context)
        {
            _context = context;
        }
        public List<SelectListItem> CarOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerOptions { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public Rental RentalToEdit { get; set; } = new Rental();

        [BindProperty]
        public int RentalIdToDelete { get; set; }

        // Lista rezerwacji, któr¹ przeka¿emy do HTML-a
        public List<Rental> Rentals { get; set; } = new List<Rental>();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            // Pobieramy rezerwacje z bazy, do³¹czaj¹c dane relacyjne auta i klienta
            Rentals = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.StartDate) // Najnowsze rezerwacje na samej górze
                .ToListAsync();

            return Page();
        }
        private async Task LoadDropdownsAsync()
        {
            CarOptions = await _context.Cars
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Brand} {c.Model} ({c.RegistrationNumber})" })
                .ToListAsync();

            CustomerOptions = await _context.Customers
                .Select(cust => new SelectListItem { Value = cust.Id.ToString(), Text = $"{cust.FirstName} {cust.LastName}" })
                .ToListAsync();
        }

        // --- HANDLER EDYCJI ---
        public async Task<IActionResult> OnPostEditAsync()
        {
            ModelState.Remove("Rentals");

            // Zaokr¹glanie czasu do pe³nych godzin
            RentalToEdit.StartDate = new DateTime(RentalToEdit.StartDate.Year, RentalToEdit.StartDate.Month, RentalToEdit.StartDate.Day, RentalToEdit.StartDate.Hour, 0, 0);
            RentalToEdit.EndDate = new DateTime(RentalToEdit.EndDate.Year, RentalToEdit.EndDate.Month, RentalToEdit.EndDate.Day, RentalToEdit.EndDate.Hour, 0, 0);

            if (RentalToEdit.EndDate <= RentalToEdit.StartDate)
            {
                TempData["ErrorMessage"] = "Data zakoñczenia musi byæ póŸniejsza ni¿ data rozpoczêcia!";
                return RedirectToPage();
            }

            // Walidacja dostêpnoœci auta (z pominiêciem tej edytowanej rezerwacji)
            bool isCarOccupied = await _context.Rentals.AnyAsync(r =>
                r.Id != RentalToEdit.Id &&
                r.CarId == RentalToEdit.CarId &&
                RentalToEdit.StartDate < r.EndDate &&
                RentalToEdit.EndDate > r.StartDate
            );

            if (isCarOccupied)
            {
                TempData["ErrorMessage"] = "Ten samochód jest ju¿ zajêty w podanym terminie!";
                return RedirectToPage();
            }

            // Przeliczenie ceny na nowo
            var car = await _context.Cars.FindAsync(RentalToEdit.CarId);
            if (car != null)
            {
                TimeSpan duration = RentalToEdit.EndDate - RentalToEdit.StartDate;
                double days = Math.Ceiling(duration.TotalDays);
                if (days <= 0) days = 1;
                RentalToEdit.TotalPrice = (decimal)days * car.PricePerDay;
            }

            _context.Attach(RentalToEdit).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // --- HANDLER USUWANIA ---
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var rental = await _context.Rentals.FindAsync(RentalIdToDelete);
            if (rental != null)
            {
                var car = await _context.Cars.FindAsync(rental.CarId);
                if (car != null)
                {
                    car.Status = CarStatus.Available;
                }
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}