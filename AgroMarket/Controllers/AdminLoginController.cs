using Microsoft.AspNetCore.Mvc;
using AgroMarket.Models;
using AgroMarket.Data; 
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AgroMarket.Models.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace AgroMarket.Controllers
{
    public class AdminLoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminLoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the admin user from the database
                var adminUser = await _context.AdminUsers
                    .SingleOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

                if (adminUser != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("Role","admin"),                     
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
                    // Authentication successful, redirect to the admin dashboard
                    return RedirectToAction("Dashboard", "Admin");
                }

                // If authentication fails, add an error message
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
