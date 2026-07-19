using ShoppingCart.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCart.Models
{
    public class SliderModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu không được bỏ trống tên slider")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu không được bỏ trống mô tả")]
        public string Description { get; set; }

        public int? Status { get; set; }

        public string Image { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpLoad { get; set; }
    }
}
