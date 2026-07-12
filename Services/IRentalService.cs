using Microsoft.AspNetCore.Mvc.Rendering;
using CarRental.Models;
using static CarRental.Pages.RentalSettleModel;

namespace CarRental.Services
{
    public interface IRentalService
    {
        // Dropdowny
        Task<List<SelectListItem>> GetAvailableCarOptionsAsync();
        Task<List<SelectListItem>> GetAllCarOptionsAsync();
        Task<List<SelectListItem>> GetCustomerOptionsAsync();
        Task<List<SelectListItem>> GetSimpleCustomerOptionsAsync();

        // Operacje główne
        Task<(bool Success, string ErrorMessage)> CreateRentalAsync(Rental newRental);
        Task<Rental?> GetRentalForSettlementAsync(int id);
        Task<(bool Success, string Message)> SettleRentalAsync(RentalSettleViewModel settleData);
        Task<List<Rental>> GetAllRentalsWithDetailsAsync();
        Task UpdatePlannedAndActiveRentalsStatusAsync();
        Task<(bool Success, string ErrorMessage)> EditRentalAsync(Rental rentalToEdit);
        Task<bool> DeleteRentalAsync(int rentalId);
        Task<(bool Success, string ErrorMessage, Rental? Rental)> CreateRentalAsync(int carId, int customerId, DateTime startDate, DateTime endDate);
    }
}