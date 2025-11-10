using MediatR;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Commands.UpdateProduct
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductRepository _repo;

        public UpdateProductHandler(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repo.GetByIdAsync(request.ProductId);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            if (product.UserId != request.UserId)
                throw new UnauthorizedAccessException("You do not own this product");

            product.Title = request.Title;
            product.Description = request.Description;
            product.Price = request.Price;
            product.IsAvailable = request.IsAvailable;

            await _repo.UpdateAsync(product);
        }
    }

}
