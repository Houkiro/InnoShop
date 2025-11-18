using Microsoft.AspNetCore.Http;
using Moq;
using ProductsService.Infrastructure.Services;
using System.Security.Claims;

namespace ProductsService.Tests.Products.Services
{
    public class CurrentUserServiceTests
    {
        [Fact]
        public void UserId_ShouldReturnGuid_WhenClaimExists()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.User).Returns(principal);

            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            // Act
            var result = service.UserId;

            // Assert
            Assert.Equal(Guid.Parse(userId), result);
        }

        [Fact]
        public void UserId_ShouldThrowUnauthorizedAccessException_WhenClaimMissing()
        {
            // Arrange
            var identity = new ClaimsIdentity(); 
            var principal = new ClaimsPrincipal(identity);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.User).Returns(principal);

            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => service.UserId);
        }

        [Fact]
        public void UserId_ShouldThrowUnauthorizedAccessException_WhenHttpContextIsNull()
        {
            // Arrange
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);

            var service = new CurrentUserService(accessorMock.Object);

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => service.UserId);
        }
    }
}