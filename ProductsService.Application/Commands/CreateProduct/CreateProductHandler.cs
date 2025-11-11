using MediatR;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;

namespace ProductsService.Application.Commands.CreateProduct
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _repo;
        private readonly ICurrentUserService _currentUser;

        public CreateProductHandler(IProductRepository repo, ICurrentUserService currentUser)
        {
            _repo = repo;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                UserId = _currentUser.UserId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                IsAvailable = request.IsAvailable,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(product);
            return product.Id;
        }
    }
}