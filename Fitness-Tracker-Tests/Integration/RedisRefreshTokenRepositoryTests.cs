using Fitness_Tracker_Infrastructure.Repository.Refresh;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace Fitness_Tracker.Tests.Integration
{
    public class RedisRefreshTokenRepositoryTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly RedisRefreshTokenRepository _repository;
        private readonly IDatabase _redisDb;

        public RedisRefreshTokenRepositoryTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            _repository = new RedisRefreshTokenRepository(_fixture.RedisMultiplexer);
            _redisDb = _fixture.RedisMultiplexer.GetDatabase();
        }

        [Fact]
        public async Task AddNewRefreshTokenAsync_ShouldSaveTokenInRedis_AndCheckShouldReturnUserId()
        {
            var refreshToken = $"token_{Guid.NewGuid():N}";
            var userId = Guid.NewGuid();
            var expiry = TimeSpan.FromMinutes(5);

            await _repository.AddNewRefreshTokenAsync(refreshToken, userId, expiry, CancellationToken.None);
            var result = await _repository.CheckRefreshTokenAsync(refreshToken, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(userId);

            var ttl = await _redisDb.KeyTimeToLiveAsync(refreshToken);
            ttl.Should().NotBeNull();
            ttl!.Value.TotalSeconds.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CheckRefreshTokenAsync_WhenTokenDoesNotExist_ShouldReturnFailure()
        {
            var nonExistentToken = $"not_exists_{Guid.NewGuid():N}";

            var result = await _repository.CheckRefreshTokenAsync(nonExistentToken, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Contain("expired or not found");
        }

        [Fact]
        public async Task CheckRefreshTokenAsync_WhenDataInRedisIsNotGuid_ShouldReturnFailure()
        {
            var corruptedToken = $"corrupted_{Guid.NewGuid():N}";
            await _redisDb.StringSetAsync(corruptedToken, "not-a-valid-guid-string");

            var result = await _repository.CheckRefreshTokenAsync(corruptedToken, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Contain("Invalid user ID format");
        }

        [Fact]
        public async Task RemoveRefreshTokenAsync_ShouldDeleteTokenFromRedis()
        {
            var refreshToken = $"token_to_remove_{Guid.NewGuid():N}";
            var userId = Guid.NewGuid();
            await _repository.AddNewRefreshTokenAsync(refreshToken, userId, TimeSpan.FromMinutes(5), CancellationToken.None);

            await _repository.RemoveRefreshTokenAsync(refreshToken, CancellationToken.None);

            var exists = await _redisDb.KeyExistsAsync(refreshToken);
            exists.Should().BeFalse();

            var checkResult = await _repository.CheckRefreshTokenAsync(refreshToken, CancellationToken.None);
            checkResult.IsFailed.Should().BeTrue();
        }
    }
}