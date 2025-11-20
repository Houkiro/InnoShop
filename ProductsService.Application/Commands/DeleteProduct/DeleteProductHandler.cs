using MediatR;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Commands.DeleteProduct
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly IProductService _productService;
        private readonly ICurrentUserService _currentUser;

        public DeleteProductHandler(IProductService productService, ICurrentUserService currentUser)
        {
            _productService = productService;
            _currentUser = currentUser;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            await _productService.DeleteProductAsync(request.ProductId, userId);
        }
    }
}