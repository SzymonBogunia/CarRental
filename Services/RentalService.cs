using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using static CarRental.Pages.RentalSettleModel;

namespace CarRental.Services
{
    public class RentalService : IRentalService
    {
        private readonly DataContext _context;

        public RentalService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<SelectListItem>> GetAvailableCarOptionsAsync()
        {
            return await _context.Cars
            .Where(c => c.Status != CarStatus.InService)
            .Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Brand} {c.Model} ({c.RegistrationNumber}) - {c.PricePerDay:F2} zł/dobę"
        }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllCarOptionsAsync()
        {
            return await _context.Cars
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Brand} {c.Model} ({c.RegistrationNumber})"
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetCustomerOptionsAsync()
        {
            return await _context.Customers
                .Select(cust => new SelectListItem
                {
                    Value = cust.Id.ToString(),
                    Text = $"{cust.FirstName} {cust.LastName} - {(string.IsNullOrEmpty(cust.Pesel) ? "Paszport: " + cust.PassportNumber : "PESEL: " + cust.Pesel)}"
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetSimpleCustomerOptionsAsync()
        {
            return await GetCustomerOptionsAsync();
        }


        public async Task<(bool Success, string ErrorMessage)> CreateRentalAsync(Rental newRental)
        {
            newRental.StartDate = RoundToHour(newRental.StartDate);
            newRental.EndDate = RoundToHour(newRental.EndDate);

            if (newRental.EndDate <= newRental.StartDate)
                return (false, "Data zakończenia musi być późniejsza niż data rozpoczęcia!");

            bool isCarOccupied = await _context.Rentals.AnyAsync(r =>
                r.CarId == newRental.CarId &&
                newRental.StartDate < r.EndDate &&
                newRental.EndDate > r.StartDate
            );

            if (isCarOccupied)
                return (false, "Ten samochód jest już zarezerwowany w wybranym przedziale czasowym! Wybierz inny termin lub inne auto.");

            var car = await _context.Cars.FindAsync(newRental.CarId);
            if (car == null) return (false, "Wybrany samochód nie istnieje.");

            newRental.TotalPrice = CalculatePrice(newRental.StartDate, newRental.EndDate, car.PricePerDay);

            var now = DateTime.Now;
            if (newRental.StartDate <= now && newRental.EndDate > now)
            {
                newRental.Status = RentalStatus.Active;
                car.Status = CarStatus.Rented;
            }
            else if (newRental.StartDate > now)
            {
                newRental.Status = RentalStatus.Planned;
                if (car.Status != CarStatus.Rented)
                {
                    car.Status = CarStatus.Available;
                }
            }
            else if (newRental.EndDate <= now)
            {
                newRental.Status = RentalStatus.ToBeSettled;
                car.Status = CarStatus.Rented;
            }

            _context.Rentals.Add(newRental);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<Rental?> GetRentalForSettlementAsync(int id)
        {
            return await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message)> SettleRentalAsync(RentalSettleViewModel settleData)
        {
            var rentalToUpdate = await _context.Rentals
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == settleData.RentalId);

            if (rentalToUpdate == null) return (false, "Nie znaleziono rezerwacji.");

            decimal extraFees = settleData.CleaningFee + settleData.FuelDeficitFee + settleData.DamageFee;
            rentalToUpdate.TotalPrice += extraFees;
            rentalToUpdate.Status = RentalStatus.Completed;
            rentalToUpdate.Comments = settleData.Comments;
            rentalToUpdate.CleaningFee = settleData.CleaningFee;
            rentalToUpdate.FuelDeficitFee = settleData.FuelDeficitFee;
            rentalToUpdate.DamageFee = settleData.DamageFee;

            if (rentalToUpdate.Car != null)
            {
                rentalToUpdate.Car.Status = CarStatus.Available;
            }

            await _context.SaveChangesAsync();
            return (true, $"Pomyślnie rozliczono umowę #{rentalToUpdate.Id} i zwolniono pojazd {rentalToUpdate.Car?.Brand}.");
        }

        public async Task<List<Rental>> GetAllRentalsWithDetailsAsync()
        {
            return await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task UpdatePlannedAndActiveRentalsStatusAsync()
        {
            var now = DateTime.Now;
            var rentalsToUpdate = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Planned || r.Status == RentalStatus.Active)
                .ToListAsync();

            bool anyChanges = false;
            foreach (var rental in rentalsToUpdate)
            {
                if (rental.Status == RentalStatus.Planned && rental.StartDate <= now)
                {
                    rental.Status = RentalStatus.Active;
                    anyChanges = true;
                }
                if (rental.Status == RentalStatus.Active && rental.EndDate <= now)
                {
                    rental.Status = RentalStatus.ToBeSettled;
                    anyChanges = true;
                }
            }

            if (anyChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(bool Success, string ErrorMessage)> EditRentalAsync(Rental rentalToEdit)
        {
            rentalToEdit.StartDate = RoundToHour(rentalToEdit.StartDate);
            rentalToEdit.EndDate = RoundToHour(rentalToEdit.EndDate);

            if (rentalToEdit.EndDate <= rentalToEdit.StartDate)
                return (false, "Data zakończenia musi być późniejsza niż data rozpoczęcia!");

            bool isCarOccupied = await _context.Rentals.AnyAsync(r =>
                r.Id != rentalToEdit.Id &&
                r.CarId == rentalToEdit.CarId &&
                rentalToEdit.StartDate < r.EndDate &&
                rentalToEdit.EndDate > r.StartDate
            );

            if (isCarOccupied)
                return (false, "Ten samochód jest już zajęty w podanym terminie!");

            var car = await _context.Cars.FindAsync(rentalToEdit.CarId);
            if (car != null)
            {
                rentalToEdit.TotalPrice = CalculatePrice(rentalToEdit.StartDate, rentalToEdit.EndDate, car.PricePerDay);
            }

            _context.Attach(rentalToEdit).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<bool> DeleteRentalAsync(int rentalId)
        {
            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental == null) return false;

            var car = await _context.Cars.FindAsync(rental.CarId);
            if (car != null)
            {
                car.Status = CarStatus.Available;
            }

            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string ErrorMessage, Rental? Rental)> CreateRentalAsync(int carId, int customerId, DateTime startDate, DateTime endDate)
        {
            // na podstawie dto
            var rental = new Rental
            {
                CarId = carId,
                CustomerId = customerId,
                StartDate = startDate,
                EndDate = endDate
            };

            
            var result = await CreateRentalAsync(rental);

            if (!result.Success)
            {
                return (false, result.ErrorMessage, null);
            }

            return (true, string.Empty, rental);
        }

        // Pomocnicze metody prywatne wewnątrz serwisu
        private DateTime RoundToHour(DateTime dt) => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);

        private decimal CalculatePrice(DateTime start, DateTime end, decimal pricePerDay)
        {
            TimeSpan duration = end - start;
            double days = Math.Ceiling(duration.TotalDays);
            if (days <= 0) days = 1;
            return (decimal)days * pricePerDay;
        }
    }
}