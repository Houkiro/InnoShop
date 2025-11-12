using MediatR;

namespace UsersService.Application.Users.Commands.UpdateUserCommand
{
    public class UpdateUserCommand : IRequest
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}