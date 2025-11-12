using MediatR;

namespace UsersService.Application.Users.Commands.LoginUserCommand
{
    public class LoginUserCommand : IRequest<string> 
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}