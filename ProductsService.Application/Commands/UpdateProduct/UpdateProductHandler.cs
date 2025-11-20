using MediatR;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Commands.UpdateProduct
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductService _productService;
        private readonly ICurrentUserService _currentUser;

        public UpdateProductHandler(IProductService productService, ICurrentUserService currentUser)
        {
            _productService = productService;
            _currentUser = currentUser;
        }

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            await _productService.UpdateProductAsync(request, userId);
        }
    }
}