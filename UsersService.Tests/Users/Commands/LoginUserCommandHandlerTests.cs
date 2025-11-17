using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.LoginUserCommand;
using UsersService.Domain.Entities;

namespace UsersService.Tests.Users.Commands
{
    public class LoginUserCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnJwtToken_WhenCredentialsAreValid()
        {
            var repoMock = new Mock<IUserRepository>();
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

            var handler = new LoginUserCommandHandler(repoMock.Object, jwtMock.Object);

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