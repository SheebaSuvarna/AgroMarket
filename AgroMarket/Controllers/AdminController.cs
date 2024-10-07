using AgroMarket.Models.Entities;
using AgroMarket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Admin Dashboard
        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard(string activeTab = "customers")  // default tab is "customers"
        {
            var customers = _context.Customers.ToList();
            var retailers = _context.Retailers.ToList();
            var products = _context.Products.ToList();
            var categories = _context.Categories.ToList();  // Fetch Categories
            var reviews = _context.Reviews.Include(r => r.Product).Include(r => r.Customer).ToList();

            var dashboardData = new
            {
                Customers = customers,
                Retailers = retailers,
                Products = products,
                Reviews = reviews,
                Categories = categories,  // Pass Categories to view
                ActiveTab = activeTab      // Pass active tab to view
            };

            return View(dashboardData);   // Pass entire data to view
        }

        // User Management: Monitor and manage actions taken by Customers and Retailers
        [HttpGet]
        [Route("users")]
        public IActionResult ManageUsers()
        {
            var customers = _context.Customers.ToList();
            var retailers = _context.Retailers.ToList();

            return View(new { Customers = customers, Retailers = retailers });
        }

        // Produce Management: View all produce listings
        [HttpGet]
        [Route("products")]
        public IActionResult ManageProduce()
        {
            var products = _context.Products.Include(p => p.Retailer).ToList();
            return View(products);
        }

        // Edit product
        [HttpGet]
        [Route("edit-product/{id}")]
        public IActionResult EditProduct(Guid id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [Route("edit-product/{id}")]
        public IActionResult EditProduct(Guid id, Product updatedProduct)
        {
            var product = _context.Products.Find(id);
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
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        // Delete product
        [HttpPost]
        [Route("delete-product/{id}")]
        public IActionResult DeleteProduct(Guid id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        // Review Management: Approve or delete reviews
        [HttpGet]
        [Route("reviews")]
        public IActionResult ManageReviews()
        {
            var reviews = _context.Reviews.Include(r => r.Product).Include(r => r.Customer).ToList();
            return View(reviews);
        }

        // Approve review
        [HttpPost]
        [Route("approve-review/{id}")]
        public IActionResult ApproveReview(Guid id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null)
            {
                return NotFound();
            }

            // Assuming you have an 'Approved' status for reviews
            review.Approved = true;

            _context.Reviews.Update(review);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", new { activeTab = "reviews" });
        }

        // Delete review
        [HttpPost]
        [Route("delete-review/{id}")]
        public IActionResult DeleteReview(Guid id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", new { activeTab = "reviews" });
        }

        // Edit User
        [HttpGet]
        [Route("edit-user/{id}")]
        public IActionResult EditUser(Guid id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [Route("edit-user/{id}")]
        public IActionResult EditUser(Guid id, Customer updatedCustomer)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Update customer properties
            customer.FirstName = updatedCustomer.FirstName;
            customer.LastName = updatedCustomer.LastName;
            customer.Email = updatedCustomer.Email;
            customer.UpdatedAt = DateTime.Now;

            _context.Customers.Update(customer);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        // Delete User
        [HttpPost]
        [Route("delete-user/{id}")]
        public IActionResult DeleteUser(Guid id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        // Edit Retailer
        [HttpGet]
        [Route("edit-retailer/{id}")]
        public IActionResult EditRetailer(Guid id)
        {
            var retailer = _context.Retailers.Find(id);
            if (retailer == null)
            {
                return NotFound();
            }
            return View(retailer);
        }

        [HttpPost]
        [Route("edit-retailer/{id}")]
        public IActionResult EditRetailer(Guid id, Retailer updatedRetailer)
        {
            var retailer = _context.Retailers.Find(id);
            if (retailer == null)
            {
                return NotFound();
            }

            // Update retailer properties
            retailer.FirstName = updatedRetailer.FirstName;
            retailer.LastName = updatedRetailer.LastName;
            retailer.Email = updatedRetailer.Email;
            retailer.FarmName = updatedRetailer.FarmName;
            retailer.UpdatedAt = DateTime.Now;

            _context.Retailers.Update(retailer);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        // Delete Retailer
        [HttpPost]
        [Route("delete-retailer/{id}")]
        public IActionResult DeleteRetailer(Guid id)
        {
            var retailer = _context.Retailers.Find(id);
            if (retailer == null)
            {
                return NotFound();
            }

            _context.Retailers.Remove(retailer);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
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
        public IActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Dashboard", new { activeTab = "categories" });  // Redirect to categories tab
            }
            return View(category);
        }

        // Edit category (GET)
        [HttpGet]
        [Route("edit-category/{id}")]
        public IActionResult EditCategory(Guid id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Edit category (POST)
        [HttpPost]
        [Route("edit-category/{id}")]
        public IActionResult EditCategory(Guid id, Category updatedCategory)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            category.CategoryName = updatedCategory.CategoryName;
            category.Description = updatedCategory.Description;

            _context.Categories.Update(category);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", new { activeTab = "categories" });
        }

        // Delete category
        [HttpPost]
        [Route("delete-category/{id}")]
        public IActionResult DeleteCategory(Guid id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", new { activeTab = "categories" });
        }
    }
}
