using FluentValidation;

namespace ProductsService.Application.Commands.HideProductsByUser
{
    public class HideProductsByUserCommandValidator : AbstractValidator<HideProductsByUserCommand>
    {
        public HideProductsByUserCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}