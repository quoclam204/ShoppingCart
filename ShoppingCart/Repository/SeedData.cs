using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;

namespace ShoppingCart.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                // khởi tạo dữ liệu ban đầu cho database
                CategoryModel macbook = new CategoryModel { Name = "Macbook", Description = "Sản phẩm của Macbook", Slug = "macbook", Status = 1 };
                CategoryModel pc = new CategoryModel { Name = "Pc", Description = "Sản phẩm của Pc", Slug = "pc", Status = 1 };

                BrandModel apple = new BrandModel { Name = "Apple", Description = "Sản phẩm của Apple", Slug = "apple", Status = 1 };
                BrandModel samsung = new BrandModel { Name = "Samsung", Description = "Sản phẩm của Samsung", Slug = "samsung", Status = 1 };

                _context.Products.AddRange(
                    new ProductModel { Name = "Macbook", Description = "Laptop MacBook Pro M4 của Apple", Price = 35000000m, Brand = apple, Category = macbook, Slug = "macbook", Image = "https://example.com/iphone-14-pro-max.jpg" },
                    new ProductModel { Name = "Pc", Description = "Pc siêu Vip", Price = 30000000m, Brand = samsung, Category = pc, Slug = "pc", Image = "https://example.com/samsung-galaxy-s23-ultra.jpg" }
                );

                _context.SaveChanges();
            }
        }
    }
}
