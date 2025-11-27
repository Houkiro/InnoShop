using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.UpdateUser;
using UsersService.Application.Users.Commands.UpdateUserStatus;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;
using Xunit;

namespace UsersService.Tests.CommandHandlers
{
    public class UpdateUserStatusCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IProductIntegrationService> _productsMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly UpdateUserStatusCommandHandler _handler;

        public UpdateUserStatusCommandHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _productsMock = new Mock<IProductIntegrationService>();
            _uowMock = new Mock<IUnitOfWork>();

            _handler = new UpdateUserStatusCommandHandler(
                _repoMock.Object,
                _productsMock.Object,
                _uowMock.Object
            );
        }

        [Fact]
        public async Task Handle_ThrowsNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var command = new UpdateUserStatusCommand(userId, true);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ActivatesUser_AndRestoresProducts()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, IsActive = false };

            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new UpdateUserStatusCommand(userId, true);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            Assert.True(user.IsActive);

            _repoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _productsMock.Verify(p => p.RestoreProductsAsync(userId), Times.Once);
            _productsMock.Verify(p => p.HideProductsAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DeactivatesUser_AndHidesProducts()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, IsActive = true };

            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new UpdateUserStatusCommand(userId, false);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            Assert.False(user.IsActive);

            _repoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _productsMock.Verify(p => p.HideProductsAsync(userId), Times.Once);
            _productsMock.Verify(p => p.RestoreProductsAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}