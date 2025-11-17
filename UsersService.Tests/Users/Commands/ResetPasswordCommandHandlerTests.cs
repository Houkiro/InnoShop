using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ResetPassword;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;
using MediatR;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UsersService.Tests.Users.Commands
{
    public class ResetPasswordCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldResetPassword_WhenTokenIsValid()
        {
            // Arrange
            var repoMock = new Mock<IUserRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = "old_hash",
                PasswordResetToken = "valid_token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(10)
            };

            repoMock
                .Setup(x => x.GetByResetPasswordTokenAsync("valid_token"))
                .ReturnsAsync(user);

            repoMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(0);

            var command = new ResetPasswordCommand("valid_token", "new_password");

            var handler = new ResetPasswordCommandHandler(repoMock.Object, uowMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Unit.Value, result);
            Assert.True(BCrypt.Net.BCrypt.Verify("new_password", user.PasswordHash));
            Assert.Null(user.PasswordResetToken);
            Assert.Null(user.PasswordResetTokenExpires);

            repoMock.Verify(x => x.UpdateAsync(user), Times.Once);
            uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenTokenIsInvalid()
        {
            // Arrange
            var repoMock = new Mock<IUserRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            repoMock
                .Setup(x => x.GetByResetPasswordTokenAsync("invalid_token"))
                .ReturnsAsync((User?)null);

            var command = new ResetPasswordCommand("invalid_token", "new_password");

            var handler = new ResetPasswordCommandHandler(repoMock.Object, uowMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenTokenIsExpired()
        {
            // Arrange
            var repoMock = new Mock<IUserRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                PasswordResetToken = "expired_token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(-10)
            };

            repoMock
                .Setup(x => x.GetByResetPasswordTokenAsync("expired_token"))
                .ReturnsAsync(user);

            var command = new ResetPasswordCommand("expired_token", "new_password");

            var handler = new ResetPasswordCommandHandler(repoMock.Object, uowMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}