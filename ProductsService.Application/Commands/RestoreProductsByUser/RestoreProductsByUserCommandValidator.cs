using FluentValidation;

namespace ProductsService.Application.Commands.RestoreProductsByUser
{
    public class RestoreProductsByUserCommandValidator : AbstractValidator<RestoreProductsByUserCommand>
    {
        public RestoreProductsByUserCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}