using Microsoft.AspNetCore.Mvc;

namespace ShoppingCart.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
