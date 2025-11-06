using MediatR;

namespace UsersService.Application.Users.Commands.LoginUser
{
    public record LoginUserCommand(string Email, string Password) : IRequest<string>;
}