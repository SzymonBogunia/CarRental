using System.Security.Cryptography;
using CarRental.Models;
using Isopoh.Cryptography.Argon2;

namespace CarRental.Security
{
    // Hashowanie haseł (gotowa biblioteka Argon2id), polityka haseł i generator haseł jednorazowych
    public static class PasswordService
    {
        public const int MaxPasswordLength = 128;
        public const int OneTimePasswordLength = 16;
        public static readonly TimeSpan OneTimePasswordValidity = TimeSpan.FromHours(12);

        // Hash używany, gdy login nie istnieje - weryfikacja trwa tyle samo, więc czas odpowiedzi nie zdradza istnienia konta
        private static readonly Lazy<string> DummyHash = new(() => Hash(Guid.NewGuid().ToString()));

        // Krótka lista popularnych haseł (NIST SP 800-63B: sprawdzanie z listą haseł zakazanych)
        private static readonly HashSet<string> BlockedPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            "123456789012", "1234567890123", "qwertyuiopas", "qwerty123456", "password1234",
            "password12345", "haslo1234567", "haslohaslo12", "zaq12wsxcde3", "1qaz2wsx3edc",
            "qwertyqwerty", "aaaaaaaaaaaa", "111111111111", "000000000000", "iloveyou1234",
            "administrator", "administrator1", "carrental123", "wypozyczalnia", "wypozyczalnia1"
        };

        // Argon2id z losową 16-bajtową solą; domyślne parametry biblioteki: 64 MiB pamięci, 3 iteracje
        public static string Hash(string password) => Argon2.Hash(password);

        public static bool Verify(string hash, string password) => Argon2.Verify(hash, password);

        public static void VerifyDummy(string password) => Argon2.Verify(DummyHash.Value, password);

        public static List<string> Validate(string password, AppUser user, SecuritySettings settings)
        {
            var errors = new List<string>();

            if (password.Length < settings.MinPasswordLength)
                errors.Add($"Hasło musi mieć co najmniej {settings.MinPasswordLength} znaków.");

            if (password.Length > MaxPasswordLength)
                errors.Add($"Hasło może mieć maksymalnie {MaxPasswordLength} znaków.");

            if (BlockedPasswords.Contains(password))
                errors.Add("To hasło jest na liście popularnych (zakazanych) haseł.");

            if (settings.PersonalDataRuleEnabled)
            {
                // Pomijamy fragmenty krótsze niż 3 znaki, żeby np. inicjał nie blokował połowy haseł
                var personalData = new[] { user.Login, user.FirstName, user.LastName }
                    .Where(p => !string.IsNullOrWhiteSpace(p) && p.Trim().Length >= 3);

                if (personalData.Any(p => password.Contains(p.Trim(), StringComparison.OrdinalIgnoreCase)))
                    errors.Add("Hasło nie może zawierać loginu, imienia ani nazwiska.");
            }

            return errors;
        }

        // 16 znaków: litery, cyfry i znaki specjalne (co najmniej po jednym), losowane kryptograficznie bezpiecznym RNG
        public static string GenerateOneTimePassword()
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*-_=+?";
            const string all = letters + digits + special;

            var chars = new List<char>
            {
                RandomNumberGenerator.GetItems<char>(letters, 1)[0],
                RandomNumberGenerator.GetItems<char>(digits, 1)[0],
                RandomNumberGenerator.GetItems<char>(special, 1)[0]
            };
            chars.AddRange(RandomNumberGenerator.GetItems<char>(all, OneTimePasswordLength - chars.Count));

            var result = chars.ToArray();
            RandomNumberGenerator.Shuffle(result.AsSpan());
            return new string(result);
        }
    }
}
