using MediatR;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;

namespace ProductsService.Application.Commands.CreateProduct
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _repo;

        public CreateProductHandler(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
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