using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UsersService.Infrastructure.Services;
using Xunit;

namespace UsersService.Tests.Services
{
    public class CurrentUserServiceTests
    {
        private static HttpContext CreateHttpContext(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            return new DefaultHttpContext { User = principal };
        }

        [Fact]
        public void UserId_ReturnsGuid_WhenClaimExists()
        {
            var userId = Guid.NewGuid();
            var context = CreateHttpContext(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);

            var service = new CurrentUserService(accessor.Object);

            Assert.Equal(userId, service.UserId);
        }

        [Fact]
        public void UserId_ThrowsUnauthorized_WhenClaimMissing()
        {
            var context = CreateHttpContext(); // без NameIdentifier
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);

            var service = new CurrentUserService(accessor.Object);

            Assert.Throws<UnauthorizedAccessException>(() => _ = service.UserId);
        }

        [Fact]
        public void Role_ReturnsRoleClaim_WhenExists()
        {
            var context = CreateHttpContext(new Claim(ClaimTypes.Role, "Admin"));
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);

            var service = new CurrentUserService(accessor.Object);

            Assert.Equal("Admin", service.Role);
        }

        [Fact]
        public void Role_ReturnsSubClaim_WhenRoleMissing()
        {
            var context = CreateHttpContext(new Claim(JwtRegisteredClaimNames.Sub, "UserSub"));
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);

            var service = new CurrentUserService(accessor.Object);

            Assert.Equal("UserSub", service.Role);
        }

        [Fact]
        public void Role_ThrowsUnauthorized_WhenNoClaims()
        {
            var context = CreateHttpContext();
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);

            var service = new CurrentUserService(accessor.Object);

            Assert.Throws<UnauthorizedAccessException>(() => _ = service.Role);
        }
    }
}
