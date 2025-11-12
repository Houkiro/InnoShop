using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Users.Commands.UpdateUserCommand
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IUserRepository _repo;

        public UpdateUserCommandHandler(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;

            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync();
        }
    }
}
