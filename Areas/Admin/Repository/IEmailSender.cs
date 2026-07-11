namespace ShoppingCart.Areas.Admin.Repository
{
    public interface IEmailSender
    {
        // Hàm gửi email
        Task SendEmailAsync(string email, string subject, string message);
    }
}
