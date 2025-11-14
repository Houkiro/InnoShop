using MediatR;
using ProductsService.Application.Interfaces;

namespace ProductsService.Application.Commands.HideProductsByUser
{
    public class HideProductsByUserHandler : IRequestHandler<HideProductsByUserCommand>
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;

        public HideProductsByUserHandler(IProductRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task Handle(HideProductsByUserCommand request, CancellationToken cancellationToken)
        {
            var products = await _repo.GetByUserIdAsync(request.UserId);

            foreach (var product in products)
                product.IsDeleted = true;

            await _uow.SaveChangesAsync();
        }
    }
}