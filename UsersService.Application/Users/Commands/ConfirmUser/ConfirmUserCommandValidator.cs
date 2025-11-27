using FluentValidation;

namespace UsersService.Application.Users.Commands.ConfirmUser
{
    public class ConfirmUserCommandValidator : AbstractValidator<ConfirmUserCommand>
    {
        public ConfirmUserCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Confirmation token is required.")
                .MaximumLength(512)
                .WithMessage("Confirmation token is too long.");
        }
    }
}