using MediatR;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Commands.CreateProduct
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductService _productService;
        private readonly ICurrentUserService _currentUser;

        public CreateProductHandler(IProductService productService, ICurrentUserService currentUser)
        {
            _productService = productService;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            return await _productService.CreateProductAsync(request, userId);
        }
    }
}