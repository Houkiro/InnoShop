using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Commands.ConfirmUser
{
    public class ConfirmUserCommandHandler : IRequestHandler<ConfirmUserCommand, bool>
    {
        private readonly IUserRepository _repo;

        public ConfirmUserCommandHandler(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(ConfirmUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByConfirmationTokenAsync(request.Token);
            if (user == null || user.ConfirmationTokenExpires < DateTime.UtcNow)
                throw new BadRequestException("Ссылка недействительна или просрочена.");

            user.IsActive = true;
            user.ConfirmationToken = null;
            user.ConfirmationTokenExpires = null;

            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync();

            return true;
        }
    }
}