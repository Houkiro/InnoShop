using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Users.Commands.ConfirmUser
{
    public class ConfirmUserCommandHandler : IRequestHandler<ConfirmUserCommand, bool>
    {
        private readonly IUserRepository _repo;
        private readonly IUnitOfWork _uow;

        public ConfirmUserCommandHandler(IUserRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<bool> Handle(ConfirmUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByConfirmationTokenAsync(request.Token);
            if (user == null)
                throw new BadRequestException("Пользователь не найден или токен неверный.");

            if (user.IsEmailConfirmed)
                return true; 

            if (user.ConfirmationTokenExpires == null || user.ConfirmationTokenExpires < DateTime.UtcNow)
                throw new BadRequestException("Ссылка недействительна или просрочена.");

            user.IsActive = true;
            user.IsEmailConfirmed = true;
            user.ConfirmationToken = null;
            user.ConfirmationTokenExpires = null;

            await _repo.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            return true;
        }

    }
}
