using Microsoft.AspNetCore.Mvc;
using ProductsService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace ProductsService.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                await HandleException(context, ex);
            }
        }

        private Task HandleException(HttpContext context, Exception ex)
        {
            var problem = new ProblemDetails();

            switch (ex)
            {
                case NotFoundException nf:
                    problem.Title = nf.Message;
                    problem.Status = (int)HttpStatusCode.NotFound;
                    break;

                case ForbiddenException fb:
                    problem.Title = fb.Message;
                    problem.Status = (int)HttpStatusCode.Forbidden;
                    break;

                case ApplicationValidationException ve:
                    problem.Title = "Validation error";
                    problem.Status = (int)HttpStatusCode.BadRequest;
                    problem.Extensions["errors"] = ve.Errors;
                    break;

                default:
                    problem.Title = "Internal server error";
                    problem.Status = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.StatusCode = problem.Status ?? 500;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
