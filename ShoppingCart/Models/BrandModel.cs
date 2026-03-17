using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Models
{
    public class BrandModel
    {
        [Key]
        public int Id { get; set; }

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Tên Thương Hiệu")]
        public string Name { get; set; }

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Mô Tả Thương Hiệu")]
        public string Description { get; set; }

        public string Slug { get; set; }

        public int Status { get; set; }

    }
}
