using AnimeShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AnimeShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;

namespace AnimeShop.Controllers
{
    public class CartController : Controller
    {
        private readonly AnimeShopContext _context;

        public CartController(AnimeShopContext context)
        {
            _context = context;
        }

        public IActionResult AddToCart(int productId, int quantity)
        {
            
            if (!Request.Cookies.ContainsKey("CustomerId"))
            {
                return RedirectToAction("Login", "Auth");
            }
            var userId = int.Parse(Request.Cookies["CustomerId"]);
            var order = _context.Orders
                .Include(o => o.OrderItems) 
                .FirstOrDefault(o => o.CustomerId == userId && o.Status == "нове");

            if (order == null)
            {
                var maxOrderId = _context.Orders.Any()
                    ? _context.Orders.Max(o => o.OrdersId)
                    : 0;

                order = new Order
                {
                    OrdersId = maxOrderId + 1,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    CustomerId = userId,
                    Status = "нове"
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            var product = _context.Products.Find(productId);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            var existingOrderItem = order.OrderItems
                .FirstOrDefault(oi => oi.ProductId == productId);

            if (existingOrderItem == null)
            {
                var orderItem = new OrderItem
                {
                    OrderItemId = int.Parse($"{order.OrdersId}{productId}"), 
                    OrdersId = order.OrdersId,
                    ProductId = productId,
                    Quantity = quantity,
                    PricePerUnit = product.Price 
                };
                _context.OrderItems.Add(orderItem);
            }

            _context.SaveChanges();

            return RedirectToAction("Index", "Cart");
        }
        public IActionResult Index()
        {
            var userId = int.Parse(Request.Cookies["CustomerId"]);

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.CustomerId == userId && o.Status == "нове");

            if (order == null || !order.OrderItems.Any())
            {
                return RedirectToAction("Index", "Products");
            }

            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.PricePerUnit);
            _context.SaveChanges();

            return View(order.OrderItems);
        }



        public IActionResult RemoveFromCart(int orderItemId)
        {
            var orderItem = _context.OrderItems.Find(orderItemId);
            var product = _context.Products.Find(orderItem.ProductId);
            if (orderItem != null)
            {
                product.Stock += orderItem.Quantity;
                _context.OrderItems.Remove(orderItem);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
