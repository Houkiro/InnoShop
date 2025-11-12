using MediatR;

namespace UsersService.Application.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<Unit>;
}