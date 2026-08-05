using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShoppingCart.Models;
using ShoppingCart.Models.ViewModels;
using ShoppingCart.Repository;

namespace ShoppingCart.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;

        public CartController(DataContext datacontext)
        {
            _dataContext = datacontext;
        }

        public IActionResult Index()
        {
            // lấy dữ liệu giỏ hàng từ session
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Lấy phí vận chuyển từ Cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            // Nếu shipping tồn tại
            if (shippingPriceCookie != null)
            {
                // Chuyển Cookie thành số
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }    

            // Nhận mã khuyến mãi từ cookie
            var coupon_code = Request.Cookies["CouponTitle"];

            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price), 
                ShippingCost = shippingPrice, // Tính phí ship
                CouponCode = coupon_code
            };

            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

        // thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddCartProduct(int Id)
        {
            // Lấy sản phẩm từ database
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            // Lấy giỏ hàng của user
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Kt sản phẩm đó có trong giỏ hàng chưa
            CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItems == null)
            {
                // nếu rỗng thì thêm sản phẩm mới dựa vào Id tìm được đưa vào giỏ hàng
                cart.Add(new CartItemModel(product));
            }
            else
            {
                // tăng số lượng sản phẩm đó lên
                cartItems.Quantity += 1;
            }

            // Cập nhật giỏ hàng vào bộ nhớ tạm của user
            HttpContext.Session.SetJson("Cart", cart);

            //TempData["success"] = "Thêm sản phẩm vào giỏ hàng thành công!";
            // coment vì lỗi hiển thị lại thông báo khi sử dung ajax (khi ấn vào nút quay lại trang)
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // giảm số lượng sản phẩm trong giỏ hàng
        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItem.Quantity > 1)
            {
                --cartItem.Quantity;
            }
            else
            {
                cart.RemoveAll(p => p.ProductId == Id);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            TempData["success"] = "Giảm số lượng sản phẩm trong giỏ hàng thành công!";
            return RedirectToAction("Index");
        }

        // tăng số lượng sản phẩm trong giỏ hàng
        public async Task<IActionResult> Increase(int Id)
        {
            ProductModel product = await _dataContext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
            {
                ++cartItem.Quantity;
                TempData["success"] = "Đã tăng số lượng sản phẩm trong giỏ hàng thành công!";
            }
            else
            {
                cartItem.Quantity = product.Quantity;
                TempData["error"] = "Không thể thêm sản phẩm vì đã đạt số lượng tối đa!";

                // cart.RemoveAll(p => p.ProductId == Id);
            }
                
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int Id)
        {
            // lấy danh sách Session của giỏ hàng
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            cart.RemoveAll(p => p.ProductId == Id);

            if (cart.Count == 0)
            {
                // nếu xóa hết sản phẩm trong giỏ hàng thì xóa luôn Session
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                // nếu 5 sp xóa 1 sp thì còn 4sp
                HttpContext.Session.SetJson("Cart", cart);
            }

            TempData["success"] = "Xóa sản phẩm khỏi giỏ hàng thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear(int Id)
        {
            HttpContext.Session.Remove("Cart");

            TempData["success"] = "Xóa tất cả sản phẩm khỏi giỏ hàng thành công!";
            return RedirectToAction("Index");
        }

        // Tính phí shipping dựa vào địa chỉ người dùng nhập vào
        [HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
        {
            var existingShipping = await _dataContext.Shippings
                .FirstOrDefaultAsync(s => s.Ward == phuong && s.District == quan && s.City == tinh);

            decimal shippingPrice = 0;

            // Nếu tìm được shipping trong database thì lấy giá, nếu lấy giá mặc định
            if (existingShipping != null)
            {
                shippingPrice = existingShipping.Price;
            }
            else
            {
                shippingPrice = 50000; // Giá mặc định nếu không tìm thấy
            }

            // Chuyển shippingPrice thành chuỗi JSON để có thể lưu vào Cookie.
            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice); 
            try
            {
                // Tạo cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(30), // Thời gian sống của cookie
                    Secure = true, // Chỉ gửi cookie qua HTTPS
                };

                // Lưu cookie
                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
            }   
            return Json(new { shippingPrice });
        }

        [HttpGet]
        [Route("Cart/DeleteShipping")]
        // xóa phí vận chuyển đã lưu trong Cookie.
        public async Task<IActionResult> DeleteShipping()
        {
            Response.Cookies.Delete("ShippingPrice"); // Xóa cookie có tên ShippingPrice
            //return Json(new { success = true }); // kiểm tra xóa có thành công không
            return RedirectToAction("Index", "Cart"); // Chuyển hướng về trang giỏ hàng.
        }

        // Áp dụng mã giảm giá
        [HttpPost]
        [Route("Cart/GetCoupon")]
        public async Task<IActionResult> GetCoupon(string coupon_value)
        {
            // Tìm mã khuyến mãi trong cơ sở dữ liệu theo tên mã người dùng nhập
            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(x => x.Name == coupon_value);

            // Ko tìm thấy mã khuyến mãi
            if (validCoupon == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "Không tìm thấy mã khuyến mãi."
                });
            }

            // Mã khuyến mãi hết hạn
            if (validCoupon.DateExpỉed < DateTime.Now)
            {
                return Ok(new
                {
                    success = false,
                    message = "Mã khuyến mãi đã hết hạn."
                });
            }

            // Tạo chuỗi thông tin mã khuyến mãi để lưu vào Cookie và hiển thị lên view
            string couponTitle = $"{validCoupon.Name} | {validCoupon.Description}";

            // Tạo cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            };

            // Lưu thông tin mã khuyến mãi vào cookie
            Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);

            // Trả kết quả thành công về cho Ajax
            return Ok(new
            {
                success = true,
                message = "Áp dụng mã khuyến mãi thành công."
            });
        }
    }
}
