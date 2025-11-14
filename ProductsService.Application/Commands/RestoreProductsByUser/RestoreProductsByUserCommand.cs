using MediatR;

namespace ProductsService.Application.Commands.RestoreProductsByUser
{
    public record RestoreProductsByUserCommand(Guid UserId) : IRequest;
}