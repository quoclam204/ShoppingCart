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
    //[Authorize(Roles = "Publisher, Author, Admin")]
    public class SliderController : Controller
    {
        private readonly DataContext _dataContext;

        // chỉ đến thư mục lưu ảnh trong server -> dùng để upload file ảnh
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("Index")]
        public async Task<IActionResult> Index(int page = 1)
        {
            // Lấy ra danh sách sản phẩm trong csdl
            List<SliderModel> slider = _dataContext.Sliders.ToList();

            const int pageSize = 10; // 10 sản phẩm trên 1 trang

            if (page < 1)
            {
                page = 1;
            }

            int recsCount = slider.Count(); // đếm sô lượng sản phẩm

            var pager = new Paginate(recsCount, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = slider.Skip(recSkip).Take(pager.PageSize).ToList();

            // đưa đối tượng pager từ Controller sang View để View
            ViewBag.Pager = pager;

            return View(data);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel slider)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra người dùng có upload ảnh hay không
                if (slider.ImageUpLoad != null)
                {
                    // đường dẫn đến thư mục lưu ảnh trong server
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    // tên ảnh để lưu vào database (đảm bảo không trùng tên)
                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(slider.ImageUpLoad.FileName);
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpLoad.CopyToAsync(fs);
                    fs.Close();

                    // lưu tên ảnh vào database
                    slider.Image = imageName;
                }
                _dataContext.Add(slider);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm banner thành công!";
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
            return View(slider);
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);

            return View(slider);
        }

        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel slider)
        {
            var sliderExisting = await _dataContext.Sliders.FindAsync(slider.Id);

            if (ModelState.IsValid)
            {
                // Kiểm tra người dùng có upload ảnh hay không
                if (slider.ImageUpLoad != null)
                {
                    // đường dẫn đến thư mục lưu ảnh trong server
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    // tên ảnh để lưu vào database (đảm bảo không trùng tên)
                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(slider.ImageUpLoad.FileName);
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpLoad.CopyToAsync(fs);
                    fs.Close();

                    // lưu tên ảnh vào database
                    sliderExisting.Image = imageName;
                }

                // Cập nhật các trường khác của slider
                sliderExisting.Name = slider.Name;
                sliderExisting.Description = slider.Description;
                sliderExisting.Status = slider.Status;

                _dataContext.Update(sliderExisting);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật banner thành công!";
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
            return View(slider);
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);

            if (slider == null)
            {
                return NotFound();
            }

            // thư mục chứa ảnh
            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
            // Ghép thêm tên file ảnh. (đường dẫn đầy đủ tới file ảnh cụ thể)
            string oldfilePath = Path.Combine(uploadDir, slider.Image);

            // Kiểm tra sản phẩm có đang dùng ảnh mặc định không
            try
            {
                // kiểm tra file có tồn tại không
                if (System.IO.File.Exists(oldfilePath))
                {
                    System.IO.File.Delete(oldfilePath);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa ảnh banner.");
            }

            _dataContext.Sliders.Remove(slider);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Xóa banner thành công.";

            return RedirectToAction("Index");
        }
    }
}
