using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.UpdateUserCommand;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;

namespace UsersService.Tests.Users.Commands
{
    public class UpdateUserCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User
            {
                Id = userId,
                Name = "Old Name",
                Email = "old@example.com"
            };

            var repoMock = new Mock<IUserRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            repoMock.Setup(r => r.GetByIdAsync(userId))
                    .ReturnsAsync(existingUser);
            repoMock.Setup(r => r.UpdateAsync(existingUser))
                    .Returns(Task.CompletedTask);
            uowMock.Setup(u => u.SaveChangesAsync())
                   .ReturnsAsync(0);

            var command = new UpdateUserCommand
            {
                UserId = userId,
                Name = "New Name",
                Email = "new@example.com"
            };

            var handler = new UpdateUserCommandHandler(repoMock.Object, uowMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("New Name", existingUser.Name);
            Assert.Equal("new@example.com", existingUser.Email);

            repoMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var repoMock = new Mock<IUserRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            var userId = Guid.NewGuid();
            repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var command = new UpdateUserCommand
            {
                UserId = userId,
                Name = "New Name",
                Email = "new@example.com"
            };
            var handler = new UpdateUserCommandHandler(repoMock.Object, uowMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldUpdateOnlyProvidedFields()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User
            {
                Id = userId,
                Name = "Old Name",
                Email = "old@example.com"
            };

            var repoMock = new Mock<IUserRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            repoMock.Setup(r => r.UpdateAsync(existingUser)).Returns(Task.CompletedTask);
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var command = new UpdateUserCommand
            {
                UserId = userId,
                Name = "New Name",
                Email = null
            };

            var handler = new UpdateUserCommandHandler(repoMock.Object, uowMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("New Name", existingUser.Name);
            Assert.Equal("old@example.com", existingUser.Email); 

            repoMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

    }
}
