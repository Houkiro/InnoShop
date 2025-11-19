using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Services
{
    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        private readonly SmtpClient _client = new();

        public Task ConnectAsync(string host, int port, SecureSocketOptions options)
            => _client.ConnectAsync(host, port, options);

        public Task AuthenticateAsync(string username, string password)
            => _client.AuthenticateAsync(username, password);

        public Task SendAsync(MimeMessage message)
            => _client.SendAsync(message);

        public Task DisconnectAsync(bool quit)
            => _client.DisconnectAsync(quit);

        public void Dispose() => _client.Dispose();
    }
}