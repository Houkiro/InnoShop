using Microsoft.AspNetCore.Http;
using ProductsService.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProductsService.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _context;

        public CurrentUserService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public Guid UserId
        {
            get
            {
                var id = _context.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? _context.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (id == null)
                    throw new UnauthorizedAccessException("Missing UserId");

                return Guid.Parse(id);
            }
        }
    }
}