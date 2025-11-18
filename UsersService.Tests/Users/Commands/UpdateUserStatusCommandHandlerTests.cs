using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.UpdateUserStatus;
using UsersService.Application.Users.Commands.UpdateUser;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;

namespace UsersService.Tests.Users.Commands
{
    public class UpdateUserStatusCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly Mock<IProductIntegrationService> _products = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        private UpdateUserStatusCommandHandler CreateHandler()
        {
            return new UpdateUserStatusCommandHandler(
                _repo.Object,
                _products.Object,
                _uow.Object);
        }

        [Fact]
        public async Task Handle_ShouldActivateUser_AndRestoreProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User
            {
                Id = userId,
                Name = "Test",
                Email = "test@example.com",
                IsActive = false
            };

            var repoMock = new Mock<IUserRepository>();
            var productMock = new Mock<IProductIntegrationService>();
            var uowMock = new Mock<IUnitOfWork>();

            repoMock.Setup(r => r.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);

            repoMock.Setup(r => r.UpdateAsync(existingUser))
                    .Returns(Task.CompletedTask);

            uowMock.Setup(u => u.SaveChangesAsync())
                   .ReturnsAsync(0);

            var command = new UpdateUserStatusCommand(userId, true);
            var handler = new UpdateUserStatusCommandHandler(repoMock.Object, productMock.Object, uowMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(existingUser.IsActive);
            productMock.Verify(p => p.RestoreProductsAsync(userId), Times.Once);
            productMock.Verify(p => p.HideProductsAsync(It.IsAny<Guid>()), Times.Never);

            repoMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDeactivateUser_AndHideProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User
            {
                Id = userId,
                Name = "Test",
                Email = "test@example.com",
                IsActive = true
            };

            var repoMock = new Mock<IUserRepository>();
            var productMock = new Mock<IProductIntegrationService>();
            var uowMock = new Mock<IUnitOfWork>();

            repoMock.Setup(r => r.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);

            repoMock.Setup(r => r.UpdateAsync(existingUser))
                    .Returns(Task.CompletedTask);

            uowMock.Setup(u => u.SaveChangesAsync())
                   .ReturnsAsync(0);

            var command = new UpdateUserStatusCommand(userId, false);
            var handler = new UpdateUserStatusCommandHandler(repoMock.Object, productMock.Object, uowMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(existingUser.IsActive);

            productMock.Verify(p => p.HideProductsAsync(userId), Times.Once);
            productMock.Verify(p => p.RestoreProductsAsync(It.IsAny<Guid>()), Times.Never);

            repoMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var repoMock = new Mock<IUserRepository>();
            var productMock = new Mock<IProductIntegrationService>();
            var uowMock = new Mock<IUnitOfWork>();

            repoMock.Setup(r => r.GetByIdAsync(userId))
                    .ReturnsAsync((User?)null);

            var command = new UpdateUserStatusCommand(userId, true);
            var handler = new UpdateUserStatusCommandHandler(repoMock.Object, productMock.Object, uowMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            productMock.Verify(p => p.RestoreProductsAsync(It.IsAny<Guid>()), Times.Never);
            productMock.Verify(p => p.HideProductsAsync(It.IsAny<Guid>()), Times.Never);
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var handler = CreateHandler();
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User)null);

            var cmd = new UpdateUserStatusCommand(Guid.NewGuid(), true);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, default));
        }

        [Fact]
        public async Task Handle_ActivateUser_CallsRestoreProducts()
        {
            // Arrange
            var handler = CreateHandler();
            var user = new User { Id = Guid.NewGuid(), IsActive = false };

            _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var cmd = new UpdateUserStatusCommand(user.Id, true);

            // Act
            await handler.Handle(cmd, default);

            // Assert
            Assert.True(user.IsActive);
            _products.Verify(p => p.RestoreProductsAsync(user.Id), Times.Once);
            _products.Verify(p => p.HideProductsAsync(It.IsAny<Guid>()), Times.Never);
            _repo.Verify(r => r.UpdateAsync(user), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_DeactivateUser_CallsHideProducts()
        {
            // Arrange
            var handler = CreateHandler();
            var user = new User { Id = Guid.NewGuid(), IsActive = true };

            _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var cmd = new UpdateUserStatusCommand(user.Id, false);

            // Act
            await handler.Handle(cmd, default);

            // Assert
            Assert.False(user.IsActive);
            _products.Verify(p => p.HideProductsAsync(user.Id), Times.Once);
            _products.Verify(p => p.RestoreProductsAsync(It.IsAny<Guid>()), Times.Never);
            _repo.Verify(r => r.UpdateAsync(user), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
