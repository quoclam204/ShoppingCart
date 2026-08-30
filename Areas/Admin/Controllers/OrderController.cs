using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
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

        // Chi tiet don hang
        [HttpGet]
        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            // Lấy danh sách sản phẩm đơn hàng
            var detailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode).ToListAsync();

            // Lấy phí vận chuyển
            var order = _dataContext.Orders.Where(o => o.OrderCode == ordercode).First();
            ViewBag.ShippingCost = order.ShippingCost;   

            ViewBag.Status = order.Status; // Trạng thái đơn hàng

            return View(detailsOrder);
        }

        [HttpGet]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }

            // Thực hiện update trạng thái
            order.Status = status;
            _dataContext.Update(order);

            if (status == 0)
            {
                // lấy danh sách chi tiết đơn hàng để cập nhật số lượng sản phẩm dựa vào ordercode
                var detailsOrder = await _dataContext.OrderDetails
                    .Include(od => od.Product)
                    .Where(od => od.OrderCode == order.OrderCode)
                    .Select(od => new
                    {
                        od.Quantity,
                        od.Product.Price,
                        od.Product.CapitalPrice
                    }).ToListAsync();

                // lấy dữ liệu thống kê dựa vào ngày đặt hàng
                var statistical = await _dataContext.Statisticals
                    .FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreatedDate.Date);

                if (statistical != null)
                {
                    foreach (var orderDetail in detailsOrder)
                    {
                        statistical.Quantity += 1; // đơn hàng đã đặt
                        statistical.Sold += orderDetail.Quantity; // số lượng bán
                        statistical.Revenue += orderDetail.Quantity * orderDetail.Price; // doanh thu
                        statistical.Profit += orderDetail.Price - orderDetail.CapitalPrice; // lợi nhuận
                    }
                    _dataContext.Update(statistical);
                }
                else
                {
                    int new_quatity = 0;
                    int new_sold = 0;   
                    decimal new_profit = 0;

                    foreach (var orderDetail in detailsOrder)
                    {
                        new_quatity += 1; // đơn hàng đã đặt
                        new_sold += orderDetail.Quantity; // số lượng bán
                        new_profit += orderDetail.Price - orderDetail.CapitalPrice; // lợi nhuận

                        statistical = new StatisticalModel
                        {
                            DateCreated = order.CreatedDate.Date,
                            Quantity = new_quatity,
                            Sold = new_sold,
                            Revenue = orderDetail.Quantity * orderDetail.Price,
                            Profit = new_profit
                        };
                        
                        _dataContext.Add(statistical);
                    }
                }    
            }    

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật trạng thái đơn hàng thành công." });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) if needed
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng." });
            }
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }
            // Xóa các chi tiết đơn hàng liên quan trước
            var orderDetails = _dataContext.OrderDetails.Where(od => od.OrderCode == ordercode);
            _dataContext.OrderDetails.RemoveRange(orderDetails);
            // Xóa đơn hàng
            _dataContext.Orders.Remove(order);
            try
            {
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Xóa đơn hàng thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) if needed
                return StatusCode(500, "Đã xảy ra lỗi khi xóa đơn hàng.");
            }
        }
    }
}
