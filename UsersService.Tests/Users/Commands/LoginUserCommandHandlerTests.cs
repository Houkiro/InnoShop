
using Microsoft.VisualStudio.Services.WebApi.Jwt;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.LoginUserCommand;
using UsersService.Domain.Entities;
using UsersService.Domain.Exceptions;

namespace UsersService.Tests.Users.Commands
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtSettingsProvider> _jwtSettingsMock;

        public LoginUserCommandHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtSettingsMock = new Mock<IJwtSettingsProvider>();
        }

        private LoginUserCommandHandler CreateHandler() =>
            new LoginUserCommandHandler(
                _repoMock.Object,
                _passwordHasherMock.Object,
                _jwtSettingsMock.Object
            );

        [Fact]
        public async Task Handle_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var cmd = new LoginUserCommand { Email = "test@mail.com", Password = "123" };
            _repoMock.Setup(r => r.GetByEmailAsync(cmd.Email)).ReturnsAsync((User)null);

            var handler = CreateHandler();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, default));
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsBadRequestException()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct_password"),
                IsEmailConfirmed = true
            };

            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var cmd = new LoginUserCommand { Email = user.Email, Password = "wrong_password" };
            var handler = CreateHandler();

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, default));
        }

        [Fact]
        public async Task Handle_EmailNotConfirmed_ThrowsBadRequestException()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsEmailConfirmed = false
            };
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var cmd = new LoginUserCommand { Email = user.Email, Password = "123456" };
            var handler = CreateHandler();

            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, default));
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsJwtToken()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Name = "Test User",
                Role = "User",
                IsEmailConfirmed = true
            };

            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            // Настройка JWT для генерации токена
            _jwtSettingsMock.SetupGet(j => j.Key).Returns("ThisIsA32CharLongSecretKeyForTesting!");
            _jwtSettingsMock.SetupGet(j => j.Issuer).Returns("TestIssuer");
            _jwtSettingsMock.SetupGet(j => j.Audience).Returns("TestAudience");
            _jwtSettingsMock.SetupGet(j => j.ExpirationMinutes).Returns(60);

            var cmd = new LoginUserCommand { Email = user.Email, Password = "123456" };
            var handler = CreateHandler();

            var token = await handler.Handle(cmd, default);

            Assert.NotNull(token);
            Assert.IsType<string>(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task Handle_ShouldReturnJwtToken_WhenCredentialsAreValid()
        {
            var repoMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtMock = new Mock<IJwtSettingsProvider>();

            var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = passwordHash,
                IsEmailConfirmed = true,
                Role = "Admin"
            };

            repoMock
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            jwtMock.SetupGet(x => x.Key).Returns("verysecretkeyverysecretkey123456!");
            jwtMock.SetupGet(x => x.Issuer).Returns("TestIssuer");
            jwtMock.SetupGet(x => x.Audience).Returns("TestAudience");
            jwtMock.SetupGet(x => x.ExpirationMinutes).Returns(60);

            var handler = new LoginUserCommandHandler(
                repoMock.Object,
                passwordHasherMock.Object,
                jwtMock.Object
            );

            var command = new LoginUserCommand
            {
                Email = user.Email!,
                Password = "123456"
            };

            // Act
            var tokenString = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(tokenString));

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);

            Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

    }
}