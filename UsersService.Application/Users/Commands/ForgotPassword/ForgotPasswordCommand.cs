using MediatR;

namespace UsersService.Application.Users.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<Unit>;
}