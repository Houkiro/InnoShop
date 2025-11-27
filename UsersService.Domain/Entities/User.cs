namespace UsersService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; } = Roles.User;
        public bool IsActive { get; set; } = false;
        public bool IsEmailConfirmed { get; set; } = false;
        public string? ConfirmationToken { get; set; }
        public DateTime? ConfirmationTokenExpires { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
    }
}