
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace ECommerce_API_2.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("veno1udemy@gmail.com", "fpsc itfe fjih ceai")
            };

            return client.SendMailAsync(
                new MailMessage (from: "veno1udemy@gmail.com"
                , to: email
                , subject
                , htmlMessage)
                {
                    IsBodyHtml = true
                });   
        }
    }
}
