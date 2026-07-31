using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price)
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
    }
}
