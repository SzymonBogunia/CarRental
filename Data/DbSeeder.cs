using CarRental.Models;
using CarRental.Security;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Data
{
    // Tworzy bazę (migracje), domyślne ustawienia bezpieczeństwa i konta testowe (wariant 2: 6 użytkowników)
    public static class DbSeeder
    {
        // Hasło startowe konta ADMIN - przy pierwszym logowaniu system wymusi jego zmianę
        private const string AdminInitialPassword = "Start-Haslo-2026";

        // Konta testowe do sprawdzania ról - hasła startowe nadane "przez administratora",
        // więc tak jak przy ADMIN przy pierwszym logowaniu system wymusi ich zmianę
        private static readonly (string Login, string FirstName, string LastName, UserRole Role, string Password)[] TestUsers =
        {
            ("jkowalski", "Jan", "Kowalski", UserRole.Kierownik, "Kierownik-Test-2026"),
            ("anowak", "Anna", "Nowak", UserRole.Pracownik, "Pracownik1-Test-2026"),
            ("pwisniewski", "Piotr", "Wiśniewski", UserRole.Pracownik, "Pracownik2-Test-2026"),
            ("mzielinski", "Marek", "Zieliński", UserRole.Gosc, "Gosc1-Test-2026"),
            ("ewojcik", "Ewa", "Wójcik", UserRole.Gosc, "Gosc2-Test-2026")
        };

        public static void Seed(DataContext context)
        {
            context.Database.Migrate();

            if (!context.SecuritySettings.Any())
                context.SecuritySettings.Add(new SecuritySettings());

            if (!context.AppUsers.Any())
            {
                context.AppUsers.Add(new AppUser
                {
                    Login = "ADMIN",
                    FirstName = "Administrator",
                    LastName = "Systemu",
                    Role = UserRole.Administrator,
                    PasswordHash = PasswordService.Hash(AdminInitialPassword),
                    MustChangePassword = true
                });

                foreach (var u in TestUsers)
                {
                    context.AppUsers.Add(new AppUser
                    {
                        Login = u.Login,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Role = u.Role,
                        PasswordHash = PasswordService.Hash(u.Password),
                        MustChangePassword = true
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
