using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UsersService.Infrastructure.Services;
using UsersService.Application.Interfaces;

namespace UsersService.Tests.Services
{
    public class CurrentUserServiceTests
    {
        private static IHttpContextAccessor CreateAccessorWithClaims(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            var accessor = new HttpContextAccessor
            {
                HttpContext = httpContext
            };

            return accessor;
        }

        [Fact]
        public void UserId_ShouldReturn_Guid_WhenClaimExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var accessor = CreateAccessorWithClaims(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            });

            var service = new CurrentUserService(accessor);

            // Act
            var result = service.UserId;

            // Assert
            Assert.Equal(userId, result);
        }

        [Fact]
        public void UserId_ShouldThrow_WhenMissing()
        {
            // Arrange
            var accessor = CreateAccessorWithClaims(Array.Empty<Claim>());
            var service = new CurrentUserService(accessor);

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => _ = service.UserId);
        }

        [Fact]
        public void Role_ShouldReturnRole_WhenClaimExists()
        {
            // Arrange
            var accessor = CreateAccessorWithClaims(new[]
            {
                new Claim(ClaimTypes.Role, "Admin")
            });

            var service = new CurrentUserService(accessor);

            // Act
            var result = service.Role;

            // Assert
            Assert.Equal("Admin", result);
        }

        [Fact]
        public void Role_ShouldFallbackToSub_WhenRoleMissing()
        {
            // Arrange
            var accessor = CreateAccessorWithClaims(new[]
            {
                new Claim("sub", "User123")
            });

            var service = new CurrentUserService(accessor);

            // Act
            var result = service.Role;

            // Assert
            Assert.Equal("User123", result);
        }

        [Fact]
        public void Role_ShouldThrow_WhenNoRoleAndNoSub()
        {
            // Arrange
            var accessor = CreateAccessorWithClaims(Array.Empty<Claim>());
            var service = new CurrentUserService(accessor);

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => _ = service.Role);
        }
    }
}
