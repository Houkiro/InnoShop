namespace UsersService.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        Task SendConfirmationEmailAsync(string toEmail, string userName, string confirmationToken);

        Task SendResetPasswordEmailAsync(string toEmail, string userName, string resetToken);
    }
}