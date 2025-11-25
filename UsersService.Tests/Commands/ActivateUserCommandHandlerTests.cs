using Microsoft.Extensions.Logging;
using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ActivateUser;
using UsersService.Domain.Entities;

namespace UsersService.Tests.CommandHandlers
{
    public class ActivateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IProductServiceClient> _productServiceMock;
        private readonly Mock<ILogger<ActivateUserCommandHandler>> _loggerMock;
        private readonly ActivateUserCommandHandler _handler;

        public ActivateUserCommandHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _productServiceMock = new Mock<IProductServiceClient>();
            _loggerMock = new Mock<ILogger<ActivateUserCommandHandler>>();

            _handler = new ActivateUserCommandHandler(
                _userRepoMock.Object,
                _uowMock.Object,
                _productServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Handle_ActivatesUser_AndCallsDependencies()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, IsActive = false };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new ActivateUserCommand(userId);

            await _handler.Handle(command, CancellationToken.None);

            Assert.True(user.IsActive);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _productServiceMock.Verify(p => p.RestoreProductsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_Throws_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var command = new ActivateUserCommand(userId);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_LogsError_WhenProductServiceFails()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, IsActive = false };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _productServiceMock.Setup(p => p.RestoreProductsByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Service error"));

            var command = new ActivateUserCommand(userId);

            await _handler.Handle(command, CancellationToken.None);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error restoring products")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.True(user.IsActive);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
