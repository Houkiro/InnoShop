using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.LoginUserCommand;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;
using Xunit;

namespace UsersService.Tests.CommandHandlers
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IJwtSettingsProvider> _jwtSettingsMock;
        private readonly LoginUserCommandHandler _handler;

        public LoginUserCommandHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _jwtSettingsMock = new Mock<IJwtSettingsProvider>();

            _jwtSettingsMock.SetupGet(s => s.Key)
                .Returns("super-secret-key-1234567890-super-secret-key-1234567890");
            _jwtSettingsMock.SetupGet(s => s.Issuer).Returns("TestIssuer");
            _jwtSettingsMock.SetupGet(s => s.Audience).Returns("TestAudience");
            _jwtSettingsMock.SetupGet(s => s.ExpirationMinutes).Returns(30);

            _handler = new LoginUserCommandHandler(_repoMock.Object, Mock.Of<IPasswordHasher>(), _jwtSettingsMock.Object);
        }

        [Fact]
        public async Task Handle_ThrowsNotFound_WhenUserNotExists()
        {
            _repoMock.Setup(r => r.GetByEmailAsync("missing@test.com"))
                .ReturnsAsync((User?)null);

            var command = new LoginUserCommand { Email = "missing@test.com", Password = "pwd" };

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsBadRequest_WhenPasswordInvalid()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct"), IsEmailConfirmed = true };
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email!)).ReturnsAsync(user);

            var command = new LoginUserCommand { Email = user.Email!, Password = "wrong" };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ThrowsBadRequest_WhenEmailNotConfirmed()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pwd"), IsEmailConfirmed = false };
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email!)).ReturnsAsync(user);

            var command = new LoginUserCommand { Email = user.Email!, Password = "pwd" };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ReturnsJwtToken_WhenValidCredentials()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pwd"),
                IsEmailConfirmed = true,
                Role = "Admin"
            };
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email!)).ReturnsAsync(user);

            var command = new LoginUserCommand { Email = user.Email!, Password = "pwd" };

            var tokenString = await _handler.Handle(command, CancellationToken.None);

            Assert.False(string.IsNullOrEmpty(tokenString));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenString);

            Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }
    }
}