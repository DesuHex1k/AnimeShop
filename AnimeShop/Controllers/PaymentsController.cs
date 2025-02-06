using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimeShop.Models;

namespace AnimeShop.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly AnimeShopContext _context;

        public PaymentsController(AnimeShopContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Payments.ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .FirstOrDefaultAsync(m => m.PaymentId == id);

            return payment == null ? NotFound() : View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaymentId,CardNumber,Cvv,Name,Month,Year")] Payment payment, int id)
        {
            if (ModelState.IsValid)
            {
                payment.PaymentId = id;
                await AddPaymentAsync(payment);
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            return payment == null ? NotFound() : View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentId,CardNumber,Cvv,Name,Month,Year")] Payment payment)
        {
            if (id != payment.PaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await UpdatePaymentAsync(payment);
                    return RedirectToAction("Dashboard", "Customer");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.PaymentId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .FirstOrDefaultAsync(m => m.PaymentId == id);

            return payment == null ? NotFound() : View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                await DeletePaymentAsync(payment);
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helpers
        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }

        private async Task AddPaymentAsync(Payment payment)
        {
            _context.Add(payment);
            await _context.SaveChangesAsync();
        }

        private async Task UpdatePaymentAsync(Payment payment)
        {
            _context.Update(payment);
            await _context.SaveChangesAsync();
        }

        private async Task DeletePaymentAsync(Payment payment)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
