using MediatR;

namespace UsersService.Application.Users.Commands.ResetPassword
{
    public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Unit>;
}