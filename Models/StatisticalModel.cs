namespace ShoppingCart.Models
{
    public class StatisticalModel
    {
        public int Id { get; set; }

        public int Quantity { get; set; } // số lượng bán

        public int Sold { get; set; } // số lượng đơn hàng

        public decimal Revenue { get; set; } // doanh thu

        public decimal Profit { get; set; } // lợi nhuận

        public DateTime DateCreated { get; set; } // ngày đặt hàng
    }
}
