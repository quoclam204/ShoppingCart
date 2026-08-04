using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using ShoppingCart.Repository;

namespace ShoppingCart.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        // Lấy danh sách sản phẩm theo Category (dựa vào Slug) và hiển thị ra View
        public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            CategoryModel category = _dataContext.Categories.Where(c => c.Slug == Slug).FirstOrDefault();

            if (category == null)
            {
                return RedirectToAction("Index");
            }

            // Lấy tất cả sản phẩm theo danh mục(category)
            // IQueryable: tạo truy vấn dữ liệu nhưng chưa thực thi (chưa lấy dữ liệu từ Database)
            IQueryable<ProductModel> productsByCategory = _dataContext.Products.Where(p => p.CategoryId == category.Id);

            var cout = await productsByCategory.CountAsync();
            if (cout > 0)
            {
                /*
                 * Id càng lớn → sản phẩm được thêm càng mới.
                 * Id càng nhỏ → sản phẩm được thêm càng cũ.
                 */

                if (sort_by == "price_increase") // Giá tăng dần
                {
                    productsByCategory = productsByCategory.OrderBy(p => p.Price);
                }
                else if (sort_by == "price_decrease") // Giá giảm dần
                {
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
                }
                else if (sort_by == "price_newest") // Sản phẩm mới nhất (Id lớn nhất trước).
                {
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                }
                else if (sort_by == "price_oldest") // Sản phẩm cũ nhất (Id nhỏ nhất trước).        
                {
                    productsByCategory = productsByCategory.OrderBy(p => p.Id);
                }
                // Lọc giá sản phẩm
                else if (startprice != "" && endprice != "")
                {
                    decimal startPriceValue;
                    decimal endPriceValue;

                    // Chuyển đổi kiểu dữ liệu của giá từ string sang decimal
                    if (decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endprice, out endPriceValue))
                    {
                        // Lọc theo giá trong khoảng...
                        productsByCategory = productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    }
                    else
                    {
                        // Nếu ko lọc giá thì sắp xếp theo Id giảm dần
                        productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                    }    
                }
                // Nếu ko lọc   
                else
                {
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                }    
            }

            return View(await productsByCategory.ToListAsync());
        }
    }
}
