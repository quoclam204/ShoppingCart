using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;

        public OrderController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index(int page = 1)
        {
            // Lấy ra danh sách sản phẩm trong csdl
            List<OrderModel> order = _dataContext.Orders.ToList();

            const int pageSize = 10; // 10 sản phẩm trên 1 trang

            if (page < 1)
            {
                page = 1;
            }

            int recsCount = order.Count(); // đếm sô lượng sản phẩm

            var pager = new Paginate(recsCount, page, pageSize);

            int recSkip = (page - 1) * pageSize;
            var data = order.Skip(recSkip).Take(pager.PageSize).ToList();

            // đưa đối tượng pager từ Controller sang View để View
            ViewBag.Pager = pager;

            return View(data);

        }

        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var detailsOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode) .ToListAsync();
            return View(detailsOrder);
        }
    }
}
