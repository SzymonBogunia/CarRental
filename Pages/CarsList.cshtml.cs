using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Pages
{
    public class CarsListModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public CarsListModel(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<Car> Cars { get; set; } = new List<Car>();


        public async Task OnGetAsync()
        {
            Cars = await _context.Cars.ToListAsync();
        }

        [BindProperty]
        public Car CarToEdit { get; set; } = new Car();

        [BindProperty]
        public IFormFile? CarImageFile { get; set; }



        public async Task<IActionResult> OnPostEditAsync()
        {
            // Ignorujemy ewentualne puste b³êdy walidacji dla innych w³aœciwoœci listy
            ModelState.Remove("Cars");

            if (!ModelState.IsValid)
            {
                // Jeœli dane s¹ niepoprawne, odœwie¿amy stronê (b³êdy pojawi¹ siê na ekranie)
                return RedirectToPage();
            }

            // ---- NOWOŒÆ: OBS£UGA WGRYWANIA PLIKU ZDJÊCIA ----
            if (CarImageFile != null && CarImageFile.Length > 0)
            {
                // 1. Generujemy bezpieczn¹ i unikaln¹ nazwê pliku
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CarImageFile.FileName);
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cars");

                // 2. Upewniamy siê, ¿e folder docelowy w projekcie istnieje
                Directory.CreateDirectory(uploadsFolder);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3. [Opcjonalnie] Usuwamy poprzednie zdjêcie z dysku, by nie œmieciæ w projekcie
                if (!string.IsNullOrEmpty(CarToEdit.ImageUrl) && !CarToEdit.ImageUrl.Contains("default-car.jpg"))
                {
                    string oldFilePath = Path.Combine(_environment.WebRootPath, CarToEdit.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // 4. Strumieniowe kopiowanie pliku do folderu wwwroot/uploads/cars
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CarImageFile.CopyToAsync(fileStream);
                }

                // 5. Przypisujemy now¹ œcie¿kê do Twojej w³aœciwoœci ImageUrl
                CarToEdit.ImageUrl = "/uploads/cars/" + uniqueFileName;
            }
            // Jeœli CarImageFile == null, to CarToEdit.ImageUrl zachowa swoj¹ 
            // dotychczasow¹ wartoœæ przekazan¹ przez <input type="hidden"> w HTML.

            // Informujemy Entity Framework, ¿e ten obiekt zosta³ zmodyfikowany
            _context.Attach(CarToEdit).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                if (!_context.Cars.Any(e => e.Id == CarToEdit.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage();
        }
    }
}