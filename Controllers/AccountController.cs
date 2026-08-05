using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Models.ViewModels;
using ShoppingCart.Repository;
using System.Security.Claims;

namespace ShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _dataContext;

        // Dịch vụ quản lý tài khoản và đăng nhập của ASP.NET Core Identity
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;

        public AccountController(DataContext context, UserManager<AppUserModel> userManage, SignInManager<AppUserModel> signInManager)
        {
            _dataContext = context;

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

        [HttpPost]
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

        // Hiển thị đơn hàng theo User
        [Route("account/history")]
        public async Task<IActionResult> History()
        {
            // Nếu người dùng chưa đăng nhập
            if ((bool) !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Lấy ds đơn hàng theo Email người dùng đăng nhập, sau đó sx theo Id giảm dần (đơn hàng mới nhất sẽ hiển thị trước)
            var order = await _dataContext.Orders
                .Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();

            ViewBag.UserEmail = userEmail;
            return View(order); 
        }

        // Hủy đơn hàng theo OrderCode
        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Lấy đơn hàng theo OrderCode
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                order.Status = 3; 
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while canceling the order.");
            }

            return RedirectToAction("History", "Account");
        }
    }
}
