using MediatR;
using Microsoft.AspNetCore.Mvc;
using UsersService.Application.Users.Commands.CreateUser;

namespace UsersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }
    }
}