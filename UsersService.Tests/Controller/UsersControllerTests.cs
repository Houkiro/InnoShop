using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using UsersService.Application.Contracts;
using UsersService.Application.Queries.GetCurrentUser;
using UsersService.Application.Users.Commands.ActivateUser;
using UsersService.Application.Users.Commands.ChangeUserRole;
using UsersService.Application.Users.Commands.ConfirmUser;
using UsersService.Application.Users.Commands.DeactivateUser;
using UsersService.Application.Users.Commands.LoginUserCommand;
using UsersService.Application.Users.Commands.RegisterUser;
using UsersService.Application.Users.Commands.UpdateUserCommand;
using UsersService.Controllers;
using UsersService.Domain.Exceptions;
using Xunit;

namespace UsersService.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new UsersController(_mediatorMock.Object);
        }

        private void SetUser(Guid userId, string claimType = ClaimTypes.NameIdentifier)
        {
            var claims = new[] { new Claim(claimType, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsOk_WhenValidGuid()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);

            var expected = new UserDto { Id = userId, Email = "test@test.com" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), default))
                .ReturnsAsync(expected);

            var result = await _controller.GetCurrentUser();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsUnauthorized_WhenInvalidGuid()
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var result = await _controller.GetCurrentUser();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid token payload", unauthorized.Value);
        }

        [Fact]
        public async Task Register_ReturnsOk_WithId()
        {
            var command = new RegisterUserCommand("name", "email@test.com", "password");
            var newId = Guid.NewGuid();
            _mediatorMock.Setup(m => m.Send(command, default)).ReturnsAsync(newId);

            var result = await _controller.Register(command);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var value = okResult.Value;
            var idProp = value!.GetType().GetProperty("id")!.GetValue(value, null);

            Assert.Equal(newId, idProp);
        }

        [Fact]
        public async Task Deactivate_ReturnsNoContent()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);

            var result = await _controller.Deactivate();

            Assert.IsType<NoContentResult>(result);
            _mediatorMock.Verify(m => m.Send(It.Is<DeactivateUserCommand>(c => c.UserId == userId), default), Times.Once);
        }

        [Fact]
        public async Task Activate_ReturnsNoContent()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);

            var result = await _controller.Activate();

            Assert.IsType<NoContentResult>(result);
            _mediatorMock.Verify(m => m.Send(It.Is<ActivateUserCommand>(c => c.UserId == userId), default), Times.Once);
        }

        [Fact]
        public async Task Confirm_ReturnsOk_WhenValid()
        {
            var token = "valid-token";

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ConfirmUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.Confirm(token);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Аккаунт подтверждён.", okResult.Value);
        }


        [Fact]
        public async Task Confirm_ReturnsBadRequest_WhenException()
        {
            var token = "bad-token";
            _mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmUserCommand>(), default))
                .ThrowsAsync(new BadRequestException("Invalid"));

            var result = await _controller.Confirm(token);

            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid", badResult.Value);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsOk()
        {
            var dto = new ForgotPasswordDto("email@test.com");

            var result = await _controller.ForgotPassword(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Ссылка для сброса пароля отправлена на email.", okResult.Value);
        }

        [Fact]
        public async Task ResetPassword_ReturnsOk()
        {
            var dto = new ResetPasswordDto("token", "newpass");

            var result = await _controller.ResetPassword(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Пароль успешно изменён.", okResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsToken()
        {
            var command = new LoginUserCommand { Email = "email@test.com", Password = "password" };
            _mediatorMock.Setup(m => m.Send(command, default)).ReturnsAsync("jwt-token");

            var result = await _controller.Login(command);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var value = okResult.Value;
            var tokenProp = value!.GetType().GetProperty("Token")!.GetValue(value, null);

            Assert.Equal("jwt-token", tokenProp);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent()
        {
            var id = Guid.NewGuid();
            var command = new UpdateUserCommand { Email = "newemail@test.com", Name = "newname" };

            var result = await _controller.UpdateUser(id, command);

            Assert.IsType<NoContentResult>(result);
            _mediatorMock.Verify(m => m.Send(command, default), Times.Once);
        }

        [Fact]
        public async Task ChangeUserRole_ReturnsNoContent()
        {
            var userId = Guid.NewGuid();
            var newRole = "Admin";

            var result = await _controller.ChangeUserRole(userId, newRole);

            Assert.IsType<NoContentResult>(result);
            _mediatorMock.Verify(m => m.Send(It.Is<ChangeUserRoleCommand>(c => c.UserId == userId && c.NewRole == newRole), default), Times.Once);
        }
    }
}
