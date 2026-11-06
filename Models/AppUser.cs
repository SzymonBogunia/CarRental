using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    // Konto pracownika logującego się do systemu (nie mylić z Customer - klientem wypożyczalni)
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Login { get; set; } = string.Empty;

        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Gosc;

        // Hash Argon2id w formacie $argon2id$v=19$m=...,t=...,p=...$sól$hash (sól jest częścią zapisu)
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // true = hasło nadane przez admina (jednorazowe) lub wygasłe - wymuszona zmiana po zalogowaniu
        public bool MustChangePassword { get; set; }

        // Ważność hasła jednorazowego; null = zwykłe hasło ustawione przez użytkownika
        public DateTime? OneTimePasswordExpiresUtc { get; set; }

        public DateTime PasswordChangedUtc { get; set; } = DateTime.UtcNow;

        public bool IsBlocked { get; set; }

        // Identyfikator aktywnej sesji - zerowany przy wylogowaniu/wygaśnięciu, co unieważnia ciasteczko po stronie serwera
        [MaxLength(64)]
        public string? SessionId { get; set; }

        public DateTime? LastActivityUtc { get; set; }

        public List<PasswordHistory> PasswordHistory { get; set; } = new();
    }

    public class PasswordHistory
    {
        [Key]
        public int Id { get; set; }

        public int AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }

    // Ustawienia bezpieczeństwa edytowane w panelu administratora (zawsze jeden wiersz, Id = 1)
    public class SecuritySettings
    {
        [Key]
        public int Id { get; set; } = 1;

        [Range(8, 64)]
        public int MinPasswordLength { get; set; } = 12;

        // Reguła z wariantu 2: hasło nie może zawierać loginu ani imienia/nazwiska
        public bool PersonalDataRuleEnabled { get; set; } = true;

        // 0 = hasło nie wygasa
        [Range(0, 3650)]
        public int PasswordValidityDays { get; set; } = 90;

        [Range(1, 480)]
        public int SessionIdleMinutes { get; set; } = 15;
    }

    public enum UserRole
    {
        Administrator,
        Kierownik,
        Pracownik,
        Gosc
    }
}
