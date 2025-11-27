using FluentValidation;

namespace UsersService.Application.Users.Commands.ChangeUserRole
{
    public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
    {
        public ChangeUserRoleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.NewRole)
                .NotEmpty()
                .WithMessage("NewRole is required.")
                .Must(role => role == "User" || role == "Admin")
                .WithMessage("NewRole must be either 'User' or 'Admin'.");
        }
    }
}