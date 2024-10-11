using AgroMarket.Data;
using AgroMarket.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AgroMarket.Controllers
{
    [Authorize]
    public class OrderViewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderViewController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> ViewOrderItems(string sortOrder, string searchString)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilter = searchString;

            // Get the order items with related Order and Product data
            var orderItemsQuery = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .AsQueryable();

            // Apply search filter first
            if (!string.IsNullOrEmpty(searchString))
            {
                orderItemsQuery = orderItemsQuery
                    .Where(oi => oi.Product.ProductName.Contains(searchString));
            }

            // Apply sorting after search
            switch (sortOrder)
            {
               
                case "price_asc":
                    orderItemsQuery = orderItemsQuery.OrderBy(oi => oi.Price);
                    break;

                case "price_desc":
                    orderItemsQuery = orderItemsQuery.OrderByDescending(oi => oi.Price);
                    break;

                case "newest":
                    orderItemsQuery = orderItemsQuery.OrderByDescending(oi => oi.Order.OrderDate);
                    break;

                default:
                    orderItemsQuery = orderItemsQuery.OrderBy(oi => oi.Order.OrderDate); // Default sort
                    break;
            }

            // Execute the query and pass the result to the view
            var orderItems = await orderItemsQuery.ToListAsync();
            return View(orderItems);
        }





        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Completed")
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ViewOrderItems));
        }

    }
}
