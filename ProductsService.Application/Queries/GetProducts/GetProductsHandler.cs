using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductsService.Application.Contracts;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Queries.GetProducts
{
    public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly IProductRepository _repo;

        public GetProductsHandler(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _repo.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = $"%{request.Search}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Title, term) ||
                    EF.Functions.Like(p.Description, term));
            }

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice.Value);

            if (request.IsAvailable.HasValue)
                query = query.Where(p => p.IsAvailable == request.IsAvailable.Value);

            query = request.SortBy?.ToLower() switch
            {
                "price" => request.Desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "createdat" => request.Desc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                "title" => request.Desc ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
                _ => query.OrderBy(p => p.Id)
            };

            query = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            var products = await query.ToListAsync(cancellationToken);

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                CreatedAt = p.CreatedAt
            }).ToList();
        }
    }
}