using AnimeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AnimeShop.Controllers
{
    public class WishlistsController(AnimeShopContext context) : Controller
    {
        // GET: Wishlists
        public async Task<IActionResult> Index()
        {
            if (!TryGetCustomerId(out int customerId))
            {
                return BadRequest("Invalid customer ID.");
            }

            var wishlistItems = await context.Wishlists
                .Include(w => w.Product)
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

            var product = await context.Products
                .Include(p => p.Category)
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
            if (!TryGetCustomerId(out int customerId))
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
            if (!TryGetCustomerId(out int customerId))
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
            var wishlistItem = await context.Wishlists
                .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId);

            if (wishlistItem != null)
            {
                context.Wishlists.Remove(wishlistItem);
            }
            else
            {
                var newWishlistItem = new Wishlist
                {
                    WishlistId = Guid.NewGuid().GetHashCode(),
                    CustomerId = customerId,
                    ProductId = productId
                };
                context.Wishlists.Add(newWishlistItem);
            }

            await context.SaveChangesAsync();
        }

        // GET: Wishlists/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(context.Customers, "CustomerId", "CustomerId");
            ViewData["ProductId"] = new SelectList(context.Products, "ProductId", "ProductId");
            return View();
        }

        // POST: Wishlists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,ProductId")] Wishlist wishlist)
        {
            if (ModelState.IsValid)
            {
                context.Add(wishlist);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(context.Customers, "CustomerId", "CustomerId", wishlist.CustomerId);
            ViewData["ProductId"] = new SelectList(context.Products, "ProductId", "ProductId", wishlist.ProductId);
            return View(wishlist);
        }

        // GET: Wishlists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var wishlist = await context.Wishlists.FindAsync(id);
            if (wishlist == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(context.Customers, "CustomerId", "CustomerId", wishlist.CustomerId);
            ViewData["ProductId"] = new SelectList(context.Products, "ProductId", "ProductId", wishlist.ProductId);
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
                    context.Update(wishlist);
                    await context.SaveChangesAsync();
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
            ViewData["CustomerId"] = new SelectList(context.Customers, "CustomerId", "CustomerId", wishlist.CustomerId);
            ViewData["ProductId"] = new SelectList(context.Products, "ProductId", "ProductId", wishlist.ProductId);
            return View(wishlist);
        }

        // GET: Wishlists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var wishlist = await context.Wishlists
                .Include(w => w.Product)
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
            var wishlist = await context.Wishlists.FindAsync(id);
            if (wishlist != null)
            {
                context.Wishlists.Remove(wishlist);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WishlistExists(int id)
        {
            return context.Wishlists.Any(e => e.WishlistId == id);
        }
        
        private bool TryGetCustomerId(out int customerId)
        {
            if (Request.Cookies.ContainsKey("CustomerId"))
            {
                customerId = int.Parse(Request.Cookies["CustomerId"] ?? throw new InvalidOperationException("Invalid customer ID."));
                return true;
            }

            customerId = 0;
            return false;
        }
    }
}
