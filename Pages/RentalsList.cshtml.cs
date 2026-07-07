using CarRental.Models;
using CarRental.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarRental.Pages
{
    public class RentalsListModel : PageModel
    {
        private readonly IRentalService _rentalService;

        public RentalsListModel(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        public List<SelectListItem> CarOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerOptions { get; set; } = new List<SelectListItem>();

        public List<Rental> PlannedRentals { get; set; } = new List<Rental>();
        public List<Rental> ActiveRetnals { get; set; } = new List<Rental>();
        public List<Rental> ToBeSettledRentals { get; set; } = new List<Rental>();
        public List<Rental> FinishedRentals { get; set; } = new List<Rental>();

        [BindProperty]
        public Rental RentalToEdit { get; set; } = new Rental();

        [BindProperty]
        public int RentalIdToDelete { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await _rentalService.UpdatePlannedAndActiveRentalsStatusAsync();
            await LoadDropdownsAsync();

            var allRentals = await _rentalService.GetAllRentalsWithDetailsAsync();

            PlannedRentals = allRentals.Where(r => r.Status == RentalStatus.Planned).ToList();
            ActiveRetnals = allRentals.Where(r => r.Status == RentalStatus.Active).ToList();
            ToBeSettledRentals = allRentals.Where(r => r.Status == RentalStatus.ToBeSettled).ToList();
            FinishedRentals = allRentals.Where(r => r.Status == RentalStatus.Completed || r.Status == RentalStatus.Cancelled).ToList();

            return Page();
        }

        private async Task LoadDropdownsAsync()
        {
            CarOptions = await _rentalService.GetAllCarOptionsAsync();
            CustomerOptions = await _rentalService.GetSimpleCustomerOptionsAsync();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            ModelState.Remove("Rentals");

            var result = await _rentalService.EditRentalAsync(RentalToEdit);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToPage();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            await _rentalService.DeleteRentalAsync(RentalIdToDelete);
            return RedirectToPage();
        }
    }
}