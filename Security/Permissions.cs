using CarRental.Models;

namespace CarRental.Security
{
    // RBAC: uprawnienia są przypisane do ról, a rola do użytkownika.
    // Każde uprawnienie jest jednocześnie nazwą polityki autoryzacji ([Authorize(Policy = ...)]).
    public static class Permissions
    {
        public const string ClaimType = "permission";

        public const string CarsView = "Cars.View";                 // podstawowe dane floty (zdjęcie, marka, model, rok)
        public const string CarsViewDetails = "Cars.ViewDetails";   // rejestracja, VIN, cena, status
        public const string CarsManage = "Cars.Manage";             // dodawanie i edycja aut
        public const string CustomersView = "Customers.View";
        public const string CustomersManage = "Customers.Manage";   // dodawanie i edycja klientów
        public const string RentalsView = "Rentals.View";
        public const string RentalsManage = "Rentals.Manage";       // tworzenie i edycja rezerwacji
        public const string RentalsSettle = "Rentals.Settle";
        public const string RecordsDelete = "Records.Delete";       // usuwanie aut, klientów, rezerwacji
        public const string UsersManage = "Users.Manage";           // panel administratora

        public static readonly string[] All =
        {
            CarsView, CarsViewDetails, CarsManage, CustomersView, CustomersManage,
            RentalsView, RentalsManage, RentalsSettle, RecordsDelete, UsersManage
        };

        public static readonly IReadOnlyDictionary<UserRole, string[]> ByRole = new Dictionary<UserRole, string[]>
        {
            [UserRole.Administrator] = All,
            [UserRole.Kierownik] = new[]
            {
                CarsView, CarsViewDetails, CarsManage, CustomersView, CustomersManage,
                RentalsView, RentalsManage, RentalsSettle, RecordsDelete
            },
            [UserRole.Pracownik] = new[]
            {
                CarsView, CarsViewDetails, CustomersView, CustomersManage,
                RentalsView, RentalsManage, RentalsSettle
            },
            [UserRole.Gosc] = new[] { CarsView }
        };
    }

    public static class PermissionExtensions
    {
        public static bool HasPermission(this System.Security.Claims.ClaimsPrincipal user, string permission)
            => user.HasClaim(Permissions.ClaimType, permission);
    }
}
