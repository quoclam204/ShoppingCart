namespace ShoppingCart.Models.MoMo
{
    // Đọc cấu hình từ appsettings.json.
    public class MomoOptionModel
    {
        // URL API của MoMo dùng để gửi yêu cầu thanh toán
        public string MomoApiUrl { get; set; }

        // Khóa bí mật (Secret Key) do MoMo cấp để tạo chữ ký (Signature)
        // Giúp xác thực và bảo mật dữ liệu gửi đến MoMo
        public string SecretKey { get; set; }

        // Khóa truy cập (Access Key) do MoMo cấp để nhận diện Merchant
        public string AccessKey { get; set; }

        // URL mà MoMo sẽ chuyển hướng người dùng về sau khi thanh toán xong
        // (Thanh toán thành công hoặc thất bại)
        public string ReturnUrl { get; set; }

        // URL để MoMo gửi thông báo kết quả thanh toán đến server
        // (Webhook/Callback, chạy ngầm, người dùng không nhìn thấy)
        public string NotifyUrl { get; set; }

        // Mã đối tác (Merchant/Partner Code) do MoMo cấp
        public string PartnerCode { get; set; }

        // Loại yêu cầu thanh toán
        // Ví dụ:
        // - captureMoMoWallet : Thanh toán bằng ví MoMo
        // - payWithATM        : Thanh toán qua thẻ ATM
        public string RequestType { get; set; }
    }
}
