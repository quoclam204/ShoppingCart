using Microsoft.AspNetCore.Mvc;
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
                GrandTotal = cartItems.Sum(x => x.Quantity*x.Price)
            };


            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }


        // thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddCartProduct(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

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

            HttpContext.Session.SetJson("Cart", cart);

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
