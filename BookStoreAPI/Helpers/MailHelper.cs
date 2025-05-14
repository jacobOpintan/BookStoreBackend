using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;

namespace BookStoreAPI.Helpers
{
    public class MailHelper

    {
        private readonly IConfiguration _config;

        public MailHelper(IConfiguration config)
        {
            _config = config;
        }
        public  async Task SendEmailAsync (string toEmail,string subject,string body){
            


            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("no-reply@bookstore.com")); // your Mailtrap sender
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            
            await smtp.ConnectAsync("sandbox.smtp.mailtrap.io", 2525, false);
            // ✅ Pull from secrets (user-secrets or appsettings)
            var username = _config["MailSettings:Username"];
            var password = _config["MailSettings:Password"];

            await smtp.AuthenticateAsync(username, password);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            }
        
    }
}