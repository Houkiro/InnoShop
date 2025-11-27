using MediatR;
using Microsoft.Extensions.Logging;
using ProductsService.Application.Commands.HideProductsByUser;
using ProductsService.Application.Interfaces;

public class HideProductsByUserHandler : IRequestHandler<HideProductsByUserCommand>
{
    private readonly IProductRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<HideProductsByUserHandler> _logger;

    public HideProductsByUserHandler(IProductRepository repo, IUnitOfWork uow, ILogger<HideProductsByUserHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task Handle(HideProductsByUserCommand request, CancellationToken cancellationToken)
    {
        var products = await _repo.GetByUserIdAsync(request.UserId);

        if (products == null || !products.Any())
        {
            _logger.LogWarning("No products found for user with ID: {UserId}", request.UserId);
        }

        foreach (var product in products)
        {
            product.IsDeleted = true;
            _logger.LogInformation("Hiding product with ID: {ProductId}", product.Id);
        }

        await _uow.SaveChangesAsync();
    }
}