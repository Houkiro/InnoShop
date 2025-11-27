using MediatR;
using UsersService.Application.Interfaces;

namespace UsersService.Application.Users.Commands.ChangeUserRole
{
    public class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _uow;

        public ChangeUserRoleHandler(IUserRepository userRepository, ICurrentUserService currentUser, IUnitOfWork uow)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _uow = uow;
        }

        public async Task Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.Role != "Admin")
                throw new UnauthorizedAccessException("Only admins can change roles");

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.Role = request.NewRole;

            await _userRepository.UpdateAsync(user);
            await _uow.SaveChangesAsync();
        }
    }
}