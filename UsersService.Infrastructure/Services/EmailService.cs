using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UsersService.Application.Interfaces;
using UsersService.Infrastructure.Settings;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ISmtpClientWrapper _client;

    public EmailService(EmailSettings settings, ISmtpClientWrapper client)
    {
        _settings = settings;
        _client = client;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = htmlContent };

        await _client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
        await _client.AuthenticateAsync(_settings.Username, _settings.Password);
        await _client.SendAsync(email);
        await _client.DisconnectAsync(true);
    }
}