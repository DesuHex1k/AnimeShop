using Microsoft.AspNetCore.Mvc;
using AnimeShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AnimeShop.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AnimeShopContext _context;

        public CustomerController(AnimeShopContext context)
        {
            _context = context;
        }

        // GET: Customer/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!TryGetCustomerId(out int customerId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var customer = await GetCustomerById(customerId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
        private bool TryGetCustomerId(out int customerId)
        {
            customerId = 0;
            if (!Request.Cookies.ContainsKey("CustomerId") ||
                !int.TryParse(Request.Cookies["CustomerId"], out customerId) ||
                customerId <= 0)
            {
                return false;
            }
            return true;
        }
        private async Task<Customer> GetCustomerById(int customerId)
        {
            return await _context.Customers
                .Include(c => c.Address)
                .Include(c => c.Payment)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

            // GET: Customer/Edit
            [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!Request.Cookies.ContainsKey("CustomerId"))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!int.TryParse(Request.Cookies["CustomerId"], out int customerId) || customerId <= 0)
            {
                return BadRequest("Invalid customer ID.");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer updatedCustomer)
        {
            if (!Request.Cookies.ContainsKey("CustomerId"))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!int.TryParse(Request.Cookies["CustomerId"], out int customerId) || customerId <= 0)
            {
                return BadRequest("Invalid customer ID.");
            }

            if (!ModelState.IsValid)
            {
                return View(updatedCustomer);
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
            {
                return NotFound();
            }

            // Update customer details
            customer.Email = updatedCustomer.Email;
            customer.Phone = updatedCustomer.Phone;

            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(customerId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Dashboard");
        }

        // Helper method to check if a customer exists
        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}