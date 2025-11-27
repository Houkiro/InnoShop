public class EmailSettings
{
    public string FromName { get; set; } = "InnoShop";
    public string From { get; set; } = "no-reply@innoshop.local";
    public string SmtpServer { get; set; } = "mailhog"; 
    public int Port { get; set; } = 1025;
    public string Username { get; set; } = ""; 
    public string Password { get; set; } = ""; 

    public string ConfirmUrl { get; set; } = "http://localhost:5000/api/users/confirm";
    public string ResetUrl { get; set; } = "http://localhost:5000/api/users/reset-password";

    public bool UseTls { get; set; } = false;
}