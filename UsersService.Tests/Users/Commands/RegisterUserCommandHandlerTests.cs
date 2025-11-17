using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.RegisterUser;
using UsersService.Domain.Entities;

namespace UsersService.Tests.Users.Commands
{
    public class RegisterUserCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCreateUserAndSendConfirmationEmail()
        {
            // Arrange
            var repoMock = new Mock<IUserRepository>();
            var emailServiceMock = new Mock<IEmailService>();
            var uowMock = new Mock<IUnitOfWork>();

            User? addedUser = null;

            repoMock
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => addedUser = u)
                .Returns(Task.CompletedTask);

            uowMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(0);

            var command = new RegisterUserCommand(
                "Test User",
                "test@example.com",
                "Password123!"
            );

            var handler = new RegisterUserCommandHandler(
                repoMock.Object,
                emailServiceMock.Object,
                uowMock.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            Assert.NotNull(addedUser);
            Assert.Equal(result, addedUser!.Id);
            Assert.Equal("Test User", addedUser.Name);
            Assert.Equal("test@example.com", addedUser.Email);
            Assert.False(addedUser.IsActive);
            Assert.False(string.IsNullOrWhiteSpace(addedUser.PasswordHash));
            Assert.False(string.IsNullOrWhiteSpace(addedUser.ConfirmationToken));

            repoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            emailServiceMock.Verify(x =>
                x.SendEmailAsync(
                    addedUser!.Email,
                    "Подтверждение аккаунта",
                    It.Is<string>(s => s.Contains(addedUser.ConfirmationToken))
                ), Times.Once);
        }
    }
}