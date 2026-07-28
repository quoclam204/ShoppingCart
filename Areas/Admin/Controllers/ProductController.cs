using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    // Phải đăng nhập bằng tài khoản Admin được mới vô được trang Admin Product
    //[Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        // chỉ đến thư mục lưu ảnh trong server -> dùng để upload file ảnh
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).
                Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        }

        // Thêm sản phẩm
        // GET: Hiển thị form
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            // lấy dữ liệu từ server hiển thị lên view
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");

            return View();
        }

        // POST: Nhận dữ liệu + lưu DB
        [HttpPost]
        [Route("Create")]
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

                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm đã tồn tại. Vui lòng chọn tên khác.");
                    return View(product);
                }
                else
                {
                    // Kiểm tra người dùng có upload ảnh hay không
                    if (product.ImageUpLoad != null)
                    {
                        // đường dẫn đến thư mục lưu ảnh trong server
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                        // tên ảnh để lưu vào database (đảm bảo không trùng tên)
                        string imageName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageUpLoad.FileName);
                        string filePath = Path.Combine(uploadDir, imageName);

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await product.ImageUpLoad.CopyToAsync(fs);
                        fs.Close();

                        // lưu tên ảnh vào database
                        product.Image = imageName;
                    }
                }
                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm sản phẩm thành công!";
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
            return View(product);
        }

        // Sửa sản phẩm
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }

        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {           
                // 1. Tạo slug và kiểm tra trùng lặp
                var slug = product.Name.ToLower().Replace(" ", "-");
                var checkSlug = await _dataContext.Products
                    .Where(p => p.Slug == slug && p.Id != Id)
                    .FirstOrDefaultAsync();

                if (checkSlug != null)
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm đã tồn tại. Vui lòng chọn tên khác.");
                    return View(product);
                }

                // 2. Lấy sản phẩm hiện tại từ DB
                var existingProduct = await _dataContext.Products.FindAsync(Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // 3. Cập nhật các thông tin cơ bản
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;
                existingProduct.Slug = slug;

                // 4. Xử lý tải lên ảnh mới (nếu có)
                if (product.ImageUpLoad != null)
                {
                    try
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                        // Bước A: Xóa ảnh cũ trên ổ đĩa nếu tồn tại
                        if (!string.IsNullOrEmpty(existingProduct.Image))
                        {
                            string oldPath = Path.Combine(uploadDir, existingProduct.Image);
                            // Tránh xóa ảnh mặc định của hệ thống nếu bạn có quy ước (ví dụ: noimage.png)
                            if (existingProduct.Image != "noimage.png" && System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Bước B: Lưu ảnh mới vào thư mục
                        string imageName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageUpLoad.FileName);
                        string filePath = Path.Combine(uploadDir, imageName);

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageUpLoad.CopyToAsync(fs);
                        }

                        // Bước C: Cập nhật tên ảnh mới vào database
                        existingProduct.Image = imageName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Đã xảy ra lỗi khi xử lý hình ảnh sản phẩm.");
                        return View(product);
                    }
                }

                // 5. Lưu thay đổi vào Database
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Thông tin bạn nhập chưa hợp lệ. Vui lòng kiểm tra lại.";
                return View(product); // Nên trả về View cùng dữ liệu cũ để người dùng sửa thay vì BadRequest text thô
            }
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
            {
                return NotFound();
            }

            // thư mục chứa ảnh
            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
            // Ghép thêm tên file ảnh. (đường dẫn đầy đủ tới file ảnh cụ thể)
            string oldfilePath = Path.Combine(uploadDir, product.Image);

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
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa ảnh sản phẩm.");
            }

            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Sản phẩm đã xóa.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("AddQuantity")]
        public async Task<IActionResult> AddQuantity(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        [Route("StoreProductQuantity")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreProductQuantity(ProductQuantityModel productQuantityModel)
        {
            var product = await _dataContext.Products.FindAsync(productQuantityModel.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            product.Quantity += productQuantityModel.Quantity;  

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateCreated = DateTime.Now;

            _dataContext.Add(productQuantityModel);
            await _dataContext.SaveChangesAsync();
            TempData ["success"] = "Cập nhật số lượng sản phẩm thành công!";

            return RedirectToAction("AddQuantity", "Product", new {Id = productQuantityModel.ProductId });
        }

    }
}
