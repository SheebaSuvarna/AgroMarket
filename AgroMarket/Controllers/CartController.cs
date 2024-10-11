using AgroMarket.Data;
using AgroMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgroMarket.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);
            if (customerId == null)
            {
                return Unauthorized("User is not authenticated.");
            }


            // Get the cart items for the customer
            var cartItems = await _context.Carts
                .Where(c => c.CustomerID == customerId)
                .Include(c => c.Product) // Include product details
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                CartItems = cartItems.Select(c => new CartItemViewModel
                {
                    ProductID = c.ProductID,
                    ProductName = c.Product.ProductName,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    // ImageUrl = c.Product.ImageUrl // Optional
                    StockQuantity = c.Product.StockQuantity


                }).ToList(),
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price),
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(Guid productId, int quantity)
        {
            var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerID == customerId && c.ProductID == productId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid productId)
        {
            var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);


            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerID == customerId && c.ProductID == productId);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }

}
