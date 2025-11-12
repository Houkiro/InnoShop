using MediatR;

namespace UsersService.Application.Commands.ResetPassword
{
    public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Unit>;
}