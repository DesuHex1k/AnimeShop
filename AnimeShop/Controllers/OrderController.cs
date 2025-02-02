using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AnimeShop.Data;
using AnimeShop.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimeShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly AnimeShopContext _context;

        public OrderController(AnimeShopContext context)
        {
            _context = context;
        }

        public IActionResult CreateOrder()
        {
            var userId = int.Parse(Request.Cookies["CustomerId"]);

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.CustomerId == userId && o.Status == "нове");

            if (order == null || !order.OrderItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            order.Status = "оплачене";
            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.PricePerUnit);
            _context.SaveChanges();

            var maxOrderId = _context.Orders.Any()
                ? _context.Orders.Max(o => o.OrdersId)
                : 0;

            var newOrder = new Order
            {
                OrdersId = maxOrderId + 1,
                CustomerId = userId,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                Status = "нове"
            };

            _context.Orders.Add(newOrder);
            _context.SaveChanges();

            return RedirectToAction("OrderConfirmation", new { orderId = order.OrdersId });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.OrdersId == orderId);

            if (order == null)
            {
                return RedirectToAction("Index", "Products");
            }

            return View(order);
        }


        public IActionResult OrderHistory()
        {
            var userId = int.Parse(Request.Cookies["CustomerId"]);
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
    }
}
