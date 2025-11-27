using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UsersService.Domain.Entities;
using UsersService.Infrastructure.Auth;

namespace UsersService.Tests.Auth
{
    public class JwtTokenGeneratorTests
    {
        private JwtSettings GetValidSettings()
        {
            return new JwtSettings
            {
                Key = "super-secret-key-1234567890-super-secret-key-1234567890", 
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 30
            };
        }

        [Fact]
        public void GenerateToken_ReturnsValidJwt_WithCorrectClaims()
        {
            var settings = Options.Create(GetValidSettings());
            var generator = new JwtTokenGenerator(settings);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Role = "Admin"
            };

            var tokenString = generator.GenerateToken(user);

            Assert.False(string.IsNullOrEmpty(tokenString));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenString);

            Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.Equal("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);

            Assert.Equal(settings.Value.Issuer, jwt.Issuer);
            Assert.Equal(settings.Value.Audience, jwt.Audiences.First());
            Assert.True(jwt.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_UsesDefaultRole_WhenRoleIsNull()
        {
            var settings = Options.Create(GetValidSettings());
            var generator = new JwtTokenGenerator(settings);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Role = null
            };

            var tokenString = generator.GenerateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenString);

            Assert.Equal(Roles.User, jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }
    }
}