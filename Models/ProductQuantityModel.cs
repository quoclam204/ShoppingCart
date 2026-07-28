using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCart.Models
{
    public class ProductQuantityModel
    {
        [Key]
        public int Id { get; set; }

        // Số lượng khi nhập vào kho
        [Required(ErrorMessage = "Yêu cầu không được bỏ trống số lượng sản phẩm.")]
        public int Quantity { get; set; }

        public int ProductId { get; set; }

        public DateTime DateCreated { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
