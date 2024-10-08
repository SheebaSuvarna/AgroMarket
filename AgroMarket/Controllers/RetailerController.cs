﻿using AgroMarket.Data;
using AgroMarket.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket.Controllers
{
    [Authorize]
    public class RetailerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var retailerId = User.FindFirst("RetailerID")?.Value;
            if (retailerId == null)
            {
                return RedirectToAction("Login", "RetailerAuth");
            }

            var retailer = await _context.Retailers
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.RetailerID.ToString() == retailerId);

            if (retailer == null)
            {
                return NotFound();
            }

            return View(retailer);
        }

        public async Task<IActionResult> Profile()
        {
            var retailerId = User.FindFirst("RetailerID")?.Value;
            if (retailerId == null)
            {
                return RedirectToAction("Login", "RetailerAuth");
            }

            var retailer = await _context.Retailers
                .FirstOrDefaultAsync(r => r.RetailerID.ToString() == retailerId);

            if (retailer == null)
            {
                return NotFound();
            }

            // Check if the profile image exists
            string profileImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile_pictures", $"{retailer.RetailerID}.jpg");
            bool imageExists = System.IO.File.Exists(profileImagePath);

            ViewBag.ImageExists = imageExists;

            return View(retailer);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            var retailerId = User.FindFirst("RetailerID")?.Value;
            if (retailerId == null || profilePicture == null)
            {
                return RedirectToAction("Profile");
            }

            // Define the path to save the profile picture
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile_pictures", $"{retailerId}.jpg");

            // Save the image
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            return RedirectToAction("Profile");
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateRetailer([FromBody] Retailer updatedRetailer)
        {
            var retailerId = User.FindFirst("RetailerID")?.Value;
            if (retailerId == null)
            {
                return Unauthorized();
            }

            var retailer = await _context.Retailers
                .FirstOrDefaultAsync(r => r.RetailerID.ToString() == retailerId);

            if (retailer == null)
            {
                return NotFound();
            }

            // Update the necessary fields
            retailer.FirstName = updatedRetailer.FirstName ?? retailer.FirstName;
            retailer.LastName = updatedRetailer.LastName ?? retailer.LastName;
            retailer.Email = updatedRetailer.Email ?? retailer.Email;
            retailer.PhoneNumber = updatedRetailer.PhoneNumber ?? retailer.PhoneNumber;
            retailer.FarmName = updatedRetailer?.FarmName ?? retailer.FarmName;
            retailer.FarmAddress = updatedRetailer?.FarmAddress ?? retailer.FarmAddress;
            retailer.PinCode = updatedRetailer?.PinCode ?? retailer.PinCode;

            // Add other fields to update as needed

            _context.Retailers.Update(retailer);
            await _context.SaveChangesAsync();

            return Ok(retailer);
        }








    }
}
