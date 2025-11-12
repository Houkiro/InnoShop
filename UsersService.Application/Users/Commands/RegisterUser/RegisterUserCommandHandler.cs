using MediatR;
using UsersService.Application.Interfaces;
using UsersService.Domain.Entities;

namespace UsersService.Application.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly IUserRepository _repo;
        private readonly IEmailService _emailService;

        public RegisterUserCommandHandler(IUserRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = false,
                ConfirmationToken = Guid.NewGuid().ToString(),
                ConfirmationTokenExpires = DateTime.UtcNow.AddHours(24)
            };

            await _repo.AddUserAsync(user);
            await _repo.SaveChangesAsync();

            var confirmUrl = $"https://localhost:7239/api/users/confirm?token={user.ConfirmationToken}";
            var html = $"<p>Привет {user.Name},</p><p>Подтвердите аккаунт: <a href='{confirmUrl}'>Активировать</a></p>";

            await _emailService.SendEmailAsync(user.Email, "Подтверждение аккаунта", html);

            return user.Id;
        }
    }
}
