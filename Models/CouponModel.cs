using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên khuyến mãi")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mô tả khuyến mãi")]
        public string Description { get; set; } 

        // Ngày bắt đầu
        public DateTime DateStart { get; set; }

        // Ngày hết hạn
        public DateTime DateExpỉed { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số lượng khuyến mãi")]
        public int Quantity { get; set; }   

        public int Status { get; set; }


    }
}
