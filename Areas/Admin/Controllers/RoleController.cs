using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShoppingCart.Models;
using ShoppingCart.Repository;
using System.Threading.Tasks;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    //[Authorize(Roles = "Admin)]
    public class RoleController : Controller
    {
        private readonly DataContext _datdaContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(DataContext context, RoleManager<IdentityRole> roleManager)
        {
            _datdaContext = context;
            _roleManager = roleManager;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _datdaContext.Roles.OrderByDescending(p => p.Id).ToListAsync());
        }

        #region Create Role
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole role)
        {
            // Kiểm tra nếu Role không tồn tại thì tạo Role mới
            // -> Nếu Role chưa tồn tại thì thực hiện tạo Role mới.
            if (!await _roleManager.RoleExistsAsync(role.Name))
            {
                await _roleManager.CreateAsync(new IdentityRole(role.Name));
            }
            return Redirect("Index");
        }
        #endregion

        #region Edit Role
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            return View(role);
        }

        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            // Nếu ko có Id quay về trong 404
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(id);

                if (role == null)
                {
                    return NotFound();
                }

                // gán lại tên mới cho role
                role.Name = model.Name;

                try
                {
                    // Thực hiện update
                    await _roleManager.UpdateAsync(role);
                    TempData["success"] = "Cập nhật role thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi khi cập nhật role: {ex.Message}");
                }
            }

            // NẾu thất bại hiển thị lại form và giữ nguyên dữ liệu đã nhập 
            return View(model ?? new IdentityRole { Id = id});
        }
        #endregion

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            // Nếu ko có Id quay về trong 404
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }    

            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Xóa role thành công!";
            }  
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi khi xóa role: {ex.Message}");
            }

            return Redirect("Index");
        }

    }
}