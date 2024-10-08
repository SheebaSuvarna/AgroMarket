using AgroMarket.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket.Controllers
{
    public class RetailerAuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailerAuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var retailer = await _context.Retailers
                .FirstOrDefaultAsync(r => r.Email == email && r.Password == password);

            if (retailer == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }

            var claims = new List<Claim>
            {
                
                new Claim(ClaimTypes.Name, retailer.FirstName),
                new Claim(ClaimTypes.Surname, retailer.LastName),
                new Claim(ClaimTypes.Email, retailer.Email),
                new Claim(ClaimTypes.MobilePhone, retailer.PhoneNumber),
                new Claim(ClaimTypes.StreetAddress, retailer.FarmAddress),
                new Claim(ClaimTypes.PostalCode, retailer.PinCode),
                new Claim("RetailerID", retailer.RetailerID.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Dashboard", "Retailer");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
