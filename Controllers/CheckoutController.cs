using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingCart.Areas.Admin.Repository;
using ShoppingCart.Models;
using ShoppingCart.Repository;
using System.Security.Claims;

namespace ShoppingCart.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;

        public CheckoutController(DataContext context, IEmailSender emailSender)
        {
            _dataContext = context;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Checkout()
        {
            // FindFirstValue: lấy thông tin người dùng đang đăng nhập
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // Thông tin đơn hàng
                // tạo ra mã Random và không bao giờ trùng
                var oderCode = Guid.NewGuid().ToString();
                var oderItem = new OrderModel();
                oderItem.OrderCode = oderCode;

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
                oderItem.ShippingCost = shippingPrice;

                // Lưu thông tin mã khuyến mãi vào database khi thanh toán
                var coupon_code = Request.Cookies["CouponTitle"]; // Nhận mã khuyến mãi từ cookie
                oderItem.CouponCode = coupon_code;

                oderItem.UserName = userEmail;
                oderItem.Status = 1;
                oderItem.CreatedDate = DateTime.Now;

                _dataContext.Add(oderItem);
                _dataContext.SaveChanges();

                // Chi tiết đơn hàng
                // lấy dữ liệu giỏ hàng từ session
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cart in cartItems)
                {
                    var orderDetails = new OrderDetails();
                    orderDetails.UserName = userEmail;
                    orderDetails.OrderCode = oderCode;
                    orderDetails.ProductId = cart.ProductId;
                    orderDetails.Price = cart.Price;
                    orderDetails.Quantity = cart.Quantity;

                    // Cập nhật số lượng sản phẩm
                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;
                    _dataContext.Update(product);

                    // Thêm vào chi tiết đơn hàng
                    _dataContext.Add(orderDetails);
                    _dataContext.SaveChanges();
                }    

                HttpContext.Session.Remove("Cart");

                // Gửi email thông báo đặt hàng thành công.
                var receiver = userEmail;
                var subject = "Đặt hàng thành công!";
                var massage = $"Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi. Mã đơn hàng của bạn là: {oderCode}. Chúng tôi sẽ liên hệ với bạn sớm nhất có thể.";
                await _emailSender.SendEmailAsync(receiver, subject, massage);

                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToAction("History", "Accout");
            }
        }
    }
}
