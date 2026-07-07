using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarRental.Models;
using CarRental.Services;

namespace CarRental.Pages
{
    public class RentalSettleModel : PageModel
    {
        private readonly IRentalService _rentalService;

        public RentalSettleModel(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [BindProperty]
        public RentalSettleViewModel SettleData { get; set; } = new();
        public Rental Rental { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Nie podano ID rezerwacji do rozliczenia.";
                return RedirectToPage("/RentalsList");
            }

            var rental = await _rentalService.GetRentalForSettlementAsync(id.Value);
            if (rental == null)
            {
                TempData["ErrorMessage"] = $"Nie znaleziono w bazie rezerwacji o ID #{id}.";
                return RedirectToPage("/RentalsList");
            }

            Rental = rental;
            SettleData.RentalId = Rental.Id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _rentalService.SettleRentalAsync(SettleData);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = "Wyst¹pi³ b³¹d podczas zamykania umowy.";
                return RedirectToPage("/RentalsList");
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/RentalsList");
        }

        public class RentalSettleViewModel
        {
            public int RentalId { get; set; }
            public string? Comments { get; set; }
            public decimal CleaningFee { get; set; }
            public decimal FuelDeficitFee { get; set; }
            public decimal DamageFee { get; set; }
        }
    }
}