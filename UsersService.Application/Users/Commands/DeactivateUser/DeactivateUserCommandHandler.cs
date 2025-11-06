using MediatR;
using UsersService.Application.Interfaces;

namespace UsersService.Application.Users.Commands.DeactivateUser
{
    public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IProductServiceClient _productService;

        public DeactivateUserCommandHandler(IUserRepository userRepository, IProductServiceClient productService)
        {
            _userRepository = userRepository;
            _productService = productService;
        }

        public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.IsActive = false;

            await _userRepository.UpdateUserAsync(user);

            await _productService.HideProductsByUserIdAsync(user.Id);
        }
    }
}