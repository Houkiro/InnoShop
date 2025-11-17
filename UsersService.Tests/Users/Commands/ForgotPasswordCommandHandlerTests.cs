using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ForgotPassword;
using UsersService.Domain.Entities;

namespace UsersService.Tests.Users.Commands
{
    public class ForgotPasswordCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldGenerateResetToken_AndSendEmail()
        {
            // Arrange
            var repoMock = new Mock<IUserRepository>();
            var emailMock = new Mock<IEmailService>();
            var uowMock = new Mock<IUnitOfWork>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "test@test.com",
                PasswordHash = "hash",
                IsActive = true
            };

            repoMock
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            repoMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(0);

            emailMock
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var handler = new ForgotPasswordCommandHandler(
                repoMock.Object,
                emailMock.Object,
                uowMock.Object
            );

            var command = new ForgotPasswordCommand(user.Email!);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(user.PasswordResetToken);
            Assert.NotEqual("", user.PasswordResetToken);

            Assert.True(user.PasswordResetTokenExpires > DateTime.UtcNow);

            repoMock.Verify(x => x.UpdateAsync(user), Times.Once);
            uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            emailMock.Verify(x => x.SendEmailAsync(
                user.Email,
                "Сброс пароля",
                It.Is<string>(html => html.Contains(user.PasswordResetToken!))
            ), Times.Once);
        }
    }
}