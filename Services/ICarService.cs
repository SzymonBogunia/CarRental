using CarRental.Models;
using Microsoft.AspNetCore.Http;

namespace CarRental.Services
{
    public interface ICarService
    {
        Task<List<Car>> GetAllCarsAsync();
        Task<(bool Success, string ErrorMessage)> CreateCarAsync(Car newCar, IFormFile? imageFile);
        Task<(bool Success, string ErrorMessage)> EditCarAsync(Car carToEdit, IFormFile? imageFile);
    }
}