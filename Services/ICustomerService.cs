using CarRental.Models;

namespace CarRental.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<(bool Success, string ErrorMessage)> CreateCustomerAsync(Customer newCustomer);
        Task<(bool Success, string ErrorMessage)> UpdateCustomerAsync(Customer customerToEdit);
        Task<bool> DeleteCustomerAsync(int id);
    }
}