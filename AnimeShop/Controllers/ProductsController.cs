using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimeShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AnimeShop.Models;
using AnimeShop.ViewModels;

namespace AnimeShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AnimeShopContext _context;

        public ProductsController(AnimeShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search, string? sortOrder)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Фільтрація по категорії
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            // Пошук по імені
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name != null && p.Name.Contains(search));
            }

            // Сортування по ціні
            products = sortOrder switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                _ => products
            };

            var categories = await _context.Categories.ToListAsync();
            var wishlistItems = await GetWishlistItemsAsync();

            var viewModel = new ProductViewModel
            {
                Products = await products.ToListAsync(),
                WishlistItems = wishlistItems
            };

            ViewBag.Categories = categories;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearch = search;
            ViewBag.SelectedCategory = categoryId;

            return View(viewModel);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await GetProductWithReviewsAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["ProductId"] = id;
            ViewData["AverageRating"] = CalculateAverageRating(product.Reviews);

            if (TryGetCustomerId(out int customerId))
            {
                ViewData["CustomerId"] = customerId;
            }

            if (HttpContext.Request.Method == "POST")
            {
                await AddReviewAsync(id, customerId);
                return RedirectToAction("Details", new { id });
            }

            return View(product);
        }

        private async Task<Product> GetProductWithReviewsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Reviews)
                .ThenInclude(r => r.Customer)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        private double CalculateAverageRating(ICollection<Review> reviews)
        {
            return reviews.Any() ? reviews.Average(r => r.Rating) ?? 0 : 0;
        }

        private async Task AddReviewAsync(int productId, int customerId)
        {
            var review = new Review
            {
                ReviewId = Guid.NewGuid().GetHashCode(), // Використання GUID для унікальності
                Rating = int.Parse(Request.Form["Rating"]),
                ReviewText = Request.Form["ReviewText"],
                ReviewDate = DateOnly.FromDateTime(DateTime.Now),
                ProductId = productId,
                CustomerId = customerId
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        private bool TryGetCustomerId(out int customerId)
        {
            if (Request.Cookies.ContainsKey("CustomerId"))
            {
                customerId = int.Parse(Request.Cookies["CustomerId"]);
                return true;
            }

            customerId = 0;
            return false;
        }

        private async Task<List<int?>> GetWishlistItemsAsync()
        {
            if (TryGetCustomerId(out int customerId))
            {
                return await _context.Wishlists
                    .Where(w => w.CustomerId == customerId)
                    .Select(w => w.ProductId)
                    .ToListAsync();
            }

            return new List<int?>();
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Description,Price,Stock,Image,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Description,Price,Stock,Image,CategoryId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}