using System.Security.Claims;
using CarRental.Models;

namespace CarRental.Services
{
    public interface IUserService
    {
        Task<AppUser?> AuthenticateAsync(string login, string password);
        Task<ClaimsPrincipal> StartSessionAsync(AppUser user);
        Task EndSessionAsync(ClaimsPrincipal principal);
        Task<(bool Valid, int IdleMinutes)> ValidateSessionAsync(ClaimsPrincipal principal);
        Task<(bool Success, List<string> Errors)> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

        Task<List<AppUser>> GetAllUsersAsync();
        Task<AppUser?> GetUserByIdAsync(int id);
        Task<(bool Success, string Message)> CreateUserAsync(AppUser newUser);
        Task<(bool Success, string Message)> UpdateUserAsync(AppUser userToEdit);
        Task<(bool Success, string Message)> ResetPasswordAsync(int id);
        Task<(bool Success, string Message)> SetBlockedAsync(int id, bool blocked);
        Task<(bool Success, string Message)> DeleteUserAsync(int id);

        Task<SecuritySettings> GetSettingsAsync();
        Task SaveSettingsAsync(SecuritySettings settings);
    }
}
