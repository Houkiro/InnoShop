using Microsoft.AspNetCore.Mvc;
using ProductsService.Application.Interfaces;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class InternalController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public InternalController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpPost("hide-by-user/{userId}")]
        public async Task<IActionResult> HideProducts(Guid userId)
        {
            await _productRepository.HideProductsByUserIdAsync(userId);

            return NoContent();
        }

        [HttpPost("restore-by-user/{userId}")]
        public async Task<IActionResult> RestoreProducts(Guid userId)
        {
            await _productRepository.RestoreProductsByUserIdAsync(userId);

            return NoContent();
        }
    }
}
