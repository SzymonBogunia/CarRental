using Microsoft.AspNetCore.Mvc;
using CarRental.Security;
using Microsoft.AspNetCore.Authorization;
using CarRental.Models;
using CarRental.Services;

namespace CarRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        // wszystkie samochody
        [HttpGet]
        [Authorize(Policy = Permissions.CarsViewDetails)]
        public async Task<ActionResult<IEnumerable<Car>>> GetCars()
        {
            var cars = await _carService.GetAllCarsAsync();
            return Ok(cars);
        }

        // samochód po id
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.CarsViewDetails)]
        public async Task<ActionResult<Car>> GetCar(int id)
        {
            var car = await _carService.GetCarByIdAsync(id);

            if (car == null)
            {
                return NotFound($"Nie znaleziono samochodu o ID {id}.");
            }

            return Ok(car);
        }

        // dodanie auta
        [HttpPost]
        [Authorize(Policy = Permissions.CarsManage)]
        public async Task<ActionResult<Car>> CreateCar([FromBody] Car car)
        {
            
            var result = await _carService.CreateCarAsync(car, null);

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(car);
        }

        // edycja auta
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.CarsManage)]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car car)
        {
            if (id != car.Id)
            {
                return BadRequest("ID w ścieżce nie zgadza się z ID obiektu.");
            }

            var result = await _carService.EditCarAsync(car, null);

            if (!result.Success)
            {
                return NotFound($"Nie można zaktualizować. Samochód o ID {id} nie istnieje.");
            }

            return NoContent();
        }

        // zmiana statusu 
        [HttpPatch("{id}/status")]
        [Authorize(Policy = Permissions.CarsManage)]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] CarStatus newStatus)
        {
            var result = await _carService.ChangeCarStatusAsync(id, newStatus);

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(new { message = "Status samochodu został pomyślnie zmieniony." });
        }

    }
}