using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarRental.Data;
using CarRental.Models;
using CarRental.Services;

namespace CarRental.Pages
{
    public class CarCreateModel : PageModel
    {
        private readonly ICarService _carService;

        public CarCreateModel(ICarService carService)
        {
            _carService = carService;
        }

        [BindProperty]
        public Car NewCar { get; set; } = new Car();

        [BindProperty]
        public IFormFile? CarImageFile { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _carService.CreateCarAsync(NewCar, CarImageFile);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return Page();
            }

            return RedirectToPage("/CarsList");

        }
    }
}