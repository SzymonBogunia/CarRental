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

            // Domyœlnie nowe auto ustawiamy jako Dostêpne (0)
            NewCar.Status = 0;

            // Log deweloperski do sprawdzenia czy plik w ogóle trafia do backendu
            if (CarImageFile != null && CarImageFile.Length > 0)
            {
                Console.WriteLine($"[DEBUG] Plik odebrany prawid³owo: {CarImageFile.FileName}");

                try
                {
                    // Generujemy bezpieczn¹ nazwê pliku
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CarImageFile.FileName);

                    // Budowanie œcie¿ki bezwzglêdnej do folderu w wwwroot
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cars");
                    Console.WriteLine($"[DEBUG] Docelowa œcie¿ka na dysku: {uploadsFolder}");

                    // Tworzymy folder na dysku, jeœli jeszcze nie istnieje
                    Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Kopiujemy plik z pamiêci podrêcznej do docelowego folderu projektu
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await CarImageFile.CopyToAsync(fileStream);
                    }

                    // Przypisujemy relatywn¹ œcie¿kê (dostêpn¹ przez HTTP z poziomu przegl¹darki)
                    NewCar.ImageUrl = "/uploads/cars/" + uniqueFileName;
                    Console.WriteLine($"[DEBUG] Zapisano plik pomyœlnie. Œcie¿ka URL w bazie: {NewCar.ImageUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[B£¥D] Wyst¹pi³ wyj¹tek podczas zapisu pliku: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Wyst¹pi³ problem z zapisem pliku na serwerze.");
                    return Page();
                }
            }
            else
            {
                Console.WriteLine("[DEBUG] Brak pliku (CarImageFile jest NULL lub pusty). Ustawiam placeholder.");

                if (string.IsNullOrWhiteSpace(NewCar.ImageUrl))
                {
                    NewCar.ImageUrl = "/images/cars/default-car.jpg";
                }
            }

            // Zapis danych pojazdu do bazy danych
            _context.Cars.Add(NewCar);
            await _context.SaveChangesAsync();

            return RedirectToPage("/CarsList");
        }
    }
}