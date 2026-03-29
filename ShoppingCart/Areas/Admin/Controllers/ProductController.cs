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
                // test trước có thêm được không
                //TempData["success"] = "Thêm sản phẩm thành công!";

                // thêm dữ liệu
                product.Slug = product.Name.ToLower().Replace(" ", "-");
                var slug = await _dataContext.Products.Where(p => p.Slug == product.Slug).FirstOrDefaultAsync();

                if(slug != null)
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm đã tồn tại. Vui lòng chọn tên khác.");
                    return View(product);
                }
                else
                {
                    if(product.ImageUpLoad != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageUpLoad.FileName);
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageUpLoad.CopyToAsync(stream);
                        }
                        product.Image = fileName;
                    }
                }    

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
