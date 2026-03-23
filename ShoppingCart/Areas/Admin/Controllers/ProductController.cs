using Microsoft.AspNetCore.Mvc;

namespace ShoppingCart.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
