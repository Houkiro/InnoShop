using System.Security.Cryptography;
using System.Text;
using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public bool Verify(string password, string hash) => Hash(password) == hash;
    }
}
