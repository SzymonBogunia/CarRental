using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

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
    }
}