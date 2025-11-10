using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Application.Contracts;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Queries.GetProductById
{
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        private readonly IProductRepository _repo;

        public GetProductByIdHandler(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _repo
                .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            return new ProductDto
            {
                Id = product.Id,
                UserId = product.UserId,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                IsAvailable = product.IsAvailable,
                CreatedAt = product.CreatedAt
            };
        }
    }
}
