using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.DeactivateUser;
using UsersService.Domain.Entities;

namespace UsersService.Tests.Users.Commands
{
    public class DeactivateUserCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldDeactivateUser_AndHideProducts()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var uowMock = new Mock<IUnitOfWork>();
            var productServiceMock = new Mock<IProductServiceClient>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "test@mail.com",
                PasswordHash = "hash",
                IsActive = true
            };

            userRepositoryMock
                .Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            userRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(0);

            productServiceMock
                .Setup(x => x.HideProductsByUserIdAsync(user.Id))
                .Returns(Task.CompletedTask);

            var handler = new DeactivateUserCommandHandler(
                userRepositoryMock.Object,
                uowMock.Object,
                productServiceMock.Object
            );

            var command = new DeactivateUserCommand(user.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(user.IsActive);

            userRepositoryMock.Verify(x => x.GetByIdAsync(user.Id), Times.Once);
            userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
            uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            productServiceMock.Verify(x => x.HideProductsByUserIdAsync(user.Id), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotThrow_WhenProductServiceFails()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var uowMock = new Mock<IUnitOfWork>();
            var productServiceMock = new Mock<IProductServiceClient>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "test@mail.com",
                PasswordHash = "hash",
                IsActive = true
            };

            userRepositoryMock
                .Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            userRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(0);

            productServiceMock
                .Setup(x => x.HideProductsByUserIdAsync(user.Id))
                .ThrowsAsync(new Exception("Service unavailable"));

            var handler = new DeactivateUserCommandHandler(
                userRepositoryMock.Object,
                uowMock.Object,
                productServiceMock.Object
            );

            var command = new DeactivateUserCommand(user.Id);

            await handler.Handle(command, CancellationToken.None);

            Assert.False(user.IsActive);

            productServiceMock.Verify(x => x.HideProductsByUserIdAsync(user.Id), Times.Once);
        }
    }
}