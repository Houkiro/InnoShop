using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ResetPassword;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;
using Xunit;

namespace UsersService.Tests.CommandHandlers
{
    public class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly ResetPasswordCommandHandler _handler;

        public ResetPasswordCommandHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _handler = new ResetPasswordCommandHandler(_repoMock.Object, _uowMock.Object);
        }

        [Fact]
        public async Task Handle_ThrowsBadRequest_WhenUserNotFound()
        {
            _repoMock.Setup(r => r.GetByResetPasswordTokenAsync("bad-token"))
                .ReturnsAsync((User?)null);

            var command = new ResetPasswordCommand("bad-token", "newpwd");

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsBadRequest_WhenTokenExpired()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                PasswordResetToken = "expired-token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(-1)
            };

            _repoMock.Setup(r => r.GetByResetPasswordTokenAsync("expired-token"))
                .ReturnsAsync(user);

            var command = new ResetPasswordCommand("expired-token", "newpwd");

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ResetsPassword_WhenValid()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                PasswordResetToken = "valid-token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(10),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpwd")
            };

            _repoMock.Setup(r => r.GetByResetPasswordTokenAsync("valid-token"))
                .ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new ResetPasswordCommand("valid-token", "newpwd");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            Assert.True(BCrypt.Net.BCrypt.Verify("newpwd", user.PasswordHash));
            Assert.Null(user.PasswordResetToken);
            Assert.Null(user.PasswordResetTokenExpires);

            _repoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}