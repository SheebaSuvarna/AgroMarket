using AgroMarket.Models.Entities;
using AgroMarket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace AgroMarket.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard (async)
        [HttpGet]
        [Route("dashboard")]
        public async Task<IActionResult> Dashboard(string activeTab = "customers")  // default tab is "customers"
        {
            // Fetch customers, retailers, products, categories, reviews, and orders asynchronously
            var customers = await _context.Customers.ToListAsync();
            var retailers = await _context.Retailers.ToListAsync();
            var products = await _context.Products.ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var reviews = await _context.Reviews
                                        .Include(r => r.Product)
                                        .Include(r => r.Customer)
                                        .ToListAsync();
            var orders = await _context.Orders
                                        .Include(o => o.Customer)        // Include Customer related to the order
                                        .Include(o => o.OrderItem)       // Include Order Items
                                            .ThenInclude(oi => oi.Product) // Include Product within Order Items
                                        .ToListAsync();  // Fetch Orders with Order Items and Product info

            // Prepare data for analytics
            var productLabels = products.Select(p => p.ProductName).ToList();
            var productQuantities = products.Select(p => p.StockQuantity).ToList();

            // Prepare monthly sales data asynchronously
            var salesData = orders
                .GroupBy(o => o.OrderDate.Date)  // Group by day 
                .Select(g => new
                {
                    Date = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(s => s.Date) // Correctly order by Date
                .Select(s => new
                {
                    Date = s.Date.ToString("dd MMM yyyy"),  // Format as "Day Month Year"
                    TotalSales = s.TotalSales
                })
                .ToList();

            var salesLabels = salesData.Select(s => s.Date).ToList();
            var dailySales = salesData.Select(s => s.TotalSales).ToList();

            // Create a dynamic object to pass to the view
            dynamic model = new ExpandoObject();
            model.Customers = customers;
            model.Retailers = retailers;
            model.Products = products;
            model.Categories = categories;
            model.Reviews = reviews;
            model.Orders = orders;
            model.ActiveTab = activeTab;  // Pass active tab to keep UI state

            // Add analytics data to the model
            model.ProductLabels = productLabels;
            model.ProductQuantities = productQuantities;
            model.SalesLabels = salesLabels;
            model.DailySales = dailySales;

            // Add total counts to the model
            model.TotalRetailers = await _context.Retailers.CountAsync();
            model.TotalCustomers = await _context.Customers.CountAsync();
            model.TotalProducts = await _context.Products.CountAsync();

            return View(model);  // Pass the model to the view
        }

        // Edit product (GET)
        [HttpGet]
        [Route("edit-product/{id}")]
        public async Task<IActionResult> EditProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Edit product (POST)
        [HttpPost]
        [Route("edit-product/{id}")]
        public async Task<IActionResult> EditProduct(Guid id, Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.ProductName = updatedProduct.ProductName;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.StockQuantity = updatedProduct.StockQuantity;
            product.UpdatedAt = DateTime.Now;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "products" });
        }

        // Delete product
        [HttpPost]
        [Route("delete-product/{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "products" });
        }

        // Review Management: Approve or delete reviews
        [HttpGet]
        [Route("reviews")]
        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _context.Reviews.Include(r => r.Product).Include(r => r.Customer).ToListAsync();
            return View(reviews);
        }

        // Approve review
        [HttpPost]
        [Route("approve-review/{id}")]
        public async Task<IActionResult> ApproveReview(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            review.Approved = true;  // Assuming an 'Approved' field

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "reviews" });
        }

        // Delete review
        [HttpPost]
        [Route("delete-review/{id}")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "reviews" });
        }

        // Edit User (GET)
        [HttpGet]
        [Route("edit-user/{id}")]
        public async Task<IActionResult> EditUser(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // Edit User (POST)
        [HttpPost]
        [Route("edit-user/{id}")]
        public async Task<IActionResult> EditUser(Guid id, Customer updatedCustomer)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            customer.FirstName = updatedCustomer.FirstName;
            customer.LastName = updatedCustomer.LastName;
            customer.Email = updatedCustomer.Email;
            customer.UpdatedAt = DateTime.Now;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // Delete User
        [HttpPost]
        [Route("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // Edit Retailer (GET)
        [HttpGet]
        [Route("edit-retailer/{id}")]
        public async Task<IActionResult> EditRetailer(Guid id)
        {
            var retailer = await _context.Retailers.FindAsync(id);
            if (retailer == null)
            {
                return NotFound();
            }
            return View(retailer);
        }

        // Edit Retailer (POST)
        [HttpPost]
        [Route("edit-retailer/{id}")]
        public async Task<IActionResult> EditRetailer(Guid id, Retailer updatedRetailer)
        {
            var retailer = await _context.Retailers.FindAsync(id);
            if (retailer == null)
            {
                return NotFound();
            }

            retailer.FirstName = updatedRetailer.FirstName;
            retailer.LastName = updatedRetailer.LastName;
            retailer.Email = updatedRetailer.Email;
            retailer.FarmName = updatedRetailer.FarmName;
            retailer.UpdatedAt = DateTime.Now;

            _context.Retailers.Update(retailer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "retailers" });
        }

        // Delete Retailer
        [HttpPost]
        [Route("delete-retailer/{id}")]
        public async Task<IActionResult> DeleteRetailer(Guid id)
        {
            var retailer = await _context.Retailers.FindAsync(id);
            if (retailer == null)
            {
                return NotFound();
            }

            _context.Retailers.Remove(retailer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "retailers" });
        }

        // Create new category (GET)
        [HttpGet]
        [Route("create-category")]
        public IActionResult CreateCategory()
        {
            var category = new Category(); // Initialize a new Category object
            return View(category);         // Return empty category model to the view
        }

        // Create new category (POST)
        [HttpPost]
        [Route("create-category")]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", new { activeTab = "categories" });
            }
            return View(category);
        }

        // Edit category (GET)
        [HttpGet]
        [Route("edit-category/{id}")]
        public async Task<IActionResult> EditCategory(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Edit category (POST)
        [HttpPost]
        [Route("edit-category/{id}")]
        public async Task<IActionResult> EditCategory(Guid id, Category updatedCategory)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.CategoryName = updatedCategory.CategoryName;
            category.Description = updatedCategory.Description;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "categories" });
        }

        // Delete category
        [HttpPost]
        [Route("delete-category/{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "categories" });
        }

        // Order Management: View all order details
        [HttpGet]
        [Route("orders")]
        public async Task<IActionResult> ManageOrders()
        {
            var orders = await _context.Orders
                                        .Include(o => o.Customer)
                                        .Include(o => o.OrderItem).ThenInclude(oi => oi.Product)
                                        .ToListAsync();

            return View(orders); // Pass the orders data to the view
        }

        // Edit Order (GET)
        [HttpGet]
        [Route("edit-order/{id}")]
        public async Task<IActionResult> EditOrder(Guid id)
        {
            var order = await _context.Orders
                                      .Include(o => o.Customer)
                                      .Include(o => o.OrderItem).ThenInclude(oi => oi.Product)
                                      .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                return NotFound(); // Order not found
            }

            return View(order); // Pass the order to the view
        }

        // Edit Order (POST)
        [HttpPost]
        [Route("edit-order/{id}")]
        public async Task<IActionResult> EditOrder(Guid id, Order updatedOrder)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(); // Order not found
            }

            order.ShippingAddress = updatedOrder.ShippingAddress;
            order.Status = updatedOrder.Status;
            order.TotalAmount = updatedOrder.TotalAmount;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "orders" });
        }

        // Delete Order
        [HttpPost]
        [Route("delete-order/{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(); // Order not found
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { activeTab = "orders" });
        }
    }
}
