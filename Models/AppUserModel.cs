using Microsoft.AspNetCore.Identity;

namespace ShoppingCart.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Occupation { get; set; }
        public string RoleId { get; set; }

        // Là chuỗi ký tự dùng để xác thực và nhận diện người dùng
        public string Token { get; set; }
    }
}
