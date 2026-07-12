using CarRental.Data;
using CarRental.Models;
using CarRental.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalsController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        // wszystkie rezerwacje
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rental>>> GetRentals()
        {
            var rentals = await _rentalService.GetAllRentalsWithDetailsAsync();
            return Ok(rentals);
        }

        // dodaj rezerwacje
        [HttpPost]
        public async Task<ActionResult<Rental>> CreateRental([FromBody] RentalDto rentalDto)
        {
            var result = await _rentalService.CreateRentalAsync(
                rentalDto.CarId,
                rentalDto.CustomerId,
                rentalDto.StartDate,
                rentalDto.EndDate
            );

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { message = "Rezerwacja została pomyślnie utworzona.", rental = result.Rental });
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