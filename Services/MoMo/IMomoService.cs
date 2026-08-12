using ShoppingCart.Models;
using ShoppingCart.Models.MoMo;

namespace ShoppingCart.Services.MoMo
{
    public interface IMomoService // Bản thiết kế
    {
        // Tạo yêu cầu thanh toán MoMo
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);

        // Xử lý kết quả MoMo trả về sau thanh toán
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
