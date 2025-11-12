using MediatR;

namespace UsersService.Application.Users.Commands.ConfirmUser
{
    public record ConfirmUserCommand(string Token) : IRequest<bool>;
}