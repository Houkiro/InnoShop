using MediatR;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Commands.UpdateProduct
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UpdateProductHandler(IProductRepository repo, IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _repo = repo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repo.GetByIdAsync(request.ProductId);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            if (product.UserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("You do not own this product");

            product.Title = request.Title;
            product.Description = request.Description;
            product.Price = request.Price;
            product.IsAvailable = request.IsAvailable;

            await _repo.UpdateAsync(product);
            await _uow.SaveChangesAsync();
        }
    }
}