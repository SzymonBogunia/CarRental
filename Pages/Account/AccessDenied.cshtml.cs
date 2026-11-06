using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}
