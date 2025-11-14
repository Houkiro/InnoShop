using MediatR;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Commands.DeleteProduct
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;

        public DeleteProductHandler(IProductRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repo.GetByIdAsync(request.ProductId);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            if (product.UserId != request.UserId)
                throw new UnauthorizedAccessException("You do not own this product");

            product.IsDeleted = true;

            await _repo.UpdateAsync(product);
            await _uow.SaveChangesAsync();
        }
    }
}