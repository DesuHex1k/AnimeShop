using Microsoft.AspNetCore.Mvc;
using AnimeShop.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimeShop.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly AnimeShopContext _context;

        public AuthController(AuthService authService, AnimeShopContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _authService.ValidateUserByEmailAndPassword(model.Email, model.Password);
                if (user != null)
                {
                    SetCustomerCookie(user.CustomerId);
                    return RedirectToAction("Dashboard", "Customer");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password");
                }
            }
            return View(model);
        }
        private void SetCustomerCookie(int customerId)
        {
            Response.Cookies.Append("CustomerId", customerId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (model.Password == model.ConfirmPassword)
            {
                var newCustomer = new Customer
                {
                    CustomerId = DateTime.Now.Second,
                    Email = model.Email,
                    Phone = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    AddressId = DateTime.Now.Second,  
                    PaymentId = DateTime.Now.Second    
                };

                var newAddress = new Address { AddressId = newCustomer.CustomerId };
                var newPayment = new Payment { PaymentId = newCustomer.CustomerId };

                _context.Customers.Add(newCustomer);
                _context.Addresses.Add(newAddress); 
                _context.Payments.Add(newPayment); 
                _context.SaveChanges();

                Response.Cookies.Append("CustomerId", newCustomer.CustomerId.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                return RedirectToAction("Dashboard", "Customer");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Паролі не співпадають.");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("CustomerId");
            Response.Cookies.Delete("WishlistItems");

            return RedirectToAction("Login", "Auth");
        }
    }
}
