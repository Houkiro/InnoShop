using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ISmtpClientWrapper _client;

        public EmailService(ISmtpClientWrapper client, IOptions<EmailSettings> settings)
        {
            _client = client;
            _settings = settings.Value;
        }

        private MimeMessage BuildMessage(string toEmail, string subject, string body, bool isHtml)
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
            return message;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var message = BuildMessage(toEmail, subject, body, isHtml);

            var secureOption = _settings.UseTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await _client.ConnectAsync(_settings.SmtpServer, _settings.Port, secureOption);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await _client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await _client.SendAsync(message);
            await _client.DisconnectAsync(true);
        }

        public Task SendConfirmationEmailAsync(string toEmail, string userName, string confirmationToken)
        {
            var confirmUrl = $"{_settings.ConfirmUrl}?token={confirmationToken}";
            var body = $@"
                <p>Привет, {userName}!</p>
                <p>Спасибо за регистрацию в InnoShop.</p>
                <p>Для подтверждения аккаунта перейди по ссылке:</p>
                <p><a href='{confirmUrl}'>Подтвердить аккаунт</a></p>";

            return SendEmailAsync(toEmail, "Подтверждение регистрации", body);
        }

        public Task SendResetPasswordEmailAsync(string toEmail, string userName, string resetToken)
        {
            var resetUrl = $"{_settings.ResetUrl}?token={resetToken}";
            var body = $@"
                <p>Привет, {userName}!</p>
                <p>Ты запросил сброс пароля.</p>
                <p>Для восстановления пароля перейди по ссылке:</p>
                <p><a href='{resetUrl}'>Сбросить пароль</a></p>
                <p>Ссылка действительна в течение 1 часа.</p>";

            return SendEmailAsync(toEmail, "Восстановление пароля", body);
        }
    }
}
