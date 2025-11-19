using FluentAssertions;
using ProductsService.Extensions;
using System.Security.Claims;

namespace ProductsService.Tests.Extensions
{
    public class UserExtensionsTests
    {
        [Fact]
        public void GetUserId_ReturnsGuid_WhenClaimExists()
        {
            // Arrange
            var expectedId = Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, expectedId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = principal.GetUserId();

            // Assert
            result.Should().Be(Guid.Parse(expectedId));
        }

        [Fact]
        public void GetUserId_ThrowsUnauthorizedAccessException_WhenClaimMissing()
        {
            // Arrange
            var identity = new ClaimsIdentity(); 
            var principal = new ClaimsPrincipal(identity);

            // Act
            var act = () => principal.GetUserId();

            // Assert
            act.Should()
                .Throw<UnauthorizedAccessException>()
                .WithMessage("UserId not found in token");
        }

        [Fact]
        public void GetUserId_ThrowsFormatException_WhenClaimIsInvalidGuid()
        {
            // Arrange
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "NOT-A-GUID")
            };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Act
            var act = () => principal.GetUserId();

            // Assert
            act.Should().Throw<FormatException>();
        }
    }
}
