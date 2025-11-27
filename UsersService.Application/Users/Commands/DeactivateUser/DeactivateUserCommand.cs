using MediatR;

namespace UsersService.Application.Users.Commands.DeactivateUser
{
    public record DeactivateUserCommand(Guid UserId) : IRequest;
}