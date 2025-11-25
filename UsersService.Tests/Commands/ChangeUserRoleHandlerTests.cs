using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ChangeUserRole;
using UsersService.Domain.Entities;

namespace UsersService.Tests.CommandHandlers
{
    public class ChangeUserRoleHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly ChangeUserRoleHandler _handler;

        public ChangeUserRoleHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _uowMock = new Mock<IUnitOfWork>();

            _handler = new ChangeUserRoleHandler(
                _userRepoMock.Object,
                _currentUserMock.Object,
                _uowMock.Object
            );
        }

        [Fact]
        public async Task Handle_ThrowsUnauthorized_WhenCurrentUserIsNotAdmin()
        {
            _currentUserMock.Setup(c => c.Role).Returns("User");
            var command = new ChangeUserRoleCommand(Guid.NewGuid(), "Manager");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsKeyNotFound_WhenTargetUserDoesNotExist()
        {
            _currentUserMock.Setup(c => c.Role).Returns("Admin");
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var command = new ChangeUserRoleCommand(userId, "Manager");

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdatesRole_WhenValid()
        {
            _currentUserMock.Setup(c => c.Role).Returns("Admin");
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Role = "User" };

            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new ChangeUserRoleCommand(userId, "Manager");

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("Manager", user.Role);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}