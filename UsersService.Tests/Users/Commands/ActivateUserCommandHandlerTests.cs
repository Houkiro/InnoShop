using Microsoft.Extensions.Logging;
using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ActivateUser;
using UsersService.Domain.Entities;

public class ActivateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductServiceClient> _productServiceMock;
    private readonly Mock<ILogger<ActivateUserCommandHandler>> _loggerMock;
    private readonly ActivateUserCommandHandler _handler;

    public ActivateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productServiceMock = new Mock<IProductServiceClient>();
        _loggerMock = new Mock<ILogger<ActivateUserCommandHandler>>();

        _handler = new ActivateUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _productServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_UserExists_ActivatesUserAndRestoresProducts()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = false };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _productServiceMock.Setup(p => p.RestoreProductsByUserIdAsync(userId))
            .Returns(Task.CompletedTask);

        var command = new ActivateUserCommand(userId);

        await _handler.Handle(command, CancellationToken.None);

        Assert.True(user.IsActive);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _productServiceMock.Verify(p => p.RestoreProductsByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var command = new ActivateUserCommand(userId);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProductServiceThrows_LogsError()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = false };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _productServiceMock.Setup(p => p.RestoreProductsByUserIdAsync(userId))
            .ThrowsAsync(new Exception("Service error"));

        var command = new ActivateUserCommand(userId);

        await _handler.Handle(command, CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error restoring products")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
