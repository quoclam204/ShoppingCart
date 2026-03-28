using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).
                Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        }

        // GET: Hiển thị form
        [HttpGet]
        public IActionResult Create()
        {
            // lấy dữ liệu từ server hiển thị lên view
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");

            return View();
        }

        // POST: Nhận dữ liệu + lưu DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                TempData["success"] = "Thêm sản phẩm thành công!";
            }
            else
            {
                TempData["error"] = "Thông tin bạn nhập chưa hợp lệ. Vui lòng kiểm tra lại.";
                
                // lấy lỗi chi tiết
                List<string> errors = new List<string>();
                // ModelState.Values → tất cả field (Name, Price,…)
                foreach (var value in ModelState.Values)
                {
                    // value.Errors → lỗi của từng field
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }    
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
                return View(product);
        }
    }
}
