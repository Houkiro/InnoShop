using MediatR;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Commands.RestoreProductsByUser
{
    public class RestoreProductsByUserHandler : IRequestHandler<RestoreProductsByUserCommand>
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;

        public RestoreProductsByUserHandler(IProductRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task Handle(RestoreProductsByUserCommand request, CancellationToken cancellationToken)
        {
            var products = await _repo.GetByUserIdAsync(request.UserId);

            if (products == null || !products.Any())
                return;

            foreach (var product in products)
                product.IsDeleted = false;

            await _uow.SaveChangesAsync();
        }
    }
}