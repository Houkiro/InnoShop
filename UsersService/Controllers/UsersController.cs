using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UsersService.Application.Contracts;
using UsersService.Application.Queries.GetCurrentUser;
using UsersService.Application.Users.Commands.ActivateUser;
using UsersService.Application.Users.Commands.ConfirmUser;
using UsersService.Application.Users.Commands.CreateUser;
using UsersService.Application.Users.Commands.DeactivateUser;
using UsersService.Application.Users.Commands.ForgotPassword;
using UsersService.Application.Users.Commands.LoginUserCommand;
using UsersService.Application.Users.Commands.ResetPassword;
using UsersService.Application.Users.Commands.UpdateUserCommand;
using UsersService.Domain.Exceptions;

namespace UsersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator) => _mediator = mediator;

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token payload");

            var result = await _mediator.Send(new GetCurrentUserQuery(userId));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HttpPost("deactivate")]
        [Authorize]
        public async Task<IActionResult> Deactivate()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _mediator.Send(new DeactivateUserCommand(userId));

            return NoContent();
        }

        [HttpPost("activate")]
        [Authorize]
        public async Task<IActionResult> Activate()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _mediator.Send(new ActivateUserCommand(userId));

            return NoContent();
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> Confirm([FromQuery] string token)
        {
            try
            {
                await _mediator.Send(new ConfirmUserCommand(token));
                return Ok("Аккаунт подтверждён.");
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _mediator.Send(new ForgotPasswordCommand(dto.Email));
            return Ok("Ссылка для сброса пароля отправлена на email.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _mediator.Send(new ResetPasswordCommand(dto.Token, dto.NewPassword));
            return Ok("Пароль успешно изменён.");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginUserCommand command)
        {
            var token = await _mediator.Send(command);
            return Ok(new { Token = token });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserCommand command)
        {
            if (id != command.UserId)
                return BadRequest("UserId mismatch");

            await _mediator.Send(command);
            return NoContent();
        }

    }
}