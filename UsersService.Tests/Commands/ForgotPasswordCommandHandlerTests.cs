using MediatR;
using Moq;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ForgotPassword;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;

namespace UsersService.Tests.CommandHandlers
{
    public class ForgotPasswordCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly ForgotPasswordCommandHandler _handler;

        public ForgotPasswordCommandHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _uowMock = new Mock<IUnitOfWork>();

            _handler = new ForgotPasswordCommandHandler(
                _repoMock.Object,
                _emailServiceMock.Object,
                _uowMock.Object
            );
        }

        [Fact]
        public async Task Handle_ThrowsBadRequest_WhenUserNotFound()
        {
            _repoMock.Setup(r => r.GetByEmailAsync("missing@test.com"))
                .ReturnsAsync((User?)null);

            var command = new ForgotPasswordCommand("missing@test.com");

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_GeneratesToken_AndSendsEmail_WhenUserExists()
        {
            var user = new User { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" };
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email!)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new ForgotPasswordCommand(user.Email!);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            Assert.False(string.IsNullOrEmpty(user.PasswordResetToken));
            Assert.NotNull(user.PasswordResetTokenExpires);
            Assert.True(user.PasswordResetTokenExpires > DateTime.UtcNow);

            _repoMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _emailServiceMock.Verify(e =>
                e.SendEmailAsync(
                    user.Email!,
                    "Сброс пароля",
                    It.Is<string>(html => html.Contains(user.Name)),
                    true),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SetsTokenExpiry_OneHourAhead()
        {
            var user = new User { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" };
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email!)).ReturnsAsync(user);
            _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var command = new ForgotPasswordCommand(user.Email!);

            await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(user.PasswordResetTokenExpires);
            var diff = user.PasswordResetTokenExpires!.Value - DateTime.UtcNow;
            Assert.True(diff.TotalMinutes <= 61 && diff.TotalMinutes >= 59);
        }
    }
}
