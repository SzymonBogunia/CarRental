using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Pages
{
    public class CarCreateModel : PageModel
    {
        private readonly DataContext _context;

        public CarCreateModel(DataContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        [BindProperty]
        public Car NewCar { get; set; } = new Car();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // domyslne zdjecie 
            
            if (string.IsNullOrWhiteSpace(NewCar.ImageUrl))
            {
                NewCar.ImageUrl = null;
            }

            // domyœlnie nowe auto ustawiamy jako Dostêpne (0)
            NewCar.Status = 0;

            _context.Cars.Add(NewCar);
            await _context.SaveChangesAsync();

            return RedirectToPage("/CarsList");
        }
    }
}