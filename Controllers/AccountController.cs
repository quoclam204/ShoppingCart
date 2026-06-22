using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;

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

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Login()
        {
            return View();
        }

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
                AppUserModel newUser = new AppUserModel
                {
                    UserName = user.Username,
                    Email = user.Email
                };
                IdentityResult result = await _userManage.CreateAsync(newUser);

                if (result.Succeeded)
                {
                    TempData["success"] = "Tạo tài khoản thành công!";
                    return Redirect("/account");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }    

            }    
            return View(user);
        }
    }
}
