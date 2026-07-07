using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WypozyczalniaSamochodowa.Models;

namespace CarRental.Pages
{
    public class CustomersListModel : PageModel
    {
        private readonly DataContext _context;

        public CustomersListModel(DataContext context)
        {
            _context = context;
        }

        public List<Customer> Customers { get; set; } = new List<Customer>();

        public async Task OnGetAsync()
        {
            Customers = await _context.Customers.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        [BindProperty]
        public Customer CustomerToEdit { get; set; } = new Customer();

        public async Task<IActionResult> OnPostEditAsync()
        {
            //Rêcznie usuwamy b³¹d "required" dla pola Pesel
            ModelState.Remove("CustomerToEdit.Pesel");
            ModelState.Remove("Customers"); // Ignorujemy walidacjê g³ównej listy wyœwietlanej na stronie

            //albo PESEL, albo Paszport musi byæ podany
            if (string.IsNullOrWhiteSpace(CustomerToEdit.Pesel) && string.IsNullOrWhiteSpace(CustomerToEdit.PassportNumber))
            {
                ModelState.AddModelError(string.Empty, "Wymagane jest podanie numeru PESEL lub numeru Paszportu!");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            // modyfikacja klienta
            _context.Attach(CustomerToEdit).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                if (!_context.Customers.Any(e => e.Id == CustomerToEdit.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage();
        }
    }
}