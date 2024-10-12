using AgroMarket.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using AgroMarket.Controllers;
using AgroMarket.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace AgroMarket
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("defaultconnection")));

            builder.Services.AddScoped<EmailService>();

            builder.Services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();
            builder.Services.AddScoped<IPasswordHasher<Retailer>, PasswordHasher<Retailer>>();

            // Configure authentication with default schemes
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login"; // Path to login page
                options.AccessDeniedPath = "/Account/AccessDenied"; // Path to access denied page
            });
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout as needed
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Required for GDPR compliance
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession(); //session middleware

            // Enable authentication middleware
            app.UseAuthentication();

            // Enable authorization middleware
            app.UseAuthorization();

            // Map routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Customer}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
