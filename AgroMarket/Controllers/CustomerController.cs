using AgroMarket.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AgroMarket.Models;
using Microsoft.EntityFrameworkCore;
using AgroMarket.Models.Entities;
using System.Security.Claims;
using System.Reflection.Metadata;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroMarket.Controllers
{
 
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string categoryID)
        {
            var products = await _context.Products
                .Include(p => p.Review)
                .Include(p => p.ProductCategory) // Include related product categories
                .ThenInclude(pc => pc.Category) // Include categories of the product
                .ToListAsync();

            // If there's a search term, filter products by product name
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products
                    .Where(p => p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            string categoryName = null; // Initialize categoryName to null

            // If a category ID is provided, retrieve the corresponding category name
            /*if (categoryID.HasValue)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryID == categoryID.Value);

                if (category != null)
                {
                    categoryName = category.CategoryName; // Get the category name
                }
            }*/
            Console.WriteLine(categoryID + "......................");
            // If a category name is found, filter by category name
            if (!string.IsNullOrEmpty(categoryName))
            {
                products = products
                    .Where(p => p.ProductCategory.Any(pc => pc.Category.CategoryName == categoryName))
                    .ToList();
            }

            // Sorting logic
            switch (sortOrder)
            {
                case "price_low_high":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_high_low":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.CreatedAt).ToList();
                    break;
                case "popularity":
                default:
                    products = products.OrderByDescending(p => p.Review.Count).ToList();
                    break;
            }

            // Dictionary to store product ID and corresponding average rating
            var productRatings = new Dictionary<Guid, double>();
            foreach (var product in products)
            {
                double averageRating = 0;
                if (product.Review != null && product.Review.Any())
                {
                    averageRating = product.Review.Average(r => r.Rating);
                }
                productRatings[product.ProductID] = averageRating;
            }

            // Create the SelectList for categories
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryID", "CategoryName");

            // Pass the sort order, search term, category, and ratings to the view using ViewBag
            ViewBag.AverageRating = productRatings;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CategoryName = categoryName; // Pass the selected category name

            return View(new CustomerDashboardViewModel
            {
                Products = products
            });
        }


        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);
            var orders = await _context.Orders
                             .Include(o => o.OrderItem)
                                 .ThenInclude(oi => oi.Product)
                                     .ThenInclude(p => p.Retailer)
                             .Where(o => o.CustomerID == customerId)
                             .ToListAsync();
            var products = await _context.Products.ToListAsync();

            return View(new CustomerDashboardViewModel
            {
                Orders = orders,
                Products = products
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItem)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Retailer)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = order.OrderItem.Select(oi => new {
                ProductName = oi.Product?.ProductName,
                RetailerName = oi.Product?.Retailer?.FirstName + " " + oi.Product?.Retailer?.LastName
            }).ToList();

            return Json(orderDetails);  // Return the details as JSON for the frontend
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {
            try
            {
                // Get the customer ID (simulated for now)
                var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);

                // Check if the product exists
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }

                // Check if the cart entry already exists
                var cartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.CustomerID == customerId && c.ProductID == productId);

                if (cartItem != null)
                {
                    // Increment the quantity if the entry exists
                    cartItem.Quantity += quantity;
                }
                else
                {
                    // Create a new cart item if it doesn't exist
                    cartItem = new Cart
                    {
                        CustomerID = customerId,
                        ProductID = productId,
                        Quantity = quantity
                    };
                    await _context.Carts.AddAsync(cartItem);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Return success message without redirection
                return Json(new { success = true, message = "Product added to cart successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to add product to cart: " + ex.Message });
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitFeedback(Guid orderId, Guid productId, int rating, string comment)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItem)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var review = new Review
            {
                ProductID = productId,  // Use the provided productId
                CustomerID = order.CustomerID,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Store a success message in TempData
            TempData["FeedbackMessage"] = "Your feedback has been successfully submitted.";

            return RedirectToAction("Dashboard"); // Redirect back to the dashboard
        }



        [Authorize]
        public IActionResult GenerateInvoicePdf(Guid orderId)
    {
        // Fetch the order details
        var order = _context.Orders
            .Include(o => o.OrderItem)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.OrderID == orderId);

        if (order == null)
        {
            return NotFound();
        }

        // Create a memory stream for the PDF
        using (MemoryStream ms = new MemoryStream())
        {
                // Create a PDF document
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            PdfWriter.GetInstance(document, ms);

            // Open the PDF document
            document.Open();

            // Add content to the PDF
            document.Add(new Paragraph("Invoice"));
            document.Add(new Paragraph($"Order ID: {order.OrderID}"));
            document.Add(new Paragraph($"Order Date: {order.OrderDate.ToShortDateString()}"));
            document.Add(new Paragraph($"Total Amount: {order.TotalAmount:C}"));

            document.Add(new Paragraph("\nItems:"));
            foreach (var item in order.OrderItem)
            {
                document.Add(new Paragraph($"{item.Product.ProductName} - {item.Quantity} x {item.Price:C}"));
            }

            // Close the PDF document
            document.Close();

            // Convert the memory stream to a byte array and return as PDF file
            byte[] pdfBytes = ms.ToArray();
            return File(pdfBytes, "application/pdf", "Invoice.pdf");
        }
    }

}
}


