using CarRental.Models;
using CarRental.Security;
using CarRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental.Pages.Admin
{
    [Authorize(Policy = Permissions.UsersManage)]
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;

        public UsersModel(IUserService userService)
        {
            _userService = userService;
        }

        public List<AppUser> Users { get; set; } = new();

        [BindProperty]
        public AppUser NewUser { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _userService.GetAllUsersAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            return ShowResult(await _userService.CreateUserAsync(new AppUser
            {
                Login = NewUser.Login ?? string.Empty,
                FirstName = NewUser.FirstName ?? string.Empty,
                LastName = NewUser.LastName ?? string.Empty,
                Role = NewUser.Role
            }));
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(int id) => ShowResult(await _userService.ResetPasswordAsync(id));

        public async Task<IActionResult> OnPostBlockAsync(int id) => ShowResult(await _userService.SetBlockedAsync(id, true));

        public async Task<IActionResult> OnPostUnblockAsync(int id) => ShowResult(await _userService.SetBlockedAsync(id, false));

        public async Task<IActionResult> OnPostDeleteAsync(int id) => ShowResult(await _userService.DeleteUserAsync(id));

        private IActionResult ShowResult((bool Success, string Message) result)
        {
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToPage();
        }
    }
}
