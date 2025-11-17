using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ConfirmUser;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;

public class ConfirmUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ConfirmUserCommandHandler _handler;

    public ConfirmUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ConfirmUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidToken_ConfirmsUser()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = Guid.NewGuid(),
            IsActive = false,
            ConfirmationToken = token,
            ConfirmationTokenExpires = DateTime.UtcNow.AddHours(1)
        };

        _userRepositoryMock.Setup(r => r.GetByConfirmationTokenAsync(token))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new ConfirmUserCommand(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(user.IsActive);
        Assert.Null(user.ConfirmationToken);
        Assert.Null(user.ConfirmationTokenExpires);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsBadRequestException()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        _userRepositoryMock.Setup(r => r.GetByConfirmationTokenAsync(token))
            .ReturnsAsync((User?)null);

        var command = new ConfirmUserCommand(token);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsBadRequestException()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = Guid.NewGuid(),
            IsActive = false,
            ConfirmationToken = token,
            ConfirmationTokenExpires = DateTime.UtcNow.AddHours(-1)
        };

        _userRepositoryMock.Setup(r => r.GetByConfirmationTokenAsync(token))
            .ReturnsAsync(user);

        var command = new ConfirmUserCommand(token);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}