using MediatR;
using ProductsService.Application.Contracts;

namespace ProductsService.Application.Queries.GetProductById
{
    public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
}