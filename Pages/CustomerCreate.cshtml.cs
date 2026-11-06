using CarRental.Models;
using CarRental.Services;
using CarRental.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages
{
    [Authorize(Policy = Permissions.CustomersManage)]
    public class CustomerCreateModel : PageModel
    {
        private readonly ICustomerService _customerService;

        public CustomerCreateModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [BindProperty]
        public Customer NewCustomer { get; set; } = new Customer();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Przekazujemy odpowiedzialnoœæ za walidacjê i zapis do serwisu
            var result = await _customerService.CreateCustomerAsync(NewCustomer);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return Page();
            }

            return RedirectToPage("/CustomersList");
        }
    }
}