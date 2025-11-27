namespace UsersService.Application.Contracts
{
    public record ResetPasswordDto(string Token, string NewPassword);
}