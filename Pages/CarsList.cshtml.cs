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
            ModelState.Remove("Cars");

            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            // wgrywanie zdjecia
            if (CarImageFile != null && CarImageFile.Length > 0)
            {
                // nazwa
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CarImageFile.FileName);
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cars");

                Directory.CreateDirectory(uploadsFolder);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                if (!string.IsNullOrEmpty(CarToEdit.ImageUrl) && !CarToEdit.ImageUrl.Contains("default-car.jpg"))
                {
                    string oldFilePath = Path.Combine(_environment.WebRootPath, CarToEdit.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // wgrywanie do wwwroot/uploads/cars
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CarImageFile.CopyToAsync(fileStream);
                }

                CarToEdit.ImageUrl = "/uploads/cars/" + uniqueFileName;
            }

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