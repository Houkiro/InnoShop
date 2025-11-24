using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtml)
                builder.HtmlBody = body;
            else
                builder.TextBody = body;

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string userName, string confirmationToken)
        {
            var confirmUrl = $"http://localhost:5000/api/users/confirm?token={confirmationToken}";
            var body = $@"
                <p>Привет, {userName}!</p>
                <p>Спасибо за регистрацию в InnoShop.</p>
                <p>Для подтверждения аккаунта перейди по ссылке:</p>
                <p><a href='{confirmUrl}'>Подтвердить аккаунт</a></p>";

            await SendEmailAsync(toEmail, "Подтверждение регистрации", body);
        }

        public async Task SendResetPasswordEmailAsync(string toEmail, string userName, string resetToken)
        {
            var resetUrl = $"http://localhost:5000/api/users/reset-password?token={resetToken}";
            var body = $@"
                <p>Привет, {userName}!</p>
                <p>Ты запросил сброс пароля.</p>
                <p>Для восстановления пароля перейди по ссылке:</p>
                <p><a href='{resetUrl}'>Сбросить пароль</a></p>
                <p>Ссылка действительна в течение 1 часа.</p>";

            await SendEmailAsync(toEmail, "Восстановление пароля", body);
        }
    }
}
