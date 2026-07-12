using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    //[Authorize(Roles = "Publisher, Author")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index(int page = 1)
        {
            // Lấy ra danh sách sản phẩm trong csdl
            List<CategoryModel> category = _dataContext.Categories.ToList();

            const int pageSize = 10; // 10 sản phẩm trên 1 trang

            if (page < 1)
            {
                page = 1;
            }    

            int recsCount = category.Count(); // đếm sô lượng sản phẩm

            var pager = new Paginate(recsCount, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            // đưa đối tượng pager từ Controller sang View để View
            ViewBag.Pager = pager;

            return View(data);

            //return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToListAsync());
        }

        #region Create category
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {

            if (ModelState.IsValid)
            {
                // test trước có thêm được không
                //TempData["success"] = "Thêm sản phẩm thành công!";

                // tạo slug tự động từ Name
                category.Slug = category.Name.ToLower().Replace(" ", "-");
                // ktra slug đã tồn tại trong database chưa
                var slug = await _dataContext.Categories.Where(p => p.Slug == category.Slug).FirstOrDefaultAsync();

                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại. Vui lòng chọn tên khác.");

                    // Giữ lại dữ liệu người dùng đã nhập.
                    return View(category);
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm danh mục thành công!";
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
            return View(category);
        }
        #endregion

        #region Edit category
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            return View(category);
        }

        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                // 1. Tạo slug và kiểm tra trùng lặp
                var slug = category.Name.ToLower().Replace(" ", "-");
                var checkSlug = await _dataContext.Categories
                    .Where(p => p.Slug == slug && p.Id != Id)
                    .FirstOrDefaultAsync();

                if (checkSlug != null)
                {
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại. Vui lòng chọn tên khác.");
                    return View(category);
                }

                // 2. Lấy sản phẩm hiện tại từ DB
                var existingCategory = await _dataContext.Categories.FindAsync(Id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                // 3. Cập nhật các thông tin cơ bản
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.Status = category.Status;
                existingCategory.Slug = slug;

                //_dataContext.update(category)

                // 5. Lưu thay đổi vào Database
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Thông tin bạn nhập chưa hợp lệ. Vui lòng kiểm tra lại.";
                return View(category); // Nên trả về View cùng dữ liệu cũ để người dùng sửa thay vì BadRequest text thô
            }
        }
        #endregion

        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);

            if (category == null)
            {
                return NotFound();
            }

            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Danh mục đã xóa.";

            return RedirectToAction("Index");
        }
    }
}
