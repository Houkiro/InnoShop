using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Services
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
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (id == null)
                    throw new UnauthorizedAccessException("Missing UserId");

                return Guid.Parse(id);
            }
        }

        public string Role
        {
            get
            {
                var role = _context.HttpContext?.User?
                    .FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(role))
                    throw new UnauthorizedAccessException("Missing Role claim");

                return role;
            }
        }
    }
}