using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using ShoppingCart.Services.MoMo;

namespace ShoppingCart.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;

        public PaymentController(IMomoService momoService)
        {
            _momoService = momoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentMomo(model);

            if (response == null)
            {
                TempData["Error"] = "MoMo trả về null.";
                return RedirectToAction("Index", "Cart");
            }

            if (response.ResultCode != 0 || response.ErrorCode != 0 || string.IsNullOrWhiteSpace(response.PayUrl))
            {
                var code = response.ResultCode != 0 ? response.ResultCode : response.ErrorCode;
                TempData["Error"] = $"MoMo lỗi: {code} - {response.Message}";
                return RedirectToAction("Index", "Cart");
            }

            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallback()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }
    }
}
