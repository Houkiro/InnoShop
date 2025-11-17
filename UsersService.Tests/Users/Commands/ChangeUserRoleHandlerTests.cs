using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ChangeUserRole;
using UsersService.Domain.Entities;

public class ChangeUserRoleHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ChangeUserRoleHandler _handler;

    public ChangeUserRoleHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new ChangeUserRoleHandler(
            _userRepositoryMock.Object,
            _currentUserMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_AdminUser_UpdatesRoleSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Role = "User" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _currentUserMock.Setup(c => c.Role).Returns("Admin");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new ChangeUserRoleCommand(userId, "Admin");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        Assert.Equal("Admin", user.Role); 
    }

    [Fact]
    public async Task Handle_NonAdminUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _currentUserMock.Setup(c => c.Role).Returns(Roles.User);
        var command = new ChangeUserRoleCommand(Guid.NewGuid(), Roles.Admin);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _currentUserMock.Setup(c => c.Role).Returns(Roles.Admin);
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var command = new ChangeUserRoleCommand(userId, Roles.Admin);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}