using AgroMarket.Data;
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

        public async Task<IActionResult> ViewOrderItems(string sortOrder, string searchString)
        {
            // Store the current sort order
            ViewBag.CurrentSort = sortOrder;

            // Store the current filter for the search input
            ViewBag.CurrentFilter = searchString;

            // Retrieve order items with related entities
            var orderItems = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .ToListAsync();

            // Apply search filtering based on the product name
            if (!string.IsNullOrEmpty(searchString))
            {
                orderItems = orderItems.Where(o => o.Product.ProductName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply sorting logic
            orderItems = sortOrder switch
            {
                "date_asc" => orderItems.OrderBy(o => o.Order.OrderDate).ToList(),
                "date_desc" => orderItems.OrderByDescending(o => o.Order.OrderDate).ToList(),
                _ => orderItems
            };

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
