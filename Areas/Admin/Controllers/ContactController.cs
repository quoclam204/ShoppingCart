using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    //[Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;

        // chỉ đến thư mục lưu ảnh trong server -> dùng để upload file ảnh
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContactController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("Index")]
        public IActionResult Index()
        {
            var contact = _dataContext.Contacts.ToList();

            return View(contact);
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contacts.FirstOrDefaultAsync();

            return View(contact);
        }

        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            var contactExisting = await _dataContext.Contacts.FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                // Kiểm tra người dùng có upload ảnh hay không
                if (contact.ImageUpLoad != null)
                {
                    // đường dẫn đến thư mục lưu ảnh trong server
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/logo");
                    // tên ảnh để lưu vào database (đảm bảo không trùng tên)
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpLoad.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);
                        
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await contact.ImageUpLoad.CopyToAsync(fs);
                    fs.Close();

                    // Xóa ảnh cũ và lưu ảnh mới (nếu có)
                    contactExisting.LogoImg = imageName;
                }

                // Cập nhật các trường khác của slider
                contactExisting.Name = contact.Name;
                contactExisting.Map = contact.Map;
                contactExisting.Email = contact.Email;
                contactExisting.Phone = contact.Phone;
                contactExisting.Description = contact.Description;

                _dataContext.Update(contactExisting);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật thông tin website thành công!";
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
            return View(contact);
        }
    }
}
