using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace BookStoreAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        //constructor to inject configuration settings
        public EmailService (IConfiguration config){
            _configuration = config;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["Email:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text){Text=body};

           /** using (var smtp = new SmtpClient())
            {
                smtp.Connect(_configuration["Email:Host"], int.Parse(_configuration["Email:Port"]), true);
                smtp.Authenticate(_configuration["Email:Username"], _configuration["Email:Password"]);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }**/
            // you can uncomment the above code to send an email

            // code to send an email
            using (var smtp= new SmtpClient()){
                await smtp.ConnectAsync(_configuration["MailSettings:SmtpServer"],int.Parse(_configuration["MailSettings:Port"]),MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_configuration["MailSettings:Username"],_configuration["MailSettingsPassword"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
        }
        
    }
}