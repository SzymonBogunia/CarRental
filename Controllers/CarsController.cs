using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Controllers
{
    [ApiController]
    
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly DataContext _context;

        public CarsController(DataContext context)
        {
            _context = context;
        }

        // wszystkie samochody

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetCars()
        {
            var cars = await _context.Cars.ToListAsync();
            return Ok(cars);
        }

        // samochód po id
        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound($"Nie znaleziono samochodu o ID {id}.");
            }

            return Ok(car);
        }

        // dodanie auta
        [HttpPost]
        public async Task<ActionResult<Car>> CreateCar([FromBody] Car car)
        {
            var existingCar = await _context.Cars
                .AnyAsync(c => c.RegistrationNumber == car.RegistrationNumber || c.VIN == car.VIN);

            if (existingCar)
            {
                return BadRequest("Samochód o takim numerze rejestracyjnym lub VIN już istnieje w bazie.");
            }

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return Ok(car);
        }

        // edycja auta
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car car)
        {
            if (id != car.Id)
            {
                return BadRequest("ID w ścieżce nie zgadza się z ID obiektu.");
            }

            _context.Entry(car).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Cars.AnyAsync(c => c.Id == id))
                {
                    return NotFound($"Nie można zaktualizować. Samochód o ID {id} nie istnieje.");
                }
                throw;
            }

            return NoContent(); 
        }

        // zmiana statusu 
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] CarStatus newStatus)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound($"Nie znaleziono samochodu o ID {id}.");
            }

            car.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Status samochodu {car.Brand} {car.Model} został zmieniony na {newStatus}.", car });
        }

        // usuniecie auta
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound($"Nie znaleziono samochodu o ID {id}.");
            }

            if (car.Status == CarStatus.Rented)
            {
                return BadRequest("Nie można usunąć samochodu, który jest aktualnie wypożyczony.");
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Samochód o ID {id} został pomyślnie usunięty z floty." });
        }
    }
}