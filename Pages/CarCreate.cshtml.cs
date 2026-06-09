using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Pages
{
    public class CarCreateModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public CarCreateModel(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public void OnGet()
        {
        }

        [BindProperty]
        public Car NewCar { get; set; } = new Car();

        [BindProperty]
        public IFormFile? CarImageFile { get; set; }

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

            if (CarImageFile != null && CarImageFile.Length > 0)
            {
                // Generujemy bezpieczn¹ nazwê pliku
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CarImageFile.FileName);
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cars");

                // Tworzymy folder na dysku, jeœli jeszcze nie istnieje
                Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Kopiujemy plik z Pobranych/Pulpitu do folderu projektu
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CarImageFile.CopyToAsync(fileStream);
                }

                // Przypisujemy relatywn¹ œcie¿kê do Twojego pola ImageUrl
                NewCar.ImageUrl = "/uploads/cars/" + uniqueFileName;
            }
            else
            {
                // Jeœli pracownik nie da³ zdjêcia, wrzucamy domyœlny placeholder
                NewCar.ImageUrl = "/images/cars/default-car.jpg";
            }

            _context.Cars.Add(NewCar);


            await _context.SaveChangesAsync();

            return RedirectToPage("/CarsList");
        }
    }
}