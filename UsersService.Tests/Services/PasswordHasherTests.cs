using System;
using UsersService.Infrastructure.Services;
using Xunit;

namespace UsersService.Tests.Services
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new PasswordHasher();

        [Fact]
        public void Hash_ReturnsNonEmptyString()
        {
            var result = _hasher.Hash("mypassword");

            Assert.False(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void Verify_ReturnsTrue_ForCorrectPassword()
        {
            var password = "secure123";
            var hash = _hasher.Hash(password);

            var isValid = _hasher.Verify(password, hash);

            Assert.True(isValid);
        }

        [Fact]
        public void Verify_ReturnsFalse_ForIncorrectPassword()
        {
            var password = "secure123";
            var hash = _hasher.Hash(password);

            var isValid = _hasher.Verify("wrongpassword", hash);

            Assert.False(isValid);
        }

        [Fact]
        public void Hash_GeneratesDifferentHashes_ForSamePassword()
        {
            var password = "secure123";
            var hash1 = _hasher.Hash(password);
            var hash2 = _hasher.Hash(password);

            Assert.NotEqual(hash1, hash2); 
            Assert.True(_hasher.Verify(password, hash1));
            Assert.True(_hasher.Verify(password, hash2));
        }
    }
}