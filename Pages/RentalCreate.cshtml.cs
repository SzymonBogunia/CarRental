using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CarRental.Models;
using CarRental.Services;

namespace CarRental.Pages
{
    public class RentalCreateModel : PageModel
    {
        private readonly IRentalService _rentalService;

        public RentalCreateModel(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [BindProperty]
        public Rental NewRental { get; set; } = new Rental();
        public List<SelectListItem> CarOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerOptions { get; set; } = new List<SelectListItem>();

        private async Task LoadDropdownsAsync()
        {
            CarOptions = await _rentalService.GetAvailableCarOptionsAsync();
            CustomerOptions = await _rentalService.GetCustomerOptionsAsync();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            NewRental.StartDate = DateTime.Now;
            NewRental.EndDate = DateTime.Now.AddDays(1);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            var result = await _rentalService.CreateRentalAsync(NewRental);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                await LoadDropdownsAsync();
                return Page();
            }

            return RedirectToPage("/RentalsList");
        }
    }
}