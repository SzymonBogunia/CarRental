using CarRental.Data;
using CarRental.Models;
using CarRental.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Pages
{
    public class RentalsListModel : PageModel
    {
        private readonly DataContext _context;
        private readonly ISystemClock _clock;

        public RentalsListModel(DataContext context, ISystemClock clock)
        {
            _context = context;
            _clock = clock;
        }
        public List<SelectListItem> CarOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerOptions { get; set; } = new List<SelectListItem>();

        public List<Rental> PlannedRentals { get; set; } = new List<Rental>();
        public List<Rental> ActiveRetnals { get; set; } = new List<Rental>();
        public List<Rental> ToBeSettledRentals { get; set; } = new List<Rental>();
        public List<Rental> FinishedRentals { get; set; } = new List<Rental>();

        [BindProperty]
        public Rental RentalToEdit { get; set; } = new Rental();

        [BindProperty]
        public int RentalIdToDelete { get; set; }

        // Lista rezerwacji, do html
        public List<Rental> Rentals { get; set; } = new List<Rental>();



        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            var now = DateTime.Now;
            TempData["ErrorMessage"] = $"DEBUG: Aktualny czas w systemie to: {now:dd.MM.yyyy HH:mm}";

            var rentalsToUpdate = await _context.Rentals
        .Where(r => r.Status == RentalStatus.Planned || r.Status == RentalStatus.Active)
        .ToListAsync();

            bool anyChanges = false;
            foreach (var rental in rentalsToUpdate)
            {
                // Jeœli by³a zaplanowana, a czas min¹³ -> staje siê Aktywna
                if (rental.Status == RentalStatus.Planned && rental.StartDate <= now)
                {
                    rental.Status = RentalStatus.Active;
                    anyChanges = true;
                }

                // Jeœli by³a aktywna, a czas min¹³ -> automatycznie wpada w "Do rozliczenia"
                if (rental.Status == RentalStatus.Active && rental.EndDate <= now)
                {
                    rental.Status = RentalStatus.ToBeSettled;
                    anyChanges = true;
                }
            }

            if (anyChanges)
            {
                await _context.SaveChangesAsync();
            }

            var allRentals = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            PlannedRentals = allRentals.Where(r => r.Status == RentalStatus.Planned).ToList();
            ActiveRetnals = allRentals.Where(r => r.Status == RentalStatus.Active).ToList();
            ToBeSettledRentals = allRentals.Where(r => r.Status == RentalStatus.ToBeSettled).ToList();
            FinishedRentals = allRentals.Where(r => r.Status == RentalStatus.Completed || r.Status == RentalStatus.Cancelled).ToList();
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

        // edycja
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

        // USUWANIE
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