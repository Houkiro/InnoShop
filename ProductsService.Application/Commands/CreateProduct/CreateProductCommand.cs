using MediatR;

namespace ProductsService.Application.Commands.CreateProduct
{
   public record CreateProductCommand(
        string Title,
        string Description,
        decimal Price,
        bool IsAvailable,
        Guid UserId
   ) : IRequest<Guid>;
}