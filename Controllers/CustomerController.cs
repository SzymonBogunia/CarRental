using Microsoft.AspNetCore.Mvc;
using CarRental.Models;
using CarRental.Services;

namespace CarRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // wszyscy klienci
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        // klient po id
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound($"Klient o ID {id} nie istnieje w systemie.");
            }

            return Ok(customer);
        }

        // nowy klient
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
        {
            var result = await _customerService.CreateCustomerAsync(customer);

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(customer);
        }

        // aktualizacja danych klienta
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest("ID w ścieżce różni się od ID w przesłanym obiekcie.");
            }

            var result = await _customerService.UpdateCustomerAsync(customer);

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage);
            }

            return NoContent();
        }

        // usuwanie klienta
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            
            var result = await _customerService.DeleteCustomerAsync(id);

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { message = $"Klient o ID {id} został pomyślnie usunięty." });
        }
    }
}