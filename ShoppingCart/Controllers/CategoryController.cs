using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _datacontext;

        public CategoryController(DataContext context)
        {
            _datacontext = context;
        }

        // Lấy danh sách sản phẩm theo Category (dựa vào Slug) và hiển thị ra View
        public async Task<IActionResult> Index(string Slug = "")
        {
            CategoryModel category = _datacontext.Categories.Where(c => c.Slug == Slug).FirstOrDefault();

            if (category == null)
            {
                return RedirectToAction("Index");
            }

            // Lấy tất cả sản phẩm theo category
            var productsByCategory = _datacontext.Products.Where(p => p.CategoryId == category.Id);

            return View(await productsByCategory.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
