using CarRental.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;

        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public string Login { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public bool SessionExpired { get; set; }

        public void OnGet(bool expired = false)
        {
            SessionExpired = expired;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            var user = await _userService.AuthenticateAsync(Login ?? string.Empty, Password ?? string.Empty);
            if (user == null)
            {
                // Jeden komunikat dla każdego błędu - nie zdradza, czy login istnieje
                ErrorMessage = "Login lub hasło niepoprawne";
                return Page();
            }

            var principal = await _userService.StartSessionAsync(user);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.MustChangePassword)
                return RedirectToPage("/Account/ChangePassword");

            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }
    }
}
