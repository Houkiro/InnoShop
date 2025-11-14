using MediatR;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;

namespace ProductsService.Application.Commands.CreateProduct
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public CreateProductHandler(IProductRepository repo, IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _repo = repo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                UserId = _currentUser.UserId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                IsAvailable = request.IsAvailable,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(product);
            await _uow.SaveChangesAsync();

            return product.Id;
        }
    }
}