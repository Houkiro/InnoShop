using MediatR;

namespace UsersService.Application.Users.Commands.ActivateUser
{
    public record ActivateUserCommand(Guid UserId) : IRequest;
}