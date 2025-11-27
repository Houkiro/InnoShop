using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductsService.Domain.Exceptions;
using ProductsService.Middleware;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ProductsService.Tests.Middleware
{
    public class ErrorHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;

        public ErrorHandlingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        }

        private static HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            return context;
        }

        private async Task<string> InvokeMiddlewareWithException(Exception ex)
        {
            var context = CreateHttpContext();

            RequestDelegate next = _ => throw ex;

            var middleware = new ErrorHandlingMiddleware(next, _loggerMock.Object);
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            return await reader.ReadToEndAsync();
        }

        [Fact]
        public async Task Invoke_ShouldReturnNotFound_WhenNotFoundExceptionThrown()
        {
            var json = await InvokeMiddlewareWithException(new NotFoundException("Not found"));

            var problem = JsonSerializer.Deserialize<ProblemDetails>(json);
            Assert.Equal(404, problem.Status);
            Assert.Equal("Not found", problem.Title);
        }

        [Fact]
        public async Task Invoke_ShouldReturnForbidden_WhenForbiddenExceptionThrown()
        {
            var json = await InvokeMiddlewareWithException(new ForbiddenException("Forbidden"));

            var problem = JsonSerializer.Deserialize<ProblemDetails>(json);
            Assert.Equal(403, problem.Status);
            Assert.Equal("Forbidden", problem.Title);
        }

        [Fact]
        public async Task Invoke_ShouldReturnBadRequest_WhenValidationExceptionThrown()
        {
            var errors = new Dictionary<string, string[]> { { "Field", new[] { "Error" } } };
            var json = await InvokeMiddlewareWithException(new ApplicationValidationException(errors));

            var problem = JsonSerializer.Deserialize<ProblemDetails>(json);
            Assert.Equal(400, problem.Status);
            Assert.Equal("Validation error", problem.Title);
            Assert.True(problem.Extensions.ContainsKey("errors"));
        }

        [Fact]
        public async Task Invoke_ShouldReturnInternalServerError_WhenUnhandledExceptionThrown()
        {
            var json = await InvokeMiddlewareWithException(new Exception("Boom"));

            var problem = JsonSerializer.Deserialize<ProblemDetails>(json);
            Assert.Equal(500, problem.Status);
            Assert.Equal("Internal server error", problem.Title);
        }
    }
}