using CarRental.Data;
using CarRental.Services;
using CarRental.Services;
using Microsoft.EntityFrameworkCore;


namespace CarRental
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();

            builder.Services.AddControllers();
            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IRentalService, RentalService>();
            builder.Services.AddScoped<ICarService, CarService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();

            builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton(TimeProvider.System);
            

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(); // swagger
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
