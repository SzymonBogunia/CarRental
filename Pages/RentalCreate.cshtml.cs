using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Pages
{
    public class RentalCreateModel : PageModel
    {
        private readonly DataContext _context;

        public RentalCreateModel(DataContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Rental NewRental { get; set; } = new Rental();

        public List<SelectListItem> CarOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerOptions { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public Rental RentalToEdit { get; set; } = new Rental();

        [BindProperty]
        public int RentalIdToDelete { get; set; }

        

        private async Task LoadDropdownsAsync()
        {
            CarOptions = await _context.Cars
                .Where(c => c.Status == CarStatus.Available) // Wyœwietlamy tylko aktualnie dostêpne auta
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Brand} {c.Model} ({c.RegistrationNumber}) - {c.PricePerDay:F2} z³/dobê"
                }).ToListAsync();

            CustomerOptions = await _context.Customers
                .Select(cust => new SelectListItem
                {
                    Value = cust.Id.ToString(),
                    Text = $"{cust.FirstName} {cust.LastName} - {(string.IsNullOrEmpty(cust.Pesel) ? "Paszport: " + cust.PassportNumber : "PESEL: " + cust.Pesel)}"
                }).ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();

            // Domyœlne daty
            NewRental.StartDate = DateTime.Now;
            NewRental.EndDate = DateTime.Now.AddDays(1);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            
            NewRental.StartDate = new DateTime(NewRental.StartDate.Year, NewRental.StartDate.Month, NewRental.StartDate.Day, NewRental.StartDate.Hour, 0, 0);
            NewRental.EndDate = new DateTime(NewRental.EndDate.Year, NewRental.EndDate.Month, NewRental.EndDate.Day, NewRental.EndDate.Hour, 0, 0);

            var now = DateTime.Now;

            // 2. Podstawowa walidacja chronologii dat
            if (NewRental.EndDate <= NewRental.StartDate)
            {
                ModelState.AddModelError(string.Empty, "Data zakoñczenia musi byæ póŸniejsza ni¿ data rozpoczêcia!");
            }

            //WALIDACJA DOSTÊPNOŒCI POJAZDU
            if (ModelState.IsValid)
            {
                // Szukamy w bazie rezerwacji dla tego samego auta, które nak³adaj¹ siê na nowy termin
                bool isCarOccupied = await _context.Rentals.AnyAsync(r =>
                    r.CarId == NewRental.CarId &&
                    NewRental.StartDate < r.EndDate &&
                    NewRental.EndDate > r.StartDate
                );

                if (isCarOccupied)
                {
                    ModelState.AddModelError(string.Empty, "Ten samochód jest ju¿ zarezerwowany w wybranym przedziale czasowym! Wybierz inny termin lub inne auto.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            var car = await _context.Cars.FindAsync(NewRental.CarId);
            if (car == null)
            {
                ModelState.AddModelError(string.Empty, "Wybrany samochód nie istnieje.");
                await LoadDropdownsAsync();
                return Page();
            }

            //Automatyczne obliczanie ceny
            TimeSpan rentalDuration = NewRental.EndDate - NewRental.StartDate;
            double days = Math.Ceiling(rentalDuration.TotalDays);
            if (days <= 0) days = 1;

            NewRental.TotalPrice = (decimal)days * car.PricePerDay;
            car.Status = CarStatus.Rented;

            //Zapis 
            _context.Rentals.Add(NewRental);

            if (NewRental.StartDate <= now && NewRental.EndDate > now)
            {
                //dodawanie zaczetej ale nie skonczonej
                NewRental.Status = RentalStatus.Active;
                car.Status = CarStatus.Rented;
            }
            else if (NewRental.StartDate > now)
            {
                //dodawanie zaplanowanej
                NewRental.Status = RentalStatus.Planned;
                car.Status = CarStatus.Available;
            }
            else if (NewRental.EndDate <= now)
            {
                // dodawanie zaleg³ej (skonczonej)
                NewRental.Status = RentalStatus.ToBeSettled;
                car.Status = CarStatus.Rented;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/RentalsList");
        }
    }
}