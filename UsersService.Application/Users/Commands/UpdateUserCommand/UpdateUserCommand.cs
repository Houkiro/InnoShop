using MediatR;

namespace UsersService.Application.Users.Commands.UpdateUserCommand
{
    public class UpdateUserCommand : IRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}