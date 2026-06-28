using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Repository;
using System.Security.Claims;

namespace ShoppingCart.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;

        public CheckoutController(DataContext context)
        {
            _dataContext = context;
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

                    _dataContext.Add(orderDetails);
                    _dataContext.SaveChanges();
                }    

                HttpContext.Session.Remove("Cart"); 
                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}
