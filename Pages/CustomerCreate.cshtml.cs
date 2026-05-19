using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WypozyczalniaSamochodowa.Models;

namespace CarRental.Pages
{
    public class CustomerCreateModel : PageModel
    {
        private readonly DataContext _context;

        public CustomerCreateModel(DataContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        [BindProperty]
        public Customer NewCustomer { get; set; } = new Customer();

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCustomer.Pesel) && string.IsNullOrWhiteSpace(NewCustomer.PassportNumber))
            {
                // pesel lub nrpaszportu
                ModelState.AddModelError(string.Empty, "Wymagane jest podanie numeru PESEL (dla obywateli PL) lub numeru Paszportu (dla obcokrajowców)!");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Customers.Add(NewCustomer);
            await _context.SaveChangesAsync();

            return RedirectToPage("/CustomersList");
        }
    }
}