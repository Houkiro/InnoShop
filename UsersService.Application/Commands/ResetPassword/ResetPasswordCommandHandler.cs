using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly IUserRepository _repo;

        public ResetPasswordCommandHandler(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByPasswordResetTokenAsync(request.Token);
            if (user == null || user.PasswordResetTokenExpires < DateTime.UtcNow)
                throw new BadRequestException("Ссылка недействительна или просрочена.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync();

            return Unit.Value;
        }
    }
}