using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.RegisterUser;
using User = UsersService.Domain.Entities.User;

namespace UsersService.Tests.CommandHandlers
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly RegisterUserCommandHandler _handler;

        public RegisterUserCommandHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _uowMock = new Mock<IUnitOfWork>();

            _handler = new RegisterUserCommandHandler(
                _repoMock.Object,
                _emailServiceMock.Object,
                _uowMock.Object
            );
        }

        [Fact]
        public async Task Handle_CreatesUser_AndSendsConfirmationEmail()
        {
            var command = new RegisterUserCommand("TestName", "test@test.com", "password");

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, result);

            _repoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.Name == "TestName" &&
                u.Email == "test@test.com" &&
                !string.IsNullOrEmpty(u.PasswordHash) &&
                !u.IsActive &&
                !string.IsNullOrEmpty(u.ConfirmationToken) &&
                u.ConfirmationTokenExpires > DateTime.UtcNow)), Times.Once);

            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);

            _emailServiceMock.Verify(e =>
                e.SendEmailAsync(
                    It.Is<string>(email => email == "test@test.com"),
                    "Подтверждение аккаунта",
                    It.Is<string>(html => html.Contains("Привет TestName") && html.Contains("Активировать")),
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsUserId()
        {
            var command = new RegisterUserCommand("Name", "email@test.com", "pwd");

            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, result);
        }
    }
}