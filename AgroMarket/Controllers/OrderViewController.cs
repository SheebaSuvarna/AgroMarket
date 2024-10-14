using System.Security.Claims;
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

            // Get the retailer ID from the logged-in user's claims
            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if RetailerID is not found
            }

            // Convert the retailer ID to a Guid
            var retailerID = Guid.Parse(retailerId);


            // Fetch the order items that belong to the logged-in retailer
            var orderItemsQuery = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Retailer)  // Include the retailer via Product
                .Where(oi => oi.Product.Retailer.RetailerID == retailerID)  // Filter by RetailerID (Guid comparison)
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
        public IActionResult UpdateOrderItemStatus(Guid orderItemId, string status)
        {
            // Find the OrderItem by ID
            var orderItem = _context.OrderItems.FirstOrDefault(o => o.OrderItemID == orderItemId);

            if (orderItem == null)
            {
                return Json(new { success = false, message = "Order item not found." });
            }

            // Update the status of the order item
            orderItem.Status = status;
            _context.SaveChanges();

            return Json(new { success = true });
        }


    }
}
