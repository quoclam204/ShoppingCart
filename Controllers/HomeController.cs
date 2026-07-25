using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var products = _dataContext.Products.Include("Category").Include("Brand").ToList();

            // tìm slider có status = 1 thì mới hiện thị
            var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
            ViewBag.Sliders = sliders;

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contacts.FirstOrDefaultAsync();

            return View(contact);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }
            else
            {
                // Trả về View mặc định của action hiện tại, có thể là View Error.cshtml
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddWishlist(int id)
        {
            // Lấy thông tin người dùng đang đăng nhập
            var user = await _userManager.GetUserAsync(User);

            var wishlistProduct = new WishlistModel
            {
                ProductId = id,
                UserId = user.Id
            };

            _dataContext.Wishlists.Add(wishlistProduct);

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm sản phẩm vào danh sách yêu thích thành công" });
            }
            catch (Exception)
            {
                _logger.LogError(500, "Đã xảy ra lỗi khi thêm sản phẩm vào danh sách yêu thích.");
            }

            return View();      
        }

        [HttpPost]
        public async Task<IActionResult> AddCompare(int id)
        {
            // Lấy thông tin người dùng đang đăng nhập
            var user = await _userManager.GetUserAsync(User);

            var comparetProduct = new CompareModel()
            {
                ProductId = id,
                UserId = user.Id
            };

            _dataContext.Compares.Add(comparetProduct);

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm so sánh sản phẩm thành công" });
            }
            catch (Exception)
            {
                _logger.LogError(500, "Đã xảy ra lỗi khi thêm so sánh sản phẩm.");
            }

            return View();
        }

        public async Task<IActionResult> Wishlist()
        {
            var wishlist_product = await ( from w in _dataContext.Wishlists
                                           join p in _dataContext.Products on w.ProductId equals p.Id
                                           join u in _dataContext.Users on w.UserId equals u.Id
                                           select new { User = u, Product = p, Wishlist = w  }).ToListAsync();

            return View(wishlist_product);
        }

        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from cp in _dataContext.Compares
                                          join p in _dataContext.Products on cp.ProductId equals p.Id
                                          join u in _dataContext.Users on cp.UserId equals u.Id
                                          select new { User = u, Product = p, Compare = cp }).ToListAsync();

            return View(compare_product);
        }

        [Route("DeleteWishlist")]
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);

            if (wishlist == null)
            {
                return NotFound();
            }

            _dataContext.Wishlists.Remove(wishlist);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Xóa yêu thích thành công.";

            return RedirectToAction("Wishlist", "Home");
        }

        [Route("DeleteCompare")]
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);

            if (compare == null)
            {
                return NotFound();
            }

            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Xóa so sánh thành công.";

            return RedirectToAction("Compare", "Home");
        }
    }
}
