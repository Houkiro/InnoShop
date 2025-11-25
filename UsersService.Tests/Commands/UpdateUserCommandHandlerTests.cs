using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.UpdateUserCommand;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;
using Xunit;

namespace UsersService.Tests.CommandHandlers
{
    public class UpdateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly UpdateUserCommandHandler _handler;

        public UpdateUserCommandHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _uowMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUserService>();

            _handler = new UpdateUserCommandHandler(
                _repoMock.Object,
                _uowMock.Object,
                _currentUserMock.Object
            );
        }

        [Fact]
        public async Task Handle_ThrowsNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _currentUserMock.Setup(c => c.UserId).Returns(userId);
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var command = new UpdateUserCommand { Email = "new@test.com", Name = "NewName" };

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdatesEmailAndName_WhenProvided()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "old@test.com", Name = "OldName" };

            _currentUserMock.Setup(c => c.UserId).Returns(userId);
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new UpdateUserCommand { Email = "new@test.com", Name = "NewName" };

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("new@test.com", user.Email);
            Assert.Equal("NewName", user.Name);

            _repoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdatesOnlyEmail_WhenNameIsEmpty()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "old@test.com", Name = "OldName" };

            _currentUserMock.Setup(c => c.UserId).Returns(userId);
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            var command = new UpdateUserCommand { Email = "new@test.com", Name = "" };

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("new@test.com", user.Email);
            Assert.Equal("OldName", user.Name);
        }

        [Fact]
        public async Task Handle_UpdatesOnlyName_WhenEmailIsEmpty()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "old@test.com", Name = "OldName" };

            _currentUserMock.Setup(c => c.UserId).Returns(userId);
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            var command = new UpdateUserCommand { Email = "", Name = "NewName" };

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("old@test.com", user.Email);
            Assert.Equal("NewName", user.Name);
        }
    }
}