using LoggingService;
using ProductsService.Application.Commands.CreateProduct;
using ProductsService.Application.Commands.UpdateProduct;
using ProductsService.Application.Interfaces;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Exceptions;

namespace ProductsService.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly ILoggingService _logger;

        public ProductService(IProductRepository repo, IUnitOfWork uow, ILoggingService logger)
        {
            _repo = repo;
            _uow = uow; 
            _logger = logger;
        }

        public async Task<Guid> CreateProductAsync(CreateProductCommand command, Guid userId)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = command.Title,
                Description = command.Description,
                Price = command.Price,
                IsAvailable = command.IsAvailable,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(product);
            await _uow.SaveChangesAsync();
            return product.Id;
        }

        public async Task UpdateProductAsync(UpdateProductCommand command, Guid userId)
        {
            var product = await _repo.GetByIdAsync(command.ProductId);

            if (product == null)
                throw new ProductNotFoundException("Product not found");

            if (product.UserId != userId)
                throw new AccessDeniedException("You do not have permission to update this product");

            product.Title = command.Title;
            product.Description = command.Description;
            product.Price = command.Price;
            product.IsAvailable = command.IsAvailable;

            await _repo.UpdateAsync(product);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Guid productId, Guid userId)
        {
            var product = await _repo.GetByIdAsync(productId);

            if (product == null)
                throw new ProductNotFoundException("Product not found");

            if (product.UserId != userId)
                throw new AccessDeniedException("You do not have permission to delete this product");

            product.IsDeleted = true;

            await _repo.UpdateAsync(product);
            await _uow.SaveChangesAsync();
        }
    }
}
