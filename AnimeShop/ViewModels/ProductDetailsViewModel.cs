using AnimeShop.Models;

namespace AnimeShop.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
        public double? AverageRating { get; set; }
    }
}
