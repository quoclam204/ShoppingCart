using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using ShoppingCart.Models;
using ShoppingCart.Models.MoMo;
using System.Security.Cryptography;
using System.Text;

namespace ShoppingCart.Services.MoMo
{
    public class MomoService : IMomoService // Công việc cụ thể dựa vào bản thiết kế đó
    {
        // biến lấy cấu hình MoMo từ appsettings.json.
        private readonly IOptions<MomoOptionModel> _momoOptions;

        public MomoService(IOptions<MomoOptionModel> momoOptions)
        {
            _momoOptions = momoOptions;
        }

        // Thanh toán bằng MoMo
        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model)
        {
            model.OrderId = DateTime.Now.Ticks.ToString(); // Tạo orderId dựa vào thời gian hiện tại
            model.Orderinfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.Orderinfo; // Thêm thông tin khách hàng vào nội dung đơn hàng

            // Dữ liệu cần lưu và mã hóa
            var rawData =
                $"partnerCode={_momoOptions.Value.PartnerCode}" +
                $"&accessKey={_momoOptions.Value.AccessKey}" +
                $"&requestId={model.OrderId}" +
                $"&amount={model.Amount}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.Orderinfo}" +
                $"&returnUrl={_momoOptions.Value.ReturnUrl}" +
                $"&notifyUrl={_momoOptions.Value.NotifyUrl}" +
                $"&extraData=";

            // tạo chữ ký
            var signature = ComputeHmacSha256(rawData, _momoOptions.Value.SecretKey);

            // 1. Tạo client (địa chỉ server MoMo hoặc đường dẫn)
            var client = new RestClient(_momoOptions.Value.MomoApiUrl);

            // 2. Tạo HTTP Request (chưa gửi)
            var request = new RestRequest() { Method = Method.Post };

            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            // gom dữ liệu thành 1 object
            var requestData = new
            {
                accessKey = _momoOptions.Value.AccessKey,
                partnerCode = _momoOptions.Value.PartnerCode,
                requestType = _momoOptions.Value.RequestType,
                notifyUrl = _momoOptions.Value.NotifyUrl,
                returnUrl = _momoOptions.Value.ReturnUrl,
                orderId = model.OrderId,
                amount = model.Amount.ToString(),
                orderInfo = model.Orderinfo,
                requestId = model.OrderId,
                extraData = "",
                signature = signature
            };

            // Chuyển dữ liệu thanh toán thành JSON và gửi trong phần Body của request đến MoMo
            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content); 
        }

        // xử lý dữ liệu mà MoMo trả về sau khi khách hàng thanh toán xong.
        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            // Lấy số tiền
            var amount = collection.First(s => s.Key == "amount").Value;

            // Lấy thông tin đơn hàng
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;

            // Lấy mã đơn hàng
            var orderId = collection.First(s => s.Key == "orderId").Value;

            // Trả về model chứa kết quả thanh toán
            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
            };
        }

        // Hàm mã hóa
        private string ComputeHmacSha256(string message, string secretKey)
        {
            // Chuyển SecretKey thành mảng byte
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            // Chuyển dữ liệu cần mã hóa thành mảng byte
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            // Tạo chữ ký bằng thuật toán HMAC SHA256
            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            // Chuyển kết quả thành chuỗi Hex
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Trả về chữ ký
            return hashString;
        }
    }
}
