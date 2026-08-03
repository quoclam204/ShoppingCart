using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    //[Authorize(Roles = "Publisher, Author, Admin")]

    public class ShippingController : Controller
    {
        private readonly DataContext _dataContext;

        public ShippingController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var shippinglist = await _dataContext.Shippings.ToListAsync();
            ViewBag.Shippings = shippinglist;

            return View();
        }

        // Hàm xử lý giá vận chuyển đến nơi cần ship
        [HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string phuong, string quan, string tinh, decimal price)
        {
            // gán dữ liệu người dùng nhập lấy từ ajax và lưu vào database
            shippingModel.City = tinh;
            shippingModel.District = quan;
            shippingModel.Ward = phuong;
            shippingModel.Price = price;

            try
            {
                var existingShipping = await _dataContext.Shippings
                    .AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

                // Kiểm tra có dữ liệu chưa
                if (existingShipping)
                {
                    return Ok(new { duplicate = true, message = "Dữ liệu trùng lặp." });
                }

                _dataContext.Shippings.Add(shippingModel);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm shipping thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while storing the shipping information.");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            ShippingModel shipping = await _dataContext.Shippings.FindAsync(id);

            _dataContext.Shippings.Remove(shipping);
            await _dataContext.SaveChangesAsync();
            TempData["Success"] = "Xóa thành công!";
            return RedirectToAction("Index", "Shipping");
        }
    }
}
