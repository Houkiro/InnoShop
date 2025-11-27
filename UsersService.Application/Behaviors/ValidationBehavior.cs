using FluentValidation;
using MediatR;

namespace UsersService.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if(_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var errors = _validators
                    .Select(v => v.Validate(context))
                    .SelectMany(v => v.Errors)
                    .Where(f =>  f != null)
                    .ToList();
                
                if(errors.Count != 0)
                {
                    var messages = errors.Select(e => e.ErrorMessage).ToArray();
                    throw new ValidationException(string.Join("; ", messages));
                }
            }

            return await next();
        }
    }
}