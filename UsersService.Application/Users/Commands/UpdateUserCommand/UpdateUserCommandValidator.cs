using FluentValidation;

namespace UsersService.Application.Users.Commands.UpdateUserCommand
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Name));
        }
    }
}