using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync());
        }

        #region Create category
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {

            if (ModelState.IsValid)
            {
                // test trước có thêm được không
                //TempData["success"] = "Thêm sản phẩm thành công!";

                // tạo slug tự động từ Name
                brand.Slug = brand.Name.ToLower().Replace(" ", "-");
                // ktra slug đã tồn tại trong database chưa
                var slug = await _dataContext.Brands.Where(p => p.Slug == brand.Slug).FirstOrDefaultAsync();

                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Tên thương hiệu đã tồn tại. Vui lòng chọn tên khác.");

                    // Giữ lại dữ liệu người dùng đã nhập.
                    return View(brand);
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm thương hiệu thành công!";
                return RedirectToAction("Index");

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
            return View(brand);
        }
        #endregion

        #region Edit category
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                // 1. Tạo slug và kiểm tra trùng lặp
                var slug = brand.Name.ToLower().Replace(" ", "-");
                var checkSlug = await _dataContext.Brands
                    .Where(p => p.Slug == slug && p.Id != Id)
                    .FirstOrDefaultAsync();

                if (checkSlug != null)
                {
                    ModelState.AddModelError("Name", "Tên thương hiệu đã tồn tại. Vui lòng chọn tên khác.");
                    return View(brand);
                }

                // 2. Lấy sản phẩm hiện tại từ DB
                var existingBrand = await _dataContext.Brands.FindAsync(Id);
                if (existingBrand == null)
                {
                    return NotFound();
                }

                // 3. Cập nhật các thông tin cơ bản
                existingBrand.Name = brand.Name;
                existingBrand.Description = brand.Description;
                existingBrand.Status = brand.Status;
                existingBrand.Slug = slug;

                //_dataContext.update(category)

                // 5. Lưu thay đổi vào Database
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật thương hiệu thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Thông tin bạn nhập chưa hợp lệ. Vui lòng kiểm tra lại.";
                return View(brand); // Nên trả về View cùng dữ liệu cũ để người dùng sửa thay vì BadRequest text thô
            }
        }
        #endregion

        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);

            if (brand == null)
            {
                return NotFound();
            }

            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Thương hiệu đã xóa.";

            return RedirectToAction("Index");
        }
    }
}
