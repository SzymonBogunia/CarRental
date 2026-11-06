using System.Security.Claims;
using CarRental.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IUserService _userService;

        public ChangePasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public string OldPassword { get; set; } = string.Empty;

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsForced => User.HasClaim(UserService.MustChangePasswordClaim, "true");
        public int MinLength { get; set; }
        public bool PersonalDataRuleEnabled { get; set; }
        public List<string> Errors { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadPolicyAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadPolicyAsync();

            if (NewPassword != ConfirmPassword)
            {
                Errors.Add("Nowe hasło i jego powtórzenie nie są takie same.");
                return Page();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (success, errors) = await _userService.ChangePasswordAsync(userId, OldPassword ?? string.Empty, NewPassword ?? string.Empty);
            if (!success)
            {
                Errors = errors;
                return Page();
            }

            // Nowa sesja z pełnymi uprawnieniami roli (stara sesja zostaje unieważniona)
            var user = (await _userService.GetUserByIdAsync(userId))!;
            var principal = await _userService.StartSessionAsync(user);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["SuccessMessage"] = "Hasło zostało zmienione.";
            return RedirectToPage("/Index");
        }

        private async Task LoadPolicyAsync()
        {
            var settings = await _userService.GetSettingsAsync();
            MinLength = settings.MinPasswordLength;
            PersonalDataRuleEnabled = settings.PersonalDataRuleEnabled;
        }
    }
}
