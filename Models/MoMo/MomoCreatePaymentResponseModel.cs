namespace ShoppingCart.Models.MoMo
{
    public class MomoCreatePaymentResponseModel
    {
        // Mã yêu cầu do MoMo trả về
        public string RequestId { get; set; }

        // Mã trạng thái V1 (0 = thành công)
        public int ErrorCode { get; set; }

        // Mã trạng thái V2 (0 = thành công)
        public int ResultCode { get; set; }

        // Mã đơn hàng
        public string OrderId { get; set; }

        // Thông báo từ MoMo
        public string Message { get; set; }

        // Thông báo tiếng Việt
        public string LocalMessage { get; set; }

        // Loại thanh toán
        public string RequestType { get; set; }

        // Link chuyển người dùng đến trang thanh toán MoMo
        public string PayUrl { get; set; }

        // Chữ ký xác thực dữ liệu
        public string Signature { get; set; }

        // Link mã QR thanh toán
        public string QrCodeUrl { get; set; }

        // Deep Link mở ứng dụng MoMo
        public string Deeplink { get; set; }

        // Deep Link cho trình duyệt trong ứng dụng
        public string DeeplinkWebInApp { get; set; }
    }
}
