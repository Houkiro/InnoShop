using Microsoft.AspNetCore.Http;
using ProductsService.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace ProductsService.Tests.Infrastructure.Services
{
    public class CurrentUserServiceTests
    {
        private static CurrentUserService CreateServiceWithClaims(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext();
            context.User = principal;

            var accessor = new HttpContextAccessor { HttpContext = context };
            return new CurrentUserService(accessor);
        }

        [Fact]
        public void UserId_ReturnsGuid_WhenNameIdentifierClaimExists()
        {
            var expectedId = Guid.NewGuid();
            var service = CreateServiceWithClaims(new Claim(ClaimTypes.NameIdentifier, expectedId.ToString()));

            var actualId = service.UserId;

            Assert.Equal(expectedId, actualId);
        }

        [Fact]
        public void UserId_ReturnsGuid_WhenSubClaimExists()
        {
            var expectedId = Guid.NewGuid();
            var service = CreateServiceWithClaims(new Claim(JwtRegisteredClaimNames.Sub, expectedId.ToString()));

            var actualId = service.UserId;

            Assert.Equal(expectedId, actualId);
        }

        [Fact]
        public void UserId_ThrowsUnauthorizedAccessException_WhenNoClaimsExist()
        {
            var service = CreateServiceWithClaims();

            Assert.Throws<UnauthorizedAccessException>(() => { var _ = service.UserId; });
        }
    }
}
