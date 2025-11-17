using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsService.Application.Commands.HideProductsByUser;
using ProductsService.Application.Commands.RestoreProductsByUser;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/internal/products")] // отдельный internal маршрут, чтобы не конфликтовать с публичными
    [Authorize(AuthenticationSchemes = "Service")] // требует X-Service-Auth
    public class InternalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InternalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("hide-by-user/{userId}")]
        public async Task<IActionResult> HideProducts(Guid userId)
        {
            await _mediator.Send(new HideProductsByUserCommand(userId));
            return NoContent();
        }

        [HttpPost("restore-by-user/{userId}")]
        public async Task<IActionResult> RestoreProducts(Guid userId)
        {
            await _mediator.Send(new RestoreProductsByUserCommand(userId));
            return NoContent();
        }
    }
}