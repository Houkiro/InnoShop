using MediatR;

namespace UsersService.Application.Commands.ConfirmUser
{
    public record ConfirmUserCommand(string Token) : IRequest<bool>;
}