using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ConfirmUser;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;

namespace UsersService.Tests.CommandHandlers
{
    public class ConfirmUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly ConfirmUserCommandHandler _handler;

        public ConfirmUserCommandHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _handler = new ConfirmUserCommandHandler(_repoMock.Object, _uowMock.Object);
        }

        [Fact]
        public async Task Handle_ThrowsBadRequest_WhenUserNotFound()
        {
            _repoMock.Setup(r => r.GetByConfirmationTokenAsync("bad-token"))
                .ReturnsAsync((User?)null);

            var command = new ConfirmUserCommand("bad-token");

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ReturnsTrue_WhenAlreadyConfirmed()
        {
            var user = new User { Id = Guid.NewGuid(), IsEmailConfirmed = true };
            _repoMock.Setup(r => r.GetByConfirmationTokenAsync("token"))
                .ReturnsAsync(user);

            var command = new ConfirmUserCommand("token");
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsBadRequest_WhenTokenExpired()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                IsEmailConfirmed = false,
                ConfirmationTokenExpires = DateTime.UtcNow.AddMinutes(-1)
            };
            _repoMock.Setup(r => r.GetByConfirmationTokenAsync("expired-token"))
                .ReturnsAsync(user);

            var command = new ConfirmUserCommand("expired-token");

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ConfirmsUser_WhenValid()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                IsEmailConfirmed = false,
                IsActive = false,
                ConfirmationToken = "valid-token",
                ConfirmationTokenExpires = DateTime.UtcNow.AddMinutes(10)
            };

            _repoMock.Setup(r => r.GetByConfirmationTokenAsync("valid-token"))
                .ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new ConfirmUserCommand("valid-token");
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            Assert.True(user.IsActive);
            Assert.True(user.IsEmailConfirmed);
            Assert.Null(user.ConfirmationToken);
            Assert.Null(user.ConfirmationTokenExpires);

            _repoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}