using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Users.Commands.UpdateUserCommand
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IUserRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UpdateUserCommandHandler(IUserRepository repo, IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _repo = repo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByIdAsync(_currentUser.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;

            await _repo.UpdateAsync(user);
            await _uow.SaveChangesAsync();
        }
    }
}
