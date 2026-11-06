using CarRental.Data;
using CarRental.Security;
using CarRental.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton(TimeProvider.System);

            // Uwierzytelnianie: ciasteczko sesyjne, sesja weryfikowana w bazie przy każdym żądaniu
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.Name = "CarRental.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Strict;

                    options.Events.OnValidatePrincipal = async context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var (valid, idleMinutes) = await userService.ValidateSessionAsync(context.Principal!);
                        context.HttpContext.Items["SessionIdleMinutes"] = idleMinutes;

                        if (!valid)
                        {
                            context.HttpContext.Items["SessionExpired"] = true;
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        }
                    };

                    // API zwraca kody 401/403 zamiast przekierowań na stronę logowania
                    options.Events.OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }

                        var redirect = context.RedirectUri;
                        if (context.HttpContext.Items.ContainsKey("SessionExpired"))
                            redirect += "&expired=1";
                        context.Response.Redirect(redirect);
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    };
                });

            // Autoryzacja: każda strona wymaga zalogowania, a każde uprawnienie RBAC to osobna polityka
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                foreach (var permission in Permissions.All)
                    options.AddPolicy(permission, policy => policy.RequireClaim(Permissions.ClaimType, permission));
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                DbSeeder.Seed(scope.ServiceProvider.GetRequiredService<DataContext>());
            }

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

            app.UseAuthentication();

            // Użytkownik z hasłem jednorazowym lub wygasłym może tylko zmienić hasło albo się wylogować
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;
                if (context.User.HasClaim(UserService.MustChangePasswordClaim, "true")
                    && !path.StartsWithSegments("/Account/ChangePassword", StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWithSegments("/Account/Logout", StringComparison.OrdinalIgnoreCase)
                    && !Path.HasExtension(path))
                {
                    context.Response.Redirect("/Account/ChangePassword");
                    return;
                }

                await next();
            });

            app.UseAuthorization();

            app.MapControllers();

            app.MapStaticAssets().AllowAnonymous();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
