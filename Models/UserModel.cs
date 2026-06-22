using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; }

        // EmailAddress: kiểm tra có đụng định dạng Email không
        [Required(ErrorMessage = "Vui lòng nhập Email"), EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)] // che mật khẩu khi nhập -> ***
        public string Password { get; set; }

    }
}
