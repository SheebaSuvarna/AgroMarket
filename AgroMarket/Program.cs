using AgroMarket.Controllers;
using AgroMarket.Data;
using AgroMarket.Models.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("defaultconnection")));
            builder.Services.AddScoped<EmailService>();

            builder.Services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();
            builder.Services.AddScoped<IPasswordHasher<Retailer>, PasswordHasher<Retailer>>();

            // Add Authentication services
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login"; // Path to the login action
                    options.LogoutPath = "/Account/Logout"; // Path to the logout action
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie expiration time
                    options.SlidingExpiration = true; // Sliding expiration for active users
                });
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
