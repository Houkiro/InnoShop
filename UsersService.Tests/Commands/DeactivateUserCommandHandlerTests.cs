using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.DeactivateUser;
using UsersService.Domain.Entities;

namespace UsersService.Tests.CommandHandlers
{
    public class DeactivateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IProductServiceClient> _productServiceMock;
        private readonly DeactivateUserCommandHandler _handler;

        public DeactivateUserCommandHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _productServiceMock = new Mock<IProductServiceClient>();

            _handler = new DeactivateUserCommandHandler(
                _userRepoMock.Object,
                _uowMock.Object,
                _productServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ThrowsKeyNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var command = new DeactivateUserCommand(userId);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeactivatesUser_AndCallsDependencies()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, IsActive = true };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new DeactivateUserCommand(userId);

            await _handler.Handle(command, CancellationToken.None);

            Assert.False(user.IsActive);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _productServiceMock.Verify(p => p.HideProductsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_CatchesException_WhenProductServiceFails()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, IsActive = true };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _productServiceMock.Setup(p => p.HideProductsByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Service error"));

            var command = new DeactivateUserCommand(userId);

            await _handler.Handle(command, CancellationToken.None);

            Assert.False(user.IsActive);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _productServiceMock.Verify(p => p.HideProductsByUserIdAsync(userId), Times.Once);
        }
    }
}
