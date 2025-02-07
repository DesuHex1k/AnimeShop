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

        // POST: Customer/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer updatedCustomer)
        {
            if (!TryGetCustomerId(out int customerId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(updatedCustomer);
            }

            var customer = await GetCustomerById(customerId);

            if (customer == null)
            {
                return NotFound();
            }

            UpdateCustomerDetails(customer, updatedCustomer);

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
        private void UpdateCustomerDetails(Customer customer, Customer updatedCustomer)
        {
            customer.Email = updatedCustomer.Email;
            customer.Phone = updatedCustomer.Phone;
        }
        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}