using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.CreateUser;
using UsersService.Domain.Entities;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateUser_AndReturnUserId()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var uowMock = new Mock<IUnitOfWork>();

        var command = new CreateUserCommand(
            Name: "Test User",
            Email: "test@example.com",
            Password: "123456"
        );

        passwordHasherMock
            .Setup(x => x.Hash(command.Password))
            .Returns("hashed_password");

        User? addedUser = null;

        userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => addedUser = u)
            .Returns(Task.CompletedTask);

        uowMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);

        var handler = new CreateUserCommandHandler(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
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
        Assert.Equal("hashed_password", addedUser.PasswordHash);

        passwordHasherMock.Verify(x => x.Hash(command.Password), Times.Once);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        uowMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
