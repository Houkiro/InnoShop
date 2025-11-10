using MediatR;

namespace ProductsService.Application.Commands.UpdateProduct
{
    public record UpdateProductCommand(
        Guid ProductId,
        string Title,
        string Description,
        decimal Price,
        bool IsAvailable,
        Guid UserId
    ) : IRequest;
}