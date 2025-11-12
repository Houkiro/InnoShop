using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                emailSettings["FromName"],
                emailSettings["From"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlContent };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                emailSettings["SmtpServer"],
                int.Parse(emailSettings["Port"]),
                false);

            await client.AuthenticateAsync(
                emailSettings["Username"],
                emailSettings["Password"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}