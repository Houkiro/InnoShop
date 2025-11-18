using MailKit.Security;
using MimeKit;

namespace UsersService.Application.Interfaces
{
    public interface ISmtpClientWrapper : IDisposable
    {
        Task ConnectAsync(string host, int port, SecureSocketOptions options);
        Task AuthenticateAsync(string username, string password);
        Task SendAsync(MimeMessage message);
        Task DisconnectAsync(bool quit);
    }
}