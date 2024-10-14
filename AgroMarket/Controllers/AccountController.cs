using System;
using AgroMarket.Data;
using AgroMarket.Models.Entities;
using AgroMarket.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroMarket.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using AgroMarket.Models;
namespace AgroMarket.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IPasswordHasher<Customer> _customerPasswordHasher;
        private readonly IPasswordHasher<Retailer> _retailerPasswordHasher;

        public AccountController(ApplicationDbContext context, EmailService emailService, IPasswordHasher<Customer> customerPasswordHasher, IPasswordHasher<Retailer> retailerPasswordHasher)
        {
            _context = context; // Initialize the context
            _emailService = emailService;
            _customerPasswordHasher = customerPasswordHasher;
            _retailerPasswordHasher = retailerPasswordHasher;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = PasswordHelper.HashPassword(model.Password);

                // Check if the email exists in the Customers table
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == model.Email && c.Password == hashedPassword);

                // Check if the email exists in the Retailers table
                var retailer = await _context.Retailers
                    .FirstOrDefaultAsync(r => r.Email == model.Email && r.Password == hashedPassword);


                if (customer != null && retailer != null)
                {
                    // Create claims for the authenticated retailer
                    var claims = new List<Claim>
                    {
                        new Claim("RetailerID", retailer.RetailerID.ToString()),
                        new Claim("CustomerId", customer.CustomerID.ToString()),
                        new Claim("Role","customer"),
                        new Claim("name", customer.FirstName),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : (DateTimeOffset?)null
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Redirect to a role selection page
                    return RedirectToAction("SelectRole");
                }

                if (customer != null)
                {
                    // Create claims for the authenticated user
                    var claims = new List<Claim>
                    {
                        new Claim("CustomerId", customer.CustomerID.ToString()),
                        new Claim("Role","customer"),
                        new Claim("name", customer.FirstName),


                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe, // Set persistent cookie if "Remember Me" is checked
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : (DateTimeOffset?)null // Optional: cookie expiration
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    return RedirectToAction("Index","Customer");
                }


                if (retailer != null)
                {
                    // Create claims for the authenticated retailer
                    var claims = new List<Claim>
                    {
                        new Claim("RetailerID", retailer.RetailerID.ToString()),new Claim("Role","retailer"),new Claim("name",retailer.FirstName),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : (DateTimeOffset?)null
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    return RedirectToAction("Dashboard","Retailer");
                }

                // If no match is found, add an error
                ModelState.AddModelError("", "Invalid login attempt. Please check your email and password.");
            }

            return View(model); // Return the view with validation errors
        }
       
        [HttpPost]
        public async Task<IActionResult> RoleSelected( string role)
        {
            // Check if the selected role is Customer
            if (role == "Customer")
            {
                // Authenticate as customer
                return RedirectToAction("Index","Customer");
            }
            else if (role == "Retailer")
            {
                // Authenticate as retailer

                return RedirectToAction("Dashboard","Retailer");

            }

            // Handle case where authentication fails
            ModelState.AddModelError("", "Invalid login attempt. Please check your email and password.");
            return View("SelectRole");
        }

        public IActionResult SelectRole()
        {
            
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        /* public IActionResult RegisterCustomer()
         {
             return View();
         }
         [HttpPost]
         public async Task<IActionResult> RegisterCustomer(RegisterCustomerModel model)
         {
             if (ModelState.IsValid)
             {
                 // Check if the email already exists in the database
                 var existingCustomer = await _context.Customers
                     .FirstOrDefaultAsync(c => c.Email == model.Email);

                 if (existingCustomer != null)
                 {
                     ModelState.AddModelError("Email", "Email is already registered. Please use a different email.");
                     return View(model); // Return the view with validation errors
                 }

                 // Create a new Customer object
                 var customer = new Customer
                 {
                     FirstName = model.FirstName,
                     LastName = model.LastName,
                     Email = model.Email,

                     Password = PasswordHelper.HashPassword(model.Password),
                     PhoneNumber = model.PhoneNumber,
                     Address = model.Address,
                     PinCode = model.PinCode,
                     CreatedAt = DateTime.Now,
                     UpdatedAt = DateTime.Now
                 };
                 // customer.Password = _customerPasswordHasher.HashPassword(customer, model.Password);
                 // customer.Password = PasswordHelper.HashPassword(model.Password);

                 // Add the customer to the context
                 _context.Customers.Add(customer);
                 await _context.SaveChangesAsync(); // Save changes to the database

                 // Send confirmation email
                 string subject = "Registration Successful";
                 string message = $"Dear {model.FirstName},<br/>Thank you for registering with AgroMarket.";
                 await _emailService.SendEmailAsync(model.Email, subject, message);

                 // Redirect to login or another page
                 ViewBag.SuccessMessage = "Registration successfull! Please check your email for verification.";

                 // Redirect to the Register page or any other page
                 return View("RegisterCustomer", model);
             }
             ViewBag.FailureMessage = "Please Fill neccesary fields";
             return View(model); // Return the view with validation errors
         }*/
        // GET: Register
        public IActionResult RegisterCustomer()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCustomer(RegisterCustomerModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists in the database
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == model.Email);

                if (existingCustomer != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered. Please use a different email.");
                    return View(model); // Return the view with validation errors
                }
                var customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = PasswordHelper.HashPassword(model.Password),
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    PinCode = model.PinCode
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                TempData["ProductSuccess"] = " Registration Sucessfully!";
                return RedirectToAction("Login", "Account"); // Redirect to home or a confirmation page
            }
            ViewData["PhoneNumberError"] = "Phone number must be exactly 10 digits.";
            ViewData["PasswordError"] = "Enter Valid Password";
            ViewData["ConfirmPasswordError"] = "Passwords do not match.";
            return View(model); // If validation fails, return the same view with the model to display errors
        }

        public IActionResult RegisterRetailor()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterRetailor(RegisterRetailorViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists in the database
                var existingRetailor = await _context.Retailers
                    .FirstOrDefaultAsync(r => r.Email == model.Email);

                if (existingRetailor != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered. Please use a different email.");
                    return View(model); // Return the view with validation errors
                }

                // Create a new Retailer object
                var retailer = new Retailer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    //Password = model.Password, // Make sure to hash the password
                    Password = PasswordHelper.HashPassword(model.Password),
                    PhoneNumber = model.PhoneNumber,
                    FarmName = model.FarmName,
                    FarmAddress = model.FarmAddress,
                    PinCode = model.PinCode,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                //retailer.Password = PasswordHelper.HashPassword(model.Password);

                //_retailerPasswordHasher.HashPassword(retailer, model.Password);
                // Add the retailer to the context
                _context.Retailers.Add(retailer);
                await _context.SaveChangesAsync(); // Save changes to the database

                // Send confirmation email
                string subject = "Registration Successful";
                string message = $"Dear {model.FirstName},<br/>Thank you for registering with AgroMarket.";
                //await _emailService.SendEmailAsync(model.Email, subject, message);
                TempData["ProductSuccess"] = " Registration Sucessfully!";

                // Redirect to login or another page
                return RedirectToAction("Login","Account");
            }
            ViewData["PhoneNumberError"] = "Phone number must be exactly 10 digits.";
            ViewData["PasswordError"] = "Enter Valid Password";
            ViewData["ConfirmPasswordError"] = "Passwords do not match.";

            return View(model); // Return the view with validation errors
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }
      

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmail model)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email);
                var existingRetailer = await _context.Retailers.FirstOrDefaultAsync(r => r.Email == model.Email);

                if (existingCustomer != null || existingRetailer != null)
                {
                    // Generate OTP
                    var otp = new Random().Next(100000, 999999);

                    // Store OTP in session or database as needed
                    HttpContext.Session.SetInt32("OTP", otp);
                    HttpContext.Session.SetString("OTPEmail", model.Email);

                    // Send OTP via email
                    await _emailService.SendEmailAsync(model.Email, "Your OTP Code", $"Your OTP code is: {otp}");


                    /*  TempData["SuccessMessage"] = "An OTP has been sent to your email.";
                      ViewBag.SuccessMessage = TempData["SuccessMessage"];
  */

                    return RedirectToAction("EnterOTP", new { email = model.Email });
                }

                ModelState.AddModelError("", "Email not found. Please enter a registered email.");
            }

            return View(model);
        }





        public IActionResult EnterOTP(string email)
        {
            var model = new EnterOTP { Email = email };
            return View(model);
        }

        [HttpPost]
        public IActionResult EnterOTP(EnterOTP model)
        {
            if (ModelState.IsValid)
            {
                var savedOTP = HttpContext.Session.GetInt32("OTP");
                var savedEmail = HttpContext.Session.GetString("OTPEmail");

                if (savedOTP == model.OTP && savedEmail == model.Email)
                {
                    // Clear OTP from session after successful validation
                    HttpContext.Session.Remove("OTP");
                    HttpContext.Session.Remove("OTPEmail");

                    // Redirect to ChangePassword action
                    return RedirectToAction("ChangePassword", new { email = model.Email });
                }

                ModelState.AddModelError("", "Incorrect OTP. Please try again or request a new OTP.");
            }
            return View(model);
        }


        [HttpGet] // Allows this method to respond to GET requests
        public async Task<IActionResult> ResendOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email is required.";
                return RedirectToAction("EnterOTP"); // Adjust redirection as needed
            }

            var otp = new Random().Next(100000, 999999);
            HttpContext.Session.SetInt32("OTP", otp);
            HttpContext.Session.SetString("OTPEmail", email);

            await _emailService.SendEmailAsync(email, "Your OTP Code", $"Your new OTP code is: {otp}");
            TempData["SuccessMessage"] = "A new OTP has been sent to your email.";

            return RedirectToAction("EnterOTP", new { email });
        }

        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (ModelState.IsValid)
            {
                // First, check if the email exists in the Customers table
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == model.Email);

                // If not found in Customers, check the Retailers table
                var retailer = customer == null
                    ? await _context.Retailers.FirstOrDefaultAsync(r => r.Email == model.Email)
                    : null;

                if (customer != null)
                {
                    // Update password for customer (remember to hash it in production)
                    customer.Password = PasswordHelper.HashPassword(model.NewPassword);


                    customer.UpdatedAt = DateTime.Now;

                    _context.Customers.Update(customer);
                }
                else if (retailer != null)
                {
                    // Update password for retailer (remember to hash it in production)
                    retailer.Password = PasswordHelper.HashPassword(model.NewPassword);
                    retailer.UpdatedAt = DateTime.Now;

                    _context.Retailers.Update(retailer);
                }
                else
                {
                    // Email not found in either table
                    ModelState.AddModelError("Email", "Email not found. Please enter a valid registered email.");
                    return View(model);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Redirect to a success page or login page after the password change
                return RedirectToAction("Login");
            }

            return View(model); // Return the view with validation errors
        }
        public IActionResult CustomerDashBoard()
        {
            return View();
        }
        public IActionResult RetailorDashBoard()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index","Customer");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }



    }
}
