using AnimeShop.Models;

namespace AnimeShop.ViewModels;
public partial class ProductViewModel
    {
        public List<Product> Products { get; set; }
        public List<int?> WishlistItems { get; set; }
    }