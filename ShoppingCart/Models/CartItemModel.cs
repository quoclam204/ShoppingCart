namespace ShoppingCart.Models
{
    public class CartItemModel
    {
        public long ProductId { get; set; }
        
        public string ProductName { get; set; }

        public int Quantity { get; set; }   

        public decimal Price { get; set; }  

        public decimal Total { get { return Quantity * Price; } }
        
        public string Image { get; set; }

        public CartItemModel()
        {
            
        }

        // lấy dữ liệu từ sản phẩm gán vào giỏ hàng
        //ProductModel = sản phẩm trong shop(VD: iPhone)
        //CartItemModel = sản phẩm khi đã được thêm vào giỏ
        public CartItemModel(ProductModel product)
        {
            ProductId = product.Id; 
            ProductName = product.Name;
            Price = product.Price;
            Quantity = 1;   
            Image = product.Image;
        }
    }
}
