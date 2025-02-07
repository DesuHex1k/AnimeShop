using Microsoft.CodeAnalysis.Scripting;

namespace AnimeShop.Models
{
    public class AuthService
    {
        private readonly AnimeShopContext _context;

        public AuthService()
        {
            _context = new AnimeShopContext();
        }

        public Customer ValidateUserByEmailAndPassword(string email, string phone)
        {
            return _context.Customers.SingleOrDefault(u => u.Email == email && u.Phone == phone);
        }
    }
}
