using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UsersService.Domain.Entities;
using UsersService.Infrastructure.Auth;

namespace UsersService.Tests.Users.Services
{
    public class JwtTokenGeneratorTests
    {
        [Fact]
        public void GenerateToken_ShouldCreateValidJwt_WithCorrectClaims()
        {
            var settings = new JwtSettings
            {
                Key = "THIS_IS_A_TEST_SECRET_KEY_12345678901234567890",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60
            };

            var generator = new JwtTokenGenerator(Options.Create(settings));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                Role = "Admin"
            };

            var token = generator.GenerateToken(user);

            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.Equal(user.Role, jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);

            Assert.Equal(settings.Issuer, jwt.Issuer);
            Assert.Contains(settings.Audience, jwt.Audiences);

            Assert.Contains(".", token);
        }

        [Fact]
        public void GenerateToken_ShouldThrow_WhenKeyIsTooShort()
        {
            var settings = new JwtSettings
            {
                Key = "short-key",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60
            };

            var generator = new JwtTokenGenerator(Options.Create(settings));

            var user = new User { Id = Guid.NewGuid() };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                generator.GenerateToken(user);
            });
        }
    }
}