using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace BookStoreAPI.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.Gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("Abdoelmoshy20147@gmail.com", "ncnb sako vgxt cgpr")
            };
            return client.SendMailAsync(
        new MailMessage(from: "Abdoelmoshy20147@gmail.com",
                        to: email,
                        subject,
                        htmlMessage
                        )
        {
            IsBodyHtml = true
        });
        }
    }
}
