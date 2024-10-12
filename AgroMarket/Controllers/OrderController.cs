using System.Security.Claims;
using AgroMarket.Data;
using AgroMarket.Models;
using AgroMarket.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateOrder()
        {
            var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);
            var cartItems = _context.Carts
                                    .Where(c => c.CustomerID == customerId)
                                    .Include(c => c.Product)
                                    .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart"); // No items in the cart
            }

            var model = new OrderViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity)
            };

            return View(model); // Render view for order confirmation and address input
        }

        [HttpPost]
        public IActionResult SubmitOrder(OrderViewModel model)
        {
            var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);
            var cartItems = _context.Carts
                                    .Where(c => c.CustomerID == customerId)
                                    .Include(c => c.Product)
                                    .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart"); // No items in the cart
            }

            var model2 = new OrderViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity)
            };

            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    CustomerID = customerId,  // Updated to fetch the actual customer ID
                    ShippingAddress = model.ShippingAddress,
                    TotalAmount = model2.TotalAmount,
                    Status = "Pending",
                    OrderDate = DateTime.Now
                };

                // Add OrderItems for each product in the cart
                order.OrderItem = cartItems.Select(c => new OrderItem
                {
                    ProductID = c.ProductID,
                    Quantity = c.Quantity,
                    Price = c.Product.Price,
                    Status = "Pending"
                }).ToList();

                _context.Orders.Add(order);
                _context.Carts.RemoveRange(cartItems); // Clear the cart after creating order
                _context.SaveChanges();

                return RedirectToAction("Payment", new { orderId = order.OrderID });
            }

            // If there is a validation issue, return back to the order page
            return View("CreateOrder", model);
        }


        
		public IActionResult Payment(Guid orderId)
		{
			var order = _context.Orders
								.Include(o => o.OrderItem)
								.ThenInclude(oi => oi.Product)
								.FirstOrDefault(o => o.OrderID == orderId);

			if (order == null)
			{
				return NotFound();
			}

			// Render payment page with order summary
			return View(order);
		}
		public IActionResult OrderConfirmed(Guid orderId)
		{
			var order = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
			if (order == null)
			{
				return NotFound();
			}

			return View(order); // Pass the order to the view for display
		}

		[HttpPost]
		public IActionResult ConfirmPayment(Guid orderId)
		{
			// Retrieve the order from the database
			var order = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);

			if (order == null)
			{
				return NotFound();
			}

			// Update the order status to "OrderConfirmed"
			order.Status = "OrderConfirmed";
			_context.SaveChanges();

			// Redirect to the Order Confirmation view
			return RedirectToAction("OrderConfirmed", new { orderId = orderId });
		}


	}

}

