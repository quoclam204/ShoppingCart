using System.Net;
using System.Net.Mail;

namespace ShoppingCart.Areas.Admin.Repository
{
    // dịch vụ gửi email của hệ thống
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, // bật bảo mật
                UseDefaultCredentials = false,

                // Khai báo tài khoản Gmail dùng để gửi email.
                Credentials = new NetworkCredential("nguyenlequoclam@gmail.com", "rjpiykevxogjtbxh")
            };

            // Gửi email
            return client.SendMailAsync(
                new MailMessage(from: "nguyenlequoclam@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
