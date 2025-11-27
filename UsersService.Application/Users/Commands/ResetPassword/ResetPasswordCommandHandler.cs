using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Users.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly IUserRepository _repo;
        private readonly IUnitOfWork _uow;

        public ResetPasswordCommandHandler(IUserRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByResetPasswordTokenAsync(request.Token);
            if (user == null || user.PasswordResetTokenExpires < DateTime.UtcNow)
                throw new BadRequestException("Ссылка недействительна или просрочена.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _repo.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            return Unit.Value;
        }
    }
}