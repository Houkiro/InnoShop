using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using UsersService.Application.Interfaces;
using UsersService.Infrastructure.Services;

public class EmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_CallsSmtpClientWrapperMethods_WhenUseTlsIsFalse()
    {
        var smtpMock = new Mock<ISmtpClientWrapper>();
        var settings = new EmailSettings
        {
            FromName = "TestSender",
            From = "sender@test.com",
            SmtpServer = "smtp.test.com",
            Port = 25,
            Username = "user",
            Password = "pwd",
            UseTls = false
        };

        var options = Options.Create(settings);
        var service = new EmailService(smtpMock.Object, options);

        await service.SendEmailAsync("recipient@test.com", "Subject", "<p>Hello</p>");

        smtpMock.Verify(c => c.ConnectAsync(settings.SmtpServer, settings.Port, SecureSocketOptions.None), Times.Once);
        smtpMock.Verify(c => c.AuthenticateAsync(settings.Username, settings.Password), Times.Once);
        smtpMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>()), Times.Once);
        smtpMock.Verify(c => c.DisconnectAsync(true), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_CallsSmtpClientWrapperMethods_WhenUseTlsIsTrue()
    {
        var smtpMock = new Mock<ISmtpClientWrapper>();
        var settings = new EmailSettings
        {
            FromName = "TestSender",
            From = "sender@test.com",
            SmtpServer = "smtp.test.com",
            Port = 587,
            Username = "user",
            Password = "pwd",
            UseTls = true
        };

        var options = Options.Create(settings);
        var service = new EmailService(smtpMock.Object, options);

        await service.SendEmailAsync("recipient@test.com", "Subject", "<p>Hello</p>");

        smtpMock.Verify(c => c.ConnectAsync(settings.SmtpServer, settings.Port, SecureSocketOptions.StartTls), Times.Once);
        smtpMock.Verify(c => c.AuthenticateAsync(settings.Username, settings.Password), Times.Once);
        smtpMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>()), Times.Once);
        smtpMock.Verify(c => c.DisconnectAsync(true), Times.Once);
    }
}
