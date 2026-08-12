namespace ShoppingCart.Models.MoMo
{
    public class MomoExecuteResponseModel
    {
        // Mã đơn hàng
        public string OrderId { get; set; }

        // Số tiền thanh toán
        public string Amount { get; set; }

        // Thông tin đơn hàng
        public string OrderInfo { get; set; }

        public string FullName { get; set; }    
    }
}
