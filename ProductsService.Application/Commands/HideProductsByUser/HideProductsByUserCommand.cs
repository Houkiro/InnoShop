using MediatR;

namespace ProductsService.Application.Commands.HideProductsByUser
{
    public record HideProductsByUserCommand(Guid UserId) : IRequest;
}