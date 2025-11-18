using MailKit.Security;
using MimeKit;
using Moq;
using UsersService.Application.Interfaces;
using UsersService.Infrastructure.Settings;

namespace UsersService.Tests.Users.Services
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_ShouldCall_AllSmtpMethods()
        {
            // Arrange
            var settings = new EmailSettings
            {
                From = "noreply@example.com",
                FromName = "Test Sender",
                SmtpServer = "smtp.test.com",
                Port = 587,
                Username = "user",
                Password = "password"
            };

            var smtpMock = new Mock<ISmtpClientWrapper>();

            var service = new EmailService(settings, smtpMock.Object);

            // Act
            await service.SendEmailAsync("to@example.com", "Test Subject", "<b>Hello</b>");

            // Assert
            smtpMock.Verify(c => c.ConnectAsync(
                settings.SmtpServer,
                settings.Port,
                SecureSocketOptions.StartTls
            ), Times.Once);

            smtpMock.Verify(c => c.AuthenticateAsync(
                settings.Username,
                settings.Password
            ), Times.Once);

            smtpMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>()), Times.Once);

            smtpMock.Verify(c => c.DisconnectAsync(true), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldBuildCorrectEmailMessage()
        {
            // Arrange
            MimeMessage? capturedMessage = null;

            var settings = new EmailSettings
            {
                From = "sender@example.com",
                FromName = "Sender",
                SmtpServer = "smtp.test.com",
                Port = 587,
                Username = "user",
                Password = "pass"
            };

            var smtpMock = new Mock<ISmtpClientWrapper>();

            smtpMock
                .Setup(c => c.SendAsync(It.IsAny<MimeMessage>()))
                .Callback<MimeMessage>(msg => capturedMessage = msg)
                .Returns(Task.CompletedTask);

            var service = new EmailService(settings, smtpMock.Object);

            // Act
            await service.SendEmailAsync("to@example.com", "Hello", "<h1>Hi</h1>");

            // Assert email contents
            Assert.NotNull(capturedMessage);
            Assert.Equal("Hello", capturedMessage!.Subject);
            Assert.Equal("sender@example.com", capturedMessage.From.Mailboxes.First().Address);
            Assert.Equal("Sender", capturedMessage.From.Mailboxes.First().Name);
            Assert.Equal("to@example.com", capturedMessage.To.Mailboxes.First().Address);

            Assert.Equal("<h1>Hi</h1>", (capturedMessage.Body as TextPart)?.Text);
        }
    }
}