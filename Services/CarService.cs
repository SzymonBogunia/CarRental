using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services
{
    public class CarService : ICarService
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;

        public CarService(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<Car?> GetCarByIdAsync(int id)
        {
            return await _context.Cars.FindAsync(id);
        }

        public async Task<List<Car>> GetAllCarsAsync()
        {
            return await _context.Cars.ToListAsync();
        }

        public async Task<(bool Success, string ErrorMessage)> CreateCarAsync(Car newCar, IFormFile? imageFile)
        {
            var existingCar = await _context.Cars
            .AnyAsync(c => c.RegistrationNumber == newCar.RegistrationNumber || c.VIN == newCar.VIN);

            if (existingCar)
            {
                return (false, "Samochód o takim numerze rejestracyjnym lub VIN już istnieje w bazie.");
            }

            newCar.Status = CarStatus.Available; 

            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    string? imageUrl = await SaveImageAsync(imageFile);
                    if (imageUrl != null)
                    {
                        newCar.ImageUrl = imageUrl;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BŁĄD] Wyjątek podczas zapisu pliku: {ex.Message}");
                    return (false, "Wystąpił problem z zapisem pliku na serwerze.");
                }
            }
            else if (string.IsNullOrWhiteSpace(newCar.ImageUrl))
            {
                newCar.ImageUrl = "/images/cars/default-car.jpg";
            }

            _context.Cars.Add(newCar);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> ChangeCarStatusAsync(int id, CarStatus newStatus)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return (false, $"Nie znaleziono samochodu o ID {id}.");

            car.Status = newStatus;
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> EditCarAsync(Car carToEdit, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    // Usuwamy stare zdjęcie przed wgraniem nowego (jeśli to nie jest default)
                    if (!string.IsNullOrEmpty(carToEdit.ImageUrl) && !carToEdit.ImageUrl.Contains("default-car.jpg"))
                    {
                        string oldFilePath = Path.Combine(_environment.WebRootPath, carToEdit.ImageUrl.TrimStart('/'));
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }

                    // Zapisujemy nowe zdjęcie
                    string? imageUrl = await SaveImageAsync(imageFile);
                    if (imageUrl != null)
                    {
                        carToEdit.ImageUrl = imageUrl;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BŁĄD] Wyjątek podczas edycji zdjęcia: {ex.Message}");
                    return (false, "Wystąpił problem z aktualizacją pliku graficznego.");
                }
            }

            _context.Attach(carToEdit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Cars.AnyAsync(e => e.Id == carToEdit.Id))
                {
                    return (false, "Nie znaleziono modyfikowanego pojazdu w bazie.");
                }
                throw;
            }
        }

        // Prywatna metoda pomocnicza do obsługi fizycznego zapisu I/O
        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cars");

            Directory.CreateDirectory(uploadsFolder);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/cars/" + uniqueFileName;
        }
    }
}