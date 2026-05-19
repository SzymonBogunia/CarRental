using CarRental.Data;
using CarRental.Data;
using CarRental.Models;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Pages
{
    public class CarsListModel : PageModel
    {
        private readonly DataContext _context;

        public CarsListModel(DataContext context)
        {
            _context = context;
        }

        public List<Car> Cars { get; set; } = new List<Car>();

        public async Task OnGetAsync()
        {
            Cars = await _context.Cars.ToListAsync();
        }

        [BindProperty]
        public Car CarToEdit { get; set; } = new Car();

        public async Task<IActionResult> OnPostEditAsync()
        {
            // Ignorujemy ewentualne puste b³êdy walidacji dla innych w³aœciwoœci listy
            ModelState.Remove("Cars");

            if (!ModelState.IsValid)
            {
                // Jeœli dane s¹ niepoprawne, odœwie¿amy stronê (b³êdy pojawi¹ siê na ekranie)
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(CarToEdit.ImageUrl))
            {
                CarToEdit.ImageUrl = null;
            }

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