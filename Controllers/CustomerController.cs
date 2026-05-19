using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WypozyczalniaSamochodowa.Models;

namespace CarRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly DataContext _context;

        public CustomersController(DataContext context)
        {
            _context = context;
        }

        // wszyscy klienci
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        // klient po id
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

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
            // Walidacja unikalności PESEL / Dokumentu Tożsamości
            if (!string.IsNullOrEmpty(customer.Pesel))
            {
                var peselExists = await _context.Customers.AnyAsync(c => c.Pesel == customer.Pesel);
                if (peselExists)
                {
                    return BadRequest("Klient o podanym numerze PESEL jest już zarejestrowany.");
                }
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

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

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Customers.AnyAsync(c => c.Id == id))
                {
                    return NotFound($"Nie można zaktualizować. Klient o ID {id} nie istnieje.");
                }
                throw;
            }

            return NoContent();
        }

        // usuwanie klienta

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound($"Klient o ID {id} nie istnieje.");
            }

            // Reguła biznesowa: Nie pozwalamy usunąć klienta, który ma aktywne wypożyczenia
            var hasActiveRentals = await _context.Rentals
                .AnyAsync(r => r.CustomerId == id && r.Status == RentalStatus.Active);

            if (hasActiveRentals)
            {
                return BadRequest("Nie można usunąć klienta, który aktualnie posiada wypożyczone auto.");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Klient {customer.FirstName} {customer.LastName} został pomyślnie usunięty." });
        }
    }
}