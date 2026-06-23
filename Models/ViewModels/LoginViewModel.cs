using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Models.ViewModels
{
    public class LoginViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)] // che mật khẩu khi nhập -> ***
        public string Password { get; set; }

        public string ReturnUrl { get; set; } // URL để chuyển hướng sau khi đăng nhập thành công
    }
}
