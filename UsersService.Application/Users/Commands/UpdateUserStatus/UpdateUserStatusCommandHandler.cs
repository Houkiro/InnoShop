using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.UpdateUserStatus;
using UsersService.Domain.Exceptions;

namespace UsersService.Application.Users.Commands.UpdateUser
{
    public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, Unit>
    {
        private readonly IUserRepository _repo;
        private readonly IProductIntegrationService _products;
        private readonly IUnitOfWork _uow;

        public UpdateUserStatusCommandHandler(IUserRepository repo, IProductIntegrationService products, IUnitOfWork uow)
        {
            _repo = repo;
            _products = products;
            _uow = uow;
        }

        public async Task<Unit> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            user.IsActive = request.IsActive;
            await _repo.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            if (user.IsActive)
                await _products.RestoreProducts(user.Id);
            else
                await _products.HideProducts(user.Id);

            return Unit.Value;
        }
    }
}
