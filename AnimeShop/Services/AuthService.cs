using AnimeShop.Data;
using AnimeShop.Models;

namespace AnimeShop.Services
{
    public class AuthService
    {
        private readonly AnimeShopContext _context;

        public AuthService()
        {
            _context = new AnimeShopContext();
        }

        public Customer ValidateUserByEmailAndPassword(string email, string password)
        {
            return _context.Customers.SingleOrDefault(u => u.Email == email && u.Phone == password);
        }
    }
}
