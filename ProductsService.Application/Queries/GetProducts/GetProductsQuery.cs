using MediatR;
using ProductsService.Application.Contracts;

namespace ProductsService.Application.Queries.GetProducts
{
    public record GetProductsQuery(
        string? Search,
        decimal? MinPrice,
        decimal? MaxPrice,
        bool? IsAvailable,
        int Page = 1,
        int PageSize = 20,
        string? SortBy = null,
        bool Desc = false
    ) : IRequest<List<ProductDto>>;
}