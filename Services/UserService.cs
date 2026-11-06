using System.Security.Claims;
using CarRental.Data;
using CarRental.Models;
using CarRental.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services
{
    public class UserService : IUserService
    {
        public const string AdminLogin = "ADMIN";
        public const string SessionClaim = "session_id";
        public const string MustChangePasswordClaim = "must_change_password";
        private const int PasswordHistorySize = 5;

        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        // ===== Logowanie i sesja =====

        // Zwraca null przy każdym niepowodzeniu - wywołujący pokazuje jeden wspólny komunikat
        public async Task<AppUser?> AuthenticateAsync(string login, string password)
        {
            var normalized = login.Trim().ToLower();
            var user = await _context.AppUsers.SingleOrDefaultAsync(u => u.Login.ToLower() == normalized);

            if (user == null)
            {
                PasswordService.VerifyDummy(password);
                return null;
            }

            if (!PasswordService.Verify(user.PasswordHash, password))
                return null;

            if (user.IsBlocked)
                return null;

            if (user.OneTimePasswordExpiresUtc != null && user.OneTimePasswordExpiresUtc < DateTime.UtcNow)
                return null;

            return user;
        }

        public async Task<ClaimsPrincipal> StartSessionAsync(AppUser user)
        {
            var settings = await GetSettingsAsync();

            var passwordExpired = settings.PasswordValidityDays > 0
                && user.PasswordChangedUtc.AddDays(settings.PasswordValidityDays) < DateTime.UtcNow;
            if (passwordExpired)
                user.MustChangePassword = true;

            user.SessionId = Guid.NewGuid().ToString();
            user.LastActivityUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Login),
                new(ClaimTypes.Role, user.Role.ToString()),
                new(SessionClaim, user.SessionId)
            };

            if (user.MustChangePassword)
            {
                // Dopóki hasło nie zostanie zmienione, sesja nie dostaje żadnych uprawnień
                claims.Add(new Claim(MustChangePasswordClaim, "true"));
            }
            else
            {
                claims.AddRange(Permissions.ByRole[user.Role].Select(p => new Claim(Permissions.ClaimType, p)));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        public async Task EndSessionAsync(ClaimsPrincipal principal)
        {
            var user = await FindByPrincipalAsync(principal);
            if (user != null)
            {
                user.SessionId = null;
                await _context.SaveChangesAsync();
            }
        }

        // Wywoływane przy każdym żądaniu: sprawdza, czy sesja nie została unieważniona i nie wygasła z powodu bezczynności
        public async Task<(bool Valid, int IdleMinutes)> ValidateSessionAsync(ClaimsPrincipal principal)
        {
            var settings = await GetSettingsAsync();
            var user = await FindByPrincipalAsync(principal);
            var sessionId = principal.FindFirstValue(SessionClaim);

            if (user == null || user.IsBlocked || user.SessionId == null || user.SessionId != sessionId)
                return (false, settings.SessionIdleMinutes);

            var now = DateTime.UtcNow;
            if (user.LastActivityUtc == null || user.LastActivityUtc.Value.AddMinutes(settings.SessionIdleMinutes) < now)
            {
                user.SessionId = null;
                await _context.SaveChangesAsync();
                return (false, settings.SessionIdleMinutes);
            }

            user.LastActivityUtc = now;
            await _context.SaveChangesAsync();
            return (true, settings.SessionIdleMinutes);
        }

        public async Task<(bool Success, List<string> Errors)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.AppUsers
                .Include(u => u.PasswordHistory)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null || !PasswordService.Verify(user.PasswordHash, oldPassword))
                return (false, new List<string> { "Stare hasło jest niepoprawne." });

            var errors = PasswordService.Validate(newPassword, user, await GetSettingsAsync());

            var previousHashes = user.PasswordHistory.Select(h => h.PasswordHash).Append(user.PasswordHash);
            if (previousHashes.Any(h => PasswordService.Verify(h, newPassword)))
                errors.Add("Nowe hasło musi różnić się od poprzednich haseł.");

            if (errors.Count > 0)
                return (false, errors);

            // Hasło jednorazowe nie trafia do historii - i tak musiało zostać zmienione
            if (user.OneTimePasswordExpiresUtc == null)
                user.PasswordHistory.Add(new PasswordHistory { PasswordHash = user.PasswordHash });

            foreach (var old in user.PasswordHistory.OrderByDescending(h => h.CreatedUtc).Skip(PasswordHistorySize).ToList())
                _context.PasswordHistory.Remove(old);

            user.PasswordHash = PasswordService.Hash(newPassword);
            user.PasswordChangedUtc = DateTime.UtcNow;
            user.MustChangePassword = false;
            user.OneTimePasswordExpiresUtc = null;
            await _context.SaveChangesAsync();

            return (true, new List<string>());
        }

        // ===== Zarządzanie użytkownikami (panel administratora) =====

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _context.AppUsers.OrderBy(u => u.Role).ThenBy(u => u.Login).ToListAsync();
        }

        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            return await _context.AppUsers.FindAsync(id);
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(AppUser newUser)
        {
            newUser.Login = newUser.Login.Trim();
            if (string.IsNullOrWhiteSpace(newUser.Login))
                return (false, "Login jest wymagany.");

            var normalized = newUser.Login.ToLower();
            if (await _context.AppUsers.AnyAsync(u => u.Login.ToLower() == normalized))
                return (false, "Użytkownik o takim loginie już istnieje.");

            if (newUser.Role == UserRole.Administrator)
                return (false, "Konto administratora jest tylko jedno (ADMIN).");

            var oneTimePassword = SetOneTimePassword(newUser);
            _context.AppUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return (true, OneTimePasswordMessage(newUser, oneTimePassword));
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(AppUser userToEdit)
        {
            var user = await _context.AppUsers.FindAsync(userToEdit.Id);
            if (user == null)
                return (false, "Nie znaleziono użytkownika.");

            if (IsAdminAccount(user) && userToEdit.Role != UserRole.Administrator)
                return (false, "Nie można zmienić roli konta ADMIN.");

            if (!IsAdminAccount(user) && userToEdit.Role == UserRole.Administrator)
                return (false, "Konto administratora jest tylko jedno (ADMIN).");

            if (user.Role != userToEdit.Role)
                user.SessionId = null; // nowa rola = nowe uprawnienia, więc wymuszamy ponowne logowanie

            user.FirstName = userToEdit.FirstName;
            user.LastName = userToEdit.LastName;
            user.Role = userToEdit.Role;
            await _context.SaveChangesAsync();

            return (true, $"Zapisano zmiany użytkownika {user.Login}.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return (false, "Nie znaleziono użytkownika.");

            var oneTimePassword = SetOneTimePassword(user);
            user.SessionId = null;
            await _context.SaveChangesAsync();

            return (true, OneTimePasswordMessage(user, oneTimePassword));
        }

        public async Task<(bool Success, string Message)> SetBlockedAsync(int id, bool blocked)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return (false, "Nie znaleziono użytkownika.");

            if (IsAdminAccount(user))
                return (false, "Nie można zablokować konta ADMIN.");

            user.IsBlocked = blocked;
            if (blocked)
                user.SessionId = null;
            await _context.SaveChangesAsync();

            return (true, blocked ? $"Zablokowano użytkownika {user.Login}." : $"Odblokowano użytkownika {user.Login}.");
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return (false, "Nie znaleziono użytkownika.");

            if (IsAdminAccount(user))
                return (false, "Nie można usunąć konta ADMIN.");

            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();

            return (true, $"Usunięto użytkownika {user.Login}.");
        }

        // ===== Ustawienia =====

        public async Task<SecuritySettings> GetSettingsAsync()
        {
            return await _context.SecuritySettings.FindAsync(1) ?? new SecuritySettings();
        }

        public async Task SaveSettingsAsync(SecuritySettings settings)
        {
            var current = await _context.SecuritySettings.FindAsync(1);
            if (current == null)
            {
                current = new SecuritySettings();
                _context.SecuritySettings.Add(current);
            }

            current.MinPasswordLength = settings.MinPasswordLength;
            current.PersonalDataRuleEnabled = settings.PersonalDataRuleEnabled;
            current.PasswordValidityDays = settings.PasswordValidityDays;
            current.SessionIdleMinutes = settings.SessionIdleMinutes;
            await _context.SaveChangesAsync();
        }

        // ===== Pomocnicze =====

        public static bool IsAdminAccount(AppUser user) => user.Login == AdminLogin;

        private static string SetOneTimePassword(AppUser user)
        {
            var oneTimePassword = PasswordService.GenerateOneTimePassword();
            user.PasswordHash = PasswordService.Hash(oneTimePassword);
            user.MustChangePassword = true;
            user.OneTimePasswordExpiresUtc = DateTime.UtcNow.Add(PasswordService.OneTimePasswordValidity);
            user.PasswordChangedUtc = DateTime.UtcNow;
            return oneTimePassword;
        }

        private static string OneTimePasswordMessage(AppUser user, string oneTimePassword)
        {
            var expires = user.OneTimePasswordExpiresUtc!.Value.ToLocalTime();
            return $"Hasło jednorazowe dla {user.Login}: {oneTimePassword} (ważne do {expires:dd.MM.yyyy HH:mm}). Przekaż je użytkownikowi - nie zostanie wyświetlone ponownie.";
        }

        private async Task<AppUser?> FindByPrincipalAsync(ClaimsPrincipal principal)
        {
            return int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? await _context.AppUsers.FindAsync(id)
                : null;
        }
    }
}
