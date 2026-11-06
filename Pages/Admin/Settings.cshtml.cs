using CarRental.Models;
using CarRental.Security;
using CarRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages.Admin
{
    [Authorize(Policy = Permissions.UsersManage)]
    public class SettingsModel : PageModel
    {
        private readonly IUserService _userService;

        public SettingsModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public SecuritySettings Settings { get; set; } = new();

        public async Task OnGetAsync()
        {
            Settings = await _userService.GetSettingsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await _userService.SaveSettingsAsync(Settings);
            TempData["SuccessMessage"] = "Zapisano ustawienia bezpieczeństwa.";
            return RedirectToPage();
        }
    }
}
