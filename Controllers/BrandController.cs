using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _datacontext;

        public BrandController(DataContext context)
        {
            _datacontext = context;
        }

        // Lấy danh sách sản phẩm theo Brand (dựa vào Slug) và hiển thị ra View
        public async Task<IActionResult> Index(string Slug = "")
        {
            BrandModel brand = _datacontext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();

            if (brand == null)
            {
                return RedirectToAction("Index");
            }

            // Lấy tất cả sản phẩm theo brand
            var productsByBrand = _datacontext.Products.Where(p => p.BrandId == brand.Id);

            return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
