using MediatR;

namespace ProductsService.Application.Commands.DeleteProduct
{
    public record DeleteProductCommand(Guid ProductId, Guid UserId) : IRequest;
}