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
    }
}
