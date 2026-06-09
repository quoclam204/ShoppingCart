namespace ShoppingCart.Models.ViewModels
{
    public class CartItemViewModel
    {
        // Danh sách các sản phẩm trong giỏ hàng
        public List<CartItemModel> CartItems { get; set; }

        // Tổng tiền
        public decimal GrandTotal { get; set; }
    }
}
