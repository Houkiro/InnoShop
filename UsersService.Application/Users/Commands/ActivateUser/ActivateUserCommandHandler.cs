using MediatR;
using UsersService.Application.Interfaces;

namespace UsersService.Application.Users.Commands.ActivateUser
{
    public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IProductServiceClient _productService;

        public ActivateUserCommandHandler(IUserRepository userRepository, IProductServiceClient productService)
        {
            _userRepository = userRepository;
            _productService = productService;
        }

        public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.IsActive = true;
            
            await _userRepository.UpdateUserAsync(user);
            await _productService.RestoreProductsByUserIdAsync(user.Id);
        }
    }
}