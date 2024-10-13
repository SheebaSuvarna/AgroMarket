using System.Security.Claims;
using AgroMarket.Data;
using AgroMarket.Models;
using AgroMarket.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [Authorize]
        // GET: Product/Create
        public IActionResult Create()
        {

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName"); // Load categories

            // Populate the ViewBag with existing products

            ViewBag.Products = _context.Products.Select(p => p.ProductName).ToList();

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,Description,Price,StockQuantity,ImageFile")] Product product, Guid CategoryID)
        {
            if (ModelState.IsValid)
            {
                var retailerId = User.FindFirstValue("RetailerID");
                if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
                {
                    return Forbid();
                }

                // Fetch the category
                var category = await _context.Categories.FindAsync(CategoryID);
                if (category == null)
                {
                    ModelState.AddModelError("", "Selected category not found.");
                    ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName");
                    ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                    return View(product);
                }

                product.ProductID = Guid.NewGuid();
                product.RetailerID = parsedRetailerId;
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;

                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    product.ImageUrl = await SaveImage(product.ImageFile);
                }

                _context.Add(product);

                // Create ProductCategory entry
                var productCategory = new ProductCategory
                {
                    ProductId = product.ProductID,
                    CategoryId = category.CategoryID
                };
                _context.ProductCategories.Add(productCategory);
                TempData["ProductSuccess"] = "Product Added Sucessfully!";

                await _context.SaveChangesAsync();
                return RedirectToAction("Index","Product");
            }
            TempData["ProductFail"] = "Fill All required fields";
            ViewBag.Products = _context.Products.Select(p => p.ProductName).ToList();
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName", CategoryID);

            return View(product);
        }


        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Get the image path from the product entity
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));  // Remove leading slash
            Console.WriteLine(imagePath);


            // Check if the image exists in the server folder
            if (System.IO.File.Exists(imagePath))
            {
                // Delete the image file
                System.IO.File.Delete(imagePath);
            }

            // Remove the product from the database
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(); // Return success status
        }

        [Authorize]
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/products/" + uniqueFileName; // This path should be relative to wwwroot
        }

        // GET: Product
        [Authorize]
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            // Retrieve the retailer's ID from the user claims
            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if RetailerID is not found
            }

            // Handle sorting logic
            ViewData["CurrentFilter"] = searchString;
            ViewData["SortOrder"] = sortOrder;

            // Retrieve products associated with the retailer
            var products = _context.Products
                                   .Where(p => p.RetailerID == parsedRetailerId)
                                   .Include(p => p.ProductCategory)
                                       .ThenInclude(pc => pc.Category) // Include Category details
                                   .AsQueryable(); // Make it IQueryable to apply filtering and sorting

            // Apply search filter if provided
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString) ||
                                                p.ProductCategory.Any(c => c.Category.CategoryName.Contains(searchString)));
            }

            // Apply sorting based on the selected sortOrder
            products = sortOrder switch
            {
                "price_asc" => products.OrderBy(p => p.Price),                // Price ascending (Low to High)
                "price_desc" => products.OrderByDescending(p => p.Price),     // Price descending (High to Low)
                "newest" => products.OrderByDescending(p => p.CreatedAt),
                _ => products.OrderBy(p => p.ProductName),                    // Default sorting by ProductName ascending
            };
            // Fetch the average rating for each product
            var productRatings = _context.Reviews
                .GroupBy(r => r.ProductID)
                .Select(g => new
                {
                    ProductID = g.Key,
                    AverageRating = g.Average(r => r.Rating)
                })
                .ToDictionary(r => r.ProductID, r => r.AverageRating);

            // Pass ratings via ViewBag
            ViewBag.ProductRatings = productRatings;

            var productList = await products.ToListAsync();

            // Return the entire view, AJAX will handle updating only the product list part
            return View(productList);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            // Retrieve the product with its related categories
            var product = await _context.Products
                .Include(p => p.ProductCategory)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            // Extracting the category names from ProductCategories
            var categoryNames = product.ProductCategory
                .Select(pc => pc.Category.CategoryName)
                .ToList();

            var categoryName = string.Join(",", categoryNames);

            var productDetails = new
            {
                product.ProductName,
                product.Description,
                categoryName,
                product.Price,
                product.StockQuantity,
                product.ImageUrl
            };

            return Json(productDetails);
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

        [HttpPatch("{id}")]
        public IActionResult Edit(Guid id, [FromForm] Product updatedProduct, [FromForm] IFormFile image, [FromForm] string categoryName)
        {
            var product = _context.Products.Include(p => p.ProductCategory)
                                           .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            // Update product fields
            product.ProductName = updatedProduct.ProductName;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.StockQuantity = updatedProduct.StockQuantity;

            // Handle image upload
            if (image != null && image.Length > 0)
            {
                var filePath = Path.Combine("path_to_images", image.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                product.ImageUrl = filePath; // Assuming ImageUrl is a property in Product model
            }

            // Find the category based on the passed CategoryName
            var category = _context.Categories.FirstOrDefault(c => c.CategoryName == categoryName);
            if (category != null)
            {
                var productCategory = product.ProductCategory.FirstOrDefault();
                if (productCategory != null && productCategory.CategoryId != category.CategoryID)
                {
                    productCategory.CategoryId = category.CategoryID; // Update CategoryID from the found category
                }
            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("GetAllCategories")]
        public IActionResult GetAllCategories()
        {
            var categories = _context.Categories
                .Select(c => new { c.CategoryID, c.CategoryName })
                .ToList();

            Console.WriteLine($"Categories retrieved: {categories.Count}"); // Log number of categories

            if (categories == null || categories.Count == 0)
            {
                return NotFound(); // Return 404 if no categories are found
            }

            return Ok(categories); // Return 200 OK with the categories data
        }

        public async Task<IActionResult> Review()
        {
            var retailerId = User.FindFirstValue("RetailerID");
            // Get all products for the specified retailer
            var products = await _context.Products
                .Where(p => p.RetailerID == Guid.Parse(retailerId)) // Assuming you have a RetailerID property
                .Include(p => p.Review.Where(r => r.Approved)) // Include only approved reviews
                .ToListAsync();

            return View(products);
        }

    }
}
