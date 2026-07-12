using CarRental.Data;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;

namespace CarRental.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly DataContext _context;

        public CustomerService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<(bool Success, string ErrorMessage)> CreateCustomerAsync(Customer newCustomer)
        {
            // wymagany pesel lub paszport
            if (string.IsNullOrWhiteSpace(newCustomer.Pesel) && string.IsNullOrWhiteSpace(newCustomer.PassportNumber))
            {
                return (false, "Wymagane jest podanie numeru PESEL (dla obywateli PL) lub numeru Paszportu (dla obcokrajowców)!");
            }

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateCustomerAsync(Customer customerToEdit)
        {
            // modyfikacja walidacja paszport/pesel
            if (string.IsNullOrWhiteSpace(customerToEdit.Pesel) && string.IsNullOrWhiteSpace(customerToEdit.PassportNumber))
            {
                return (false, "Wymagane jest podanie numeru PESEL lub numeru Paszportu!");
            }

            _context.Attach(customerToEdit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Customers.AnyAsync(e => e.Id == customerToEdit.Id))
                {
                    return (false, "Modyfikowany klient nie istnieje w bazie danych.");
                }
                throw;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return (false, "Klient o podanym ID nie istnieje.");
            }

            var hasActiveRentals = await _context.Rentals
                .AnyAsync(r => r.CustomerId == id && r.Status == RentalStatus.Active);

            if (hasActiveRentals)
            {
                return (false, "Nie można usunąć klienta, który aktualnie posiada wypożyczone auto.");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }
    }
}