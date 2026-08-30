using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    //[Authorize(Roles = "Publisher, Author, Admin")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;

        // chỉ đến thư mục lưu ảnh trong server -> dùng để upload file ảnh
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DashboardController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();

            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;

            return View();
        }

        // Lấy tất cả dữ liệu để hiển thị trên biểu đồ (lúc đầu khi vô trang)
        [HttpPost]
        [Route("GetChartData")]
        public async Task<IActionResult> GetChartData()
        {
            var data = _dataContext.Statisticals.Select(s => new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit,
            }).ToList();

            return Json(data);
        }

        // Lấy dữ liệu theo dropdown
        [HttpPost]
        [Route("GetChartDataBySelect")]
        public async Task<IActionResult> GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            var data = _dataContext.Statisticals
                // Lọc dữ liệu theo khoảng thời gian được chọn (khoảng giữa)
                .Where(s => s.DateCreated >= startDate && s.DateCreated <= endDate)
                .Select(s => new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit,
            }).ToList();

            return Json(data);
        }

        // Lọc theo người dùng chọn ngày (từ ngày -> đến ngày)
        [HttpPost]
        [Route("FilterData")]
        public async Task<IActionResult> FilterData(DateTime? fromDate, DateTime? toDate)
        {
            // chuẩn bị truy vấn và chưa lấy dữ liệu
            var query = _dataContext.Statisticals.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.DateCreated >= fromDate);
            }

            if (toDate.HasValue)
            {
                query = query.Where(s => s.DateCreated <= toDate);
            }

            var data = query.Select(s => new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit,
            }).ToList();

            return Json(data);
        }
    }
}
