using MediatR;

namespace UsersService.Application.Users.Commands.CreateUser
{
    public record CreateUserCommand(string Name, string Email, string Password) : IRequest<Guid>;
}