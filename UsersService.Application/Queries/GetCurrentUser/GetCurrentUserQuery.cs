using MediatR;
using UsersService.Application.Contracts;

namespace UsersService.Application.Queries.GetCurrentUser
{
    public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;
}