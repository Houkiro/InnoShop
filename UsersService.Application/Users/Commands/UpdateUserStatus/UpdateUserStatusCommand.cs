using MediatR;

namespace UsersService.Application.Users.Commands.UpdateUserStatus
{
    public record UpdateUserStatusCommand(Guid UserId, bool IsActive) : IRequest<Unit>;
}