using MediatR;
using UsersService.Application.Interfaces;

namespace UsersService.Application.Users.Commands.DeactivateUser
{
    public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductServiceClient _productService;

        public DeactivateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IProductServiceClient productService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _productService = productService;
        }

        public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.IsActive = false;

            _userRepository.UpdateAsync(user);

            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _productService.HideProductsByUserIdAsync(user.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding products: {ex.Message}");
            }
        }
    }
}
