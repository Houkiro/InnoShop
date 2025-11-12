using MediatR;

namespace UsersService.Application.Commands.RegisterUser
{
    public record RegisterUserCommand(string Name, string Email, string Password) : IRequest<Guid>;
}