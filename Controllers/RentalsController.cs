using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace WypozyczalniaSamochodowa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : ControllerBase
    {
        private readonly DataContext _context;

        public RentalsController(DataContext context)
        {
            _context = context;
        }

        // wszystkie rezerwacje
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rental>>> GetRentals()
        {
            return await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .ToListAsync();
        }

        // dodaj rezerwacje
        [HttpPost]
        public async Task<ActionResult<Rental>> CreateRental([FromBody] RentalDto rentalDto)
        {
            // Walidacja dat
            if (rentalDto.StartDate >= rentalDto.EndDate)
            {
                return BadRequest("Data rozpoczęcia musi być wcześniejsza niż data zakończenia.");
            }

            // Sprawdzenie samochodu
            var car = await _context.Cars.FindAsync(rentalDto.CarId);
            if (car == null) return NotFound("Samochód nie istnieje.");

            // Sprawdzenie klienta
            var customer = await _context.Customers.FindAsync(rentalDto.CustomerId);
            if (customer == null) return NotFound("Klient nie istnieje.");

            // Algorytm blokowania nakładających się terminów
            bool isCarOccupied = await _context.Rentals.AnyAsync(r =>
                r.CarId == rentalDto.CarId &&
                ((rentalDto.StartDate >= r.StartDate && rentalDto.StartDate < r.EndDate) ||
                 (rentalDto.EndDate > r.StartDate && rentalDto.EndDate <= r.EndDate) ||
                 (rentalDto.StartDate <= r.StartDate && rentalDto.EndDate >= r.EndDate)));

            if (isCarOccupied)
            {
                return BadRequest("Ten samochód jest już zarezerwowany w podanym terminie.");
            }

            // Wyliczenie ceny
            int rentalDays = (rentalDto.EndDate.Date - rentalDto.StartDate.Date).Days;
            if (rentalDays == 0) rentalDays = 1;

            decimal totalPrice = rentalDays * car.PricePerDay;

            // Tworzenie rezerwacji
            var rental = new Rental
            {
                CarId = rentalDto.CarId,
                CustomerId = rentalDto.CustomerId,
                StartDate = rentalDto.StartDate,
                EndDate = rentalDto.EndDate,
                TotalPrice = totalPrice
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rezerwacja została pomyślnie utworzona.", rental });
        }
    }

    // Klasa pomocnicza do odbierania danych z żądania
    public class RentalDto
    {
        public int CarId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}