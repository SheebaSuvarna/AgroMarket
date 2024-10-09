using AgroMarket.Data;
using AgroMarket.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket.Controllers
{
    
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> ProductDetails(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Retailer)
                .Include(p => p.Review)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }


            var products = await _context.Products
        .Include(p => p.Review) // Include reviews for each product
        .ToListAsync();

            // Dictionary to store product ID and corresponding average rating
            var productRatings = new Dictionary<Guid, double>();

            foreach (var prod in products)
            {
                double averageRating = 0;
                if (prod.Review != null && prod.Review.Any())
                {
                    averageRating = prod.Review.Average(r => r.Rating);
                }

                // Store average rating in the dictionary
                productRatings[prod.ProductID] = averageRating;
            }

            // Pass the ratings to the view using ViewBag
            ViewBag.AverageRating = productRatings;

            return View(product);
        }


    }

}
