namespace ShoppingCart.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public decimal ShippingCost { get; set; } // Phí vận chuyển của đơn hàng.
        public string CouponCode { get; set; } // Mã giảm giá áp dụng cho đơn hàng.
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
    }
}
