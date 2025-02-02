using System;
using System.Linq;
using System.Threading.Tasks;
using AnimeShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AnimeShop.Models;

namespace AnimeShop.Controllers
{
    public class WishlistsController : Controller
    {
        private readonly AnimeShopContext _context;

        public WishlistsController(AnimeShopContext context)
        {
            _context = context;
        }

        // GET: Wishlists
        public async Task<IActionResult> Index()
        {
            if (!Request.Cookies.ContainsKey("CustomerId"))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!int.TryParse(Request.Cookies["CustomerId"], out int customerId) || customerId <= 0)
            {
                return BadRequest("Invalid customer ID.");
            }

            var wishlistItems = await _context.Wishlists
                .Include(w => w.Product) // Include only necessary data
                .Where(w => w.CustomerId == customerId)
                .ToListAsync();

            return View(wishlistItems);
        }

        // GET: Wishlists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category) // Include only necessary data
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            if (!Request.Cookies.ContainsKey("CustomerId"))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!int.TryParse(Request.Cookies["CustomerId"], out int customerId) || customerId <= 0)
            {
                return BadRequest("Invalid customer ID.");
            }

            if (productId <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            await ToggleWishlistItem(customerId, productId);
            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleWishlistFromWishlist(int productId)
        {
            if (!Request.Cookies.ContainsKey("CustomerId"))
            {
                return Unauthorized();
            }

            if (!int.TryParse(Request.Cookies["CustomerId"], out int customerId) || customerId <= 0)
            {
                return BadRequest("Invalid customer ID.");
            }

            if (productId <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            await ToggleWishlistItem(customerId, productId);
            return RedirectToAction("Index");
        }

        // Private method to handle wishlist toggling
        private async Task ToggleWishlistItem(int customerId, int productId)
        {
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId);

            if (wishlistItem != null)
            {
                _context.Wishlists.Remove(wishlistItem);
            }
            else
            {
                var newWishlistItem = new Wishlist
                {
                    WishlistId = Guid.NewGuid().GetHashCode(),
                    CustomerId = customerId,
                    ProductId = productId
                };
                _context.Wishlists.Add(newWishlistItem);
            }

            await _context.SaveChangesAsync();
        }

        // GET: Wishlists/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            return View();
        }

        // POST: Wishlists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,ProductId")] Wishlist wishlist)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wishlist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", wishlist.CustomerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", wishlist.ProductId);
            return View(wishlist);
        }

        // GET: Wishlists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", wishlist.CustomerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", wishlist.ProductId);
            return View(wishlist);
        }

        // POST: Wishlists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WishlistId,CustomerId,ProductId")] Wishlist wishlist)
        {
            if (id != wishlist.WishlistId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wishlist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WishlistExists(wishlist.WishlistId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", wishlist.CustomerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", wishlist.ProductId);
            return View(wishlist);
        }

        // GET: Wishlists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.Product) // Include only necessary data
                .FirstOrDefaultAsync(m => m.WishlistId == id);

            if (wishlist == null)
            {
                return NotFound();
            }

            return View(wishlist);
        }

        // POST: Wishlists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist != null)
            {
                _context.Wishlists.Remove(wishlist);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WishlistExists(int id)
        {
            return _context.Wishlists.Any(e => e.WishlistId == id);
        }
    }
}