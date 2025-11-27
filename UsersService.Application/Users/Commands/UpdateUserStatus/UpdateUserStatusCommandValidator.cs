using FluentValidation;

namespace UsersService.Application.Users.Commands.UpdateUserStatus
{
    public class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
    {
        public UpdateUserStatusCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.IsActive)
                .NotNull()
                .WithMessage("IsActive flag must be specified.");
        }
    }
}