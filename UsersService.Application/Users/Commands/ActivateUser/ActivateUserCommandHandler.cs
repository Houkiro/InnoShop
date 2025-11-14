using MediatR;
using Microsoft.Extensions.Logging;
using UsersService.Application.Interfaces;
using UsersService.Application.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    private readonly IProductServiceClient _productService;
    private readonly ILogger<ActivateUserCommandHandler> _logger;

    public ActivateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork uow,
        IProductServiceClient productService,
        ILogger<ActivateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _uow = uow;
        _productService = productService;
        _logger = logger;
    }

    public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.IsActive = true;

        await _userRepository.UpdateAsync(user); 
        await _uow.SaveChangesAsync(); 

        try
        {
            await _productService.RestoreProductsByUserIdAsync(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring products for user {UserId}", user.Id);
        }
    }
}