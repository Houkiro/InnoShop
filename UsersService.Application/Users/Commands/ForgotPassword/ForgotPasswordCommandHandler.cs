using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Users.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly IUserRepository _repo;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(IUserRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }

        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByEmailAsync(request.Email);
            if (user == null)
                throw new BadRequestException("Пользователь с таким email не найден.");

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync();

            var resetUrl = $"https://localhost:7239/api/users/reset-password?token={user.PasswordResetToken}";
            var html = $"<p>Привет {user.Name},</p><p>Чтобы сбросить пароль, перейдите по ссылке: <a href='{resetUrl}'>Сбросить пароль</a></p>";
            await _emailService.SendEmailAsync(user.Email!, "Сброс пароля", html);

            return Unit.Value;
        }
    }
}
