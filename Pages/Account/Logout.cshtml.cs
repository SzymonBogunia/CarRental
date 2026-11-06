using CarRental.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly IUserService _userService;

        public LogoutModel(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult OnGet() => RedirectToPage("/Index");

        // Unieważnia sesję w bazie (stare ciasteczko przestaje działać) i usuwa ciasteczko z przeglądarki
        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
                await _userService.EndSessionAsync(User);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Account/Login");
        }
    }
}
