using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Application.Commands.CreateProduct;
using ProductsService.Application.Commands.DeleteProduct;
using ProductsService.Application.Commands.UpdateProduct;
using ProductsService.Application.Queries.GetProductById;
using ProductsService.Application.Queries.GetProducts;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductCommand command)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            var id = await _mediator.Send(command with { UserId = userId });

            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id));
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateProductCommand command)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            await _mediator.Send(command with { ProductId = id, UserId = userId });

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            await _mediator.Send(new DeleteProductCommand(id, userId));

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetProductsQuery query)
        {
            var products = await _mediator.Send(query);
            return Ok(products);
        }
    }
}
