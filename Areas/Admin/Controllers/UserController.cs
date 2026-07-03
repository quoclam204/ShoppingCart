using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    //[Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly DataContext _datdaContext;

        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(DataContext dataContext, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _datdaContext = dataContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]           
        [Route("Index")]
        public async Task <IActionResult> Index()
        {
            // Hiển thị Role theo User lên View
            // lấy được cả thông tin User và tên Role, rất phù hợp để hiển thị ở trang quản lý người dùng.
            var usersWithRoles = await (from u in _datdaContext.Users // Bảng Users

                                        // Ghép User với bảng trung gian AspNetUserRoles.
                                        join ur in _datdaContext.UserRoles on u.Id equals ur.UserId
                                        join r in _datdaContext.Roles on ur.RoleId equals r.Id

                                        select new { User = u, RoleName = r.Name }).ToListAsync();

            return View(usersWithRoles);

            /* chỉ lấy được danh sách User, không có thông tin Role.
            return View(await _userManager.Users.OrderByDescending(p => p.Id).ToListAsync()); */
        }

        #region Create User
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                // Tạo User
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);

                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email); // Tìm tk user(vừa tạo) theo Email
                    var role = await _roleManager.FindByIdAsync(user.RoleId); // Tìm role theo RoleId

                    // Thực hiện gán role cho user
                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(user);
                    }

                    return RedirectToAction("Index", "User");
                }
                else
                {
                    // Thêm các lỗi từ IdentityResult vào ModelState để hiển thị trên View
                    AddIdentityErrors(createUserResult);

                    // Hiển thị lại trang tạo tài khoản và giữ nguyên dữ liệu người dùng nhập
                    return View(user);
                }    
            }
            else // Nếu tạo User thất bại
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
        }
        #endregion

        #region Edit User
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            // Kiểm tra xem Id có null hoặc rỗng không
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Tìm user dựa trên Id vừa tìm thấy
            var user = await _userManager.FindByIdAsync(id);

            // Kiểm tra user có tồn tại không
            if (user == null)
            {
                return NotFound();
            }

            // Lấy thông tin của Dropdown
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(user);
        }

        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        { 
            var existingUser = await _userManager.FindByIdAsync(id); // Lấy user dựa vào id

            if (existingUser == null)
            {
                return NotFound();
            }    

            if (ModelState.IsValid)
            {
                // Gán User hiện tại = User được gửi từ form
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;

                // Thực hiện update user
                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }    
            }

            // Lấy danh sách Role để hiển thị lại Dropdown
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            // Thông báo dữ liệu nhập không hợp lệ
            TempData["error"] = "Thông tin bạn nhập chưa hợp lệ. Vui lòng kiểm tra lại.";

            // Lấy tất cả thông báo lỗi trong ModelState (nếu cần sử dụng)
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            string errorMessage = string.Join("\n", errors);

            // Trả về lại trang Edit để người dùng sửa thông tin
            return View(existingUser);
        }
        #endregion

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string Id)
        {
            // Kiểm tra xem Id có null hoặc rỗng không
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            // Tìm user dựa trên Id vừa tìm thấy
            var user = await _userManager.FindByIdAsync(Id);

            // Kiểm tra user có tồn tại không
            if (user == null)
            {
                return NotFound();
            }
            
            // Xóa User
            var deleteUserResult = await _userManager.DeleteAsync(user);

            // Nếu không thành công
            if (!deleteUserResult.Succeeded)
            {
                return View("Error");
            }

            TempData["success"] = "User đã xóa thành công"; 
            return RedirectToAction("Index");
        }

        // đưa các lỗi của Identity vào ModelState (Hàm hiển thị lỗi lên View)
        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
