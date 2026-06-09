using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Tên Danh Mục")]
        public string Name { get; set; }

        [Required,MinLength(4,ErrorMessage ="Yêu cầu nhập Mô tả Danh Mục")]
        public string Description { get; set; }

        [Required]
        public string Slug { get; set; }

        public int Status { get; set; }

    }
}
