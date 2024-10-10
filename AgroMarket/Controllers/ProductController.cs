using System.Security.Claims;
using AgroMarket.Data;
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

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = _context.Products.Select(p => p.ProductName).ToList();
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName", CategoryID);

            return View(product);
        }

        [Authorize]
        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return Forbid();
            }
            var product = await _context.Products
                 .Include(p => p.ProductCategory) // Include ProductCategory relationship
                     .ThenInclude(pc => pc.Category) // Include Category relationship
                 .FirstOrDefaultAsync(m => m.ProductID == id && m.RetailerID == parsedRetailerId);
            if (product == null)
        {
                return NotFound();
            }

            return View(product);
        }


        [Authorize]
        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return Forbid();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductID == id && m.RetailerID == parsedRetailerId);
            if (product == null)
            {
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    // Delete the associated image file
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete the product. Please try again, and if the problem persists, contact support.");
                return View(product);
            }
        }

        [Authorize]
        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return Forbid();
            }
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductID == id && m.RetailerID == parsedRetailerId);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ProductID,ProductName,Description,Price,StockQuantity,ImageFile")] Product product)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return Forbid();
            }
            var existingProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ProductID == id && m.RetailerID == parsedRetailerId);

            if (existingProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.RetailerID = parsedRetailerId;
                    product.UpdatedAt = DateTime.Now;

                    if (product.ImageFile != null)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        // Save new image
                        product.ImageUrl = await SaveImage(product.ImageFile);
                    }
                    else
                    {
                        // Keep the existing image URL if no new image is uploaded
                        product.ImageUrl = existingProduct.ImageUrl;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }




        // GET: Product
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if RetailerID is not found
            }

            var products = await _context.Products
        .ToListAsync();

                
            return View(products);
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

        [Authorize]
        [HttpGet]
        // GET: Product/Details/5
        public async Task<IActionResult> Details(Guid? id)
            {
            if (id == null)
                {
                return NotFound();
                }

            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return Forbid();
            }
            var product = await _context.Products
                .Include(p => p.Retailer)
                .Include(p => p.ProductCategory) // Include the ProductCategory relationship
                 .ThenInclude(pc => pc.Category)// Include the Retailer information
                .FirstOrDefaultAsync(m => m.ProductID == id && m.RetailerID == parsedRetailerId);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }



        /*[Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var retailerId = User.FindFirstValue("RetailerID");
            if (string.IsNullOrEmpty(retailerId) || !Guid.TryParse(retailerId, out Guid parsedRetailerId))
            {
                return Forbid();
            }

            var product = await _context.Products
                .Include(p => p.Retailer)
                .Include(p => p.ProductCategory)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id && m.RetailerID == parsedRetailerId);

            if (product == null)
            {
                return NotFound();
    }

            return PartialView("_ProductDetailsPartial", product); // Return partial view instead
        }*/


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
