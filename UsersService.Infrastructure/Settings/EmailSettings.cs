public class EmailSettings
{
    public string FromName { get; set; } = "InnoShop";
    public string From { get; set; } = "no-reply@innoshop.local";
    public string SmtpServer { get; set; } = "mailhog"; // имя сервиса в docker-compose
    public int Port { get; set; } = 1025;
    public string Username { get; set; } = ""; // не нужен
    public string Password { get; set; } = ""; // не нужен
}