using System.Security.Claims;

namespace ProductsService.Extensions
{
    public static class UserExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("UserId not found in token");

            return Guid.Parse(id);
        }
    }
}