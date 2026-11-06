using CarRental.Services;
using CarRental.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarRental.Models;

namespace CarRental.Pages
{
    [Authorize(Policy = Permissions.CustomersView)]
    public class CustomersListModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public CustomersListModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public List<Customer> Customers { get; set; } = new List<Customer>();

        [BindProperty]
        public Customer CustomerToEdit { get; set; } = new Customer();

        public async Task OnGetAsync()
        {
            Customers = await _customerService.GetAllCustomersAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!User.HasPermission(Permissions.RecordsDelete))
                return Forbid();

            await _customerService.DeleteCustomerAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!User.HasPermission(Permissions.CustomersManage))
                return Forbid();

            // Usuwamy weryfikacjê w³aœciwoœci, które obs³ugujemy niestandardowo lub ignorujemy w tym widoku
            ModelState.Remove("CustomerToEdit.Pesel");
            ModelState.Remove("Customers");

            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            var result = await _customerService.UpdateCustomerAsync(CustomerToEdit);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                // £adujemy dane ponownie, by widok nie rzuci³ b³êdu braku kolekcji przed prze³adowaniem strony
                Customers = await _customerService.GetAllCustomersAsync();
                return Page();
            }

            return RedirectToPage();
        }
    }
}