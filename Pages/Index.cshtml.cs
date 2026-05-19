using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages // Upewnij siê, ¿e masz tu nazwê swojego projektu (CarRental)
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Pusta metoda na start
        }
    }
}