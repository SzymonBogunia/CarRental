using CarRental.Data;
using CarRental.Models;
using CarRental.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Pages
{
    public class CarsListModel : PageModel
    {
        private readonly ICarService _carService;
        

        public CarsListModel(ICarService carService)
        {
            _carService = carService;
        }

        public List<Car> Cars { get; set; } = new List<Car>();

        [BindProperty]
        public Car CarToEdit { get; set; } = new Car();

        [BindProperty]
        public IFormFile? CarImageFile { get; set; }

        public async Task OnGetAsync()
        {
            Cars = await _carService.GetAllCarsAsync();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            ModelState.Remove("Cars");

            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            var result = await _carService.EditCarAsync(CarToEdit, CarImageFile);
            if (!result.Success)
            {
                return NotFound();
            }

            return RedirectToPage();
        }
    }
}