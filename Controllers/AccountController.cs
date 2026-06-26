using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Models.ViewModels;

namespace ShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        // Dịch vụ quản lý tài khoản và đăng nhập của ASP.NET Core Identity
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;

        public AccountController(UserManager<AppUserModel> userManage, SignInManager<AppUserModel> signInManager)
        {
            _userManage = userManage;
            _signInManager = signInManager;
        }

        #region Đăng nhập tài khoản
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            // quay lại đúng trang người dùng đang truy cập trước đó.
            return View(new LoginViewModel { ReturnUrl = returnUrl}); 
        }
                
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if(ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = 
                    await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);

                if (result.Succeeded)
                {
                    TempData["success"] = "Đăng nhập thành công!";
                    return Redirect(loginVM.ReturnUrl ?? "/"); 
                }    
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
            }    
            return View(loginVM); // Quay về trang Login.cshtml với @model LoginViewModel
        }
        #endregion

        #region Đăng ký tài khoản
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                // Tạo đối tượng User khi người dùng đăng ký tài khoản
                // Ko có password vì sẽ lưu mật khẩu vào database sẽ bị lộ
                AppUserModel newUser = new AppUserModel
                {
                    UserName = user.Username,
                    Email = user.Email
                };

                // Lưu User vào Database
                IdentityResult result = await _userManage.CreateAsync(newUser, user.Password);

                if (result.Succeeded)
                {
                    TempData["success"] = "Tạo tài khoản thành công!";
                    return Redirect("/account/login"); // quay lại trang đăng nhập
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }    

            }    
            return View(user);
        }
        #endregion

        // Đăng xuất tài khoản
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
}
