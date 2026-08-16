using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Areas.Admin.Repository;
using ShoppingCart.Models;
using ShoppingCart.Models.ViewModels;
using ShoppingCart.Repository;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _dataContext;

        // Dịch vụ quản lý tài khoản và đăng nhập của ASP.NET Core Identity
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;

        private readonly IEmailSender _emailSender;

        // tự động tạo và truyền các đối tượng này vào Controller.
        public AccountController(DataContext context, UserManager<AppUserModel> userManage, SignInManager<AppUserModel> signInManager,
                                IEmailSender emailSender)
        {
            _dataContext = context;

            _userManage = userManage;
            _signInManager = signInManager;

            _emailSender = emailSender;
        }

        #region Đăng nhập tài khoản
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            // quay lại đúng trang người dùng đang truy cập trước đó.
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
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
            await _signInManager.SignOutAsync(); // Đăng xuất ra bằng tài khoản thường
            await HttpContext.SignOutAsync(); // Đăng xuất ra bằng tài khoản Google
            return Redirect(returnUrl);
        }

        // Hiển thị đơn hàng theo User
        [Route("account/history")]
        public async Task<IActionResult> History()
        {
            // Nếu người dùng chưa đăng nhập
            if ((bool)!User.Identity.IsAuthenticated)
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

        #region Quên mật khẩu
        // Hiển thị form quên mật khẩu
        public async Task<IActionResult> ForgetPass()
        {
            return View();
        }

        // Hàm gửi mail khi đổi mật khẩu
        [HttpPost]
        public async Task<IActionResult> SendMailForgetPass(AppUserModel user)
        {
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email không tồn tại trong hệ thống!";
                return RedirectToAction("ForgetPass", "Account");
            }
            else // Có email
            {
                string token = Guid.NewGuid().ToString(); // Tạo token ngẫu nhiên

                checkMail.Token = token; // Lưu token vào database
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();

                // Gửi email cho người dùng
                var receiver = checkMail.Email; // Email của người nhận
                var subject = "Thay đổi mật khẩu cho người dùng " + checkMail.Email;
                var message = "Bạn đã yêu cầu thay đổi mật khẩu. Vui lòng nhấn vào link sau để thay đổi mật khẩu: " +
                    "<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass" +
                    $"?email=" + checkMail.Email + "&token=" + token + "'>"; // Lấy đường dẫn tự động 

                // Gửi email
                await _emailSender.SendEmailAsync(receiver, subject, message);
            }

            TempData["success"] = "Vui lòng kiểm tra email để thay đổi mật khẩu!";
            return RedirectToAction("ForgetPass", "Account");
        }

        // Kiểm tra Email + Token
        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            // Kiểm tra xem người dùng có tồn tại trong database hay không
            var checkUser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkUser != null)
            {
                ViewBag.Email = checkUser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email hoặc Token không hợp lệ!";
                return RedirectToAction("ForgetPass", "Account");
            }

            return View();
        }

        // Cập nhật mật khẩu mới
        [HttpPost]
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkUser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(t => t.Token == user.Token).FirstOrDefaultAsync();

            if (checkUser != null)
            {
                string newToken = Guid.NewGuid().ToString();

                // Hash password
                var passWordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passWordHasher.HashPassword(checkUser, user.PasswordHash); // User hiện tại và mật khẩu muốn hash

                checkUser.PasswordHash = passwordHash; // Lưu mật khẩu mới vào database
                checkUser.Token = newToken; // Cập nhật token mới

                await _userManage.UpdateAsync(checkUser);
                TempData["success"] = "Cập nhật mật khẩu thành công!";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email hoặc Token không hợp lệ!";
                return RedirectToAction("ForgetPass", "Account");
            }
        }
        #endregion

        // Lấy thông tin người dùng khi đăng nhập 
        public async Task<IActionResult> UpdateAccount()
        {
            // Nếu người dùng chưa đăng nhập
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Thực hiện update thông tin người dùng mới
        [HttpPost]
        public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
        {
            // Lấy id người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy user hiện tại từ database
            var userById = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (userById == null)
            {
                return NotFound();
            }

            // Nếu có dữ liệu người dùng nhập thì cập nhật số điện thoại mới, không thì thôi
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                userById.PhoneNumber = user.PhoneNumber;
            }

            // Nếu có dữ liệu người dùng nhập thì cập nhật mật khẩu mới, không thì thôi
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                var passwordHasher = new PasswordHasher<AppUserModel>();

                var passwordHash = passwordHasher.HashPassword(
                    userById,
                    user.PasswordHash
                );

                userById.PasswordHash = passwordHash;
            }

            // Lưu database
            _dataContext.Update(userById);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Cập nhật thông tin tài khoản thành công!";


            return RedirectToAction("UpdateAccount", "Account");
        }

        #region Đăng nhập bằng google
        // Chuyển người dùng sang gg để đăng nhập
        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, 
                new AuthenticationProperties
                {
                    // xử lý thông tin Google trả về.
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        // nhận kết quả sau khi đăng nhập Google thành công.
        public async Task<IActionResult> GoogleResponse()
        {
            // Lấy thông tin đăng nhập từ cookie
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Lấy thông tin người dùng từ Google
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            TempData["success"] = "Đăng nhập bằng Google thành công!";
            return RedirectToAction("Index", "Home");

            //// Xem dữ liệu google trả về dưới dạng Json
            //return Json(claims); 
        }
        #endregion
    }
}
