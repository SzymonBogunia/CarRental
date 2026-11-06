using CarRental.Models;
using CarRental.Security;
using CarRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages.Admin
{
    [Authorize(Policy = Permissions.UsersManage)]
    public class UserEditModel : PageModel
    {
        private readonly IUserService _userService;

        public UserEditModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public AppUser UserToEdit { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            UserToEdit = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _userService.UpdateUserAsync(UserToEdit);
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                UserToEdit = await _userService.GetUserByIdAsync(UserToEdit.Id) ?? UserToEdit;
                return Page();
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Admin/Users");
        }
    }
}
