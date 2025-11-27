using Microsoft.EntityFrameworkCore;
using UsersService.Domain.Entities;
using UsersService.Infrastructure.Persistence;
using UsersService.Infrastructure.Repositories;

namespace UsersService.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private UserDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new UserDbContext(options);
        }

        [Fact]
        public async Task AddAsync_AddsUserToDatabase()
        {
            using var db = CreateDbContext();
            var repo = new UserRepository(db);

            var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", Name = "Test" };

            await repo.AddAsync(user);
            await db.SaveChangesAsync();

            var saved = await db.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");
            Assert.NotNull(saved);
            Assert.Equal("Test", saved.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenExists()
        {
            using var db = CreateDbContext();
            var repo = new UserRepository(db);

            var user = new User { Id = Guid.NewGuid(), Email = "id@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var result = await repo.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsUser_WhenExists()
        {
            using var db = CreateDbContext();
            var repo = new UserRepository(db);

            var user = new User { Id = Guid.NewGuid(), Email = "email@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var result = await repo.GetByEmailAsync("email@test.com");

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Fact]
        public async Task EmailExistsAsync_ReturnsTrue_WhenEmailExists()
        {
            using var db = CreateDbContext();
            var repo = new UserRepository(db);

            var user = new User { Id = Guid.NewGuid(), Email = "exists@test.com" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var exists = await repo.EmailExistsAsync("exists@test.com");

            Assert.True(exists);
        }

        [Fact]
        public async Task GetByConfirmationTokenAsync_ReturnsUser_WhenTokenMatches()
        {
            using var db = CreateDbContext();
            var repo = new UserRepository(db);

            var user = new User { Id = Guid.NewGuid(), ConfirmationToken = "token123" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var result = await repo.GetByConfirmationTokenAsync("token123");

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Fact]
        public async Task GetByResetPasswordTokenAsync_ReturnsUser_WhenTokenMatches()
        {
            using var db = CreateDbContext();
            var repo = new UserRepository(db);

            var user = new User { Id = Guid.NewGuid(), PasswordResetToken = "reset123" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var result = await repo.GetByResetPasswordTokenAsync("reset123");

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesUserFields()
        {
            using var db = CreateDbContext();
            var repo = new UserRepository(db);

            var user = new User { Id = Guid.NewGuid(), Email = "update@test.com", Name = "OldName" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            user.Name = "NewName";
            await repo.UpdateAsync(user);
            await db.SaveChangesAsync();

            var updated = await db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.Equal("NewName", updated!.Name);
        }
    }
}
