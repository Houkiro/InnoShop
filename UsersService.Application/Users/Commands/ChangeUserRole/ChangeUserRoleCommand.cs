using MediatR;

namespace UsersService.Application.Users.Commands.ChangeUserRole
{
    public record ChangeUserRoleCommand(Guid UserId, string NewRole) : IRequest;
}