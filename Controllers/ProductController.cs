using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShoppingCart.Models;
using ShoppingCart.Models.ViewModels;
using ShoppingCart.Repository;

namespace ShoppingCart.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
                return RedirectToAction("Index");

            var productsById = _dataContext.Products
                .Include(p => p.Ratings)
                .Where(p => p.Id == Id).FirstOrDefault();

            // Tìm sản phẩm liên quan với categoryId của sản phẩm hiện tại
            // && trừ đi sản phẩm khi đã ấn vào xem chi tiết
            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
                .Take(4) // lấy ra 4 sản phẩm liên quan
                .ToListAsync();

            // sau đó đẩy dữ liệu tìm ra vào ViewBag để hiện lên view
            ViewBag.RelatedProducts = relatedProducts;

            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productsById,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();

            // Hiển thị từ khóa đã tìm kiếm lên view
            ViewBag.Keyword = searchTerm;

            return View(products);
        }

        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                var ratingEntity = new RatingModel
                {
                    ProductId = rating.ProductId,
                    Name = rating.Name,
                    Email = rating.Email,
                    Comment = rating.Comment,
                    Star = rating.Star  
                };

                _dataContext.Ratings.Add(ratingEntity);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm đánh giá thành công.";

                // Chuyển hướng về trang mà người dùng vừa gửi request từ đó
                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                TempData["error"] = "Thông tin bạn nhập chưa hợp lệ. Vui lòng kiểm tra lại.";

                // lấy lỗi chi tiết
                List<string> errors = new List<string>();
                // ModelState.Values → tất cả field (Name, Price,…)
                foreach (var value in ModelState.Values)
                {
                    // value.Errors → lỗi của từng field
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);

                // Chuyển hướng đến trang chi tiết của sản phẩm vừa được đánh giá
                return RedirectToAction("Detail", new {id = rating.ProductId });
            }

            return Redirect(Request.Headers["Referer"]);
        }
    }
}
