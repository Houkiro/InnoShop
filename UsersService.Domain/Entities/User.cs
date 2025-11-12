namespace UsersService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; } = "User";
        public bool IsActive { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string? ConfirmationToken { get; set; }
        public DateTime? ConfirmationTokenExpires { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}