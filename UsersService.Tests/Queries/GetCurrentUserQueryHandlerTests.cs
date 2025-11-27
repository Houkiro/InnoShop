using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsersService.Application.Contracts;
using UsersService.Application.Interfaces;
using UsersService.Application.Queries.GetCurrentUser;
using UsersService.Domain.Entities;
using Xunit;

namespace UsersService.Tests.Queries
{
    public class GetCurrentUserQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly GetCurrentUserQueryHandler _handler;

        public GetCurrentUserQueryHandlerTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _handler = new GetCurrentUserQueryHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsUserDto_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@test.com",
                Role = "User",
                IsActive = true
            };

            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            var query = new GetCurrentUserQuery(userId);
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("Test User", result.Name);
            Assert.Equal("test@test.com", result.Email);
            Assert.Equal("User", result.Role);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task Handle_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var query = new GetCurrentUserQuery(userId);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }
    }
}