using AutoMapper;
using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Infrastructure.Model;
using Fitness_Tracker_Infrastructure.Repository.User;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace Fitness_Tracker.Tests.Integration
{
    public class UserInfoRepositoryTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly UserInfoRepository _repository;
        private readonly IDatabase _redisDb;

        public UserInfoRepositoryTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserInformatonEntity, UserInformationDTO>();
            }, NullLoggerFactory.Instance);

            var mapper = config.CreateMapper();
            _repository = new UserInfoRepository(_fixture.DbContext, mapper, _fixture.RedisMultiplexer);
            _redisDb = _fixture.RedisMultiplexer.GetDatabase();
        }

        [Fact]
        public async Task GetUserInformationAsync_CacheHit_ShouldReturnDataDirectlyFromRedis()
        {
            var userId = Guid.NewGuid();
            string userCacheKey = $"user:{userId}";

            HashEntry[] entries =
            {
                new(nameof(UserInformationDTO.Login), "cached_login"),
                new(nameof(UserInformationDTO.Name), "Cached Name"),
                new(nameof(UserInformationDTO.Height), "175"),
                new(nameof(UserInformationDTO.Weight), "70.0")
            };
            await _redisDb.HashSetAsync(userCacheKey, entries);

            var result = await _repository.GetUserInformationAsync(userId, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Login.Should().Be("cached_login");
            result.Value.Name.Should().Be("Cached Name");
            result.Value.Height.Should().Be(175);
        }

        [Fact]
        public async Task GetUserInformationAsync_WhenNotFoundInDb_ShouldSetNegativeCacheInRedis()
        {
            var nonExistentUserId = Guid.NewGuid();
            string userCacheKey = $"user:{nonExistentUserId}";
            await _redisDb.KeyDeleteAsync(userCacheKey);

            var result1 = await _repository.GetUserInformationAsync(nonExistentUserId, CancellationToken.None);

            result1.IsFailed.Should().BeTrue();
            var negativeCache = await _redisDb.HashGetAsync(userCacheKey, "IsNotFound");
            negativeCache.ToString().Should().Be("true");

            var result2 = await _repository.GetUserInformationAsync(nonExistentUserId, CancellationToken.None);
            result2.IsFailed.Should().BeTrue();
            result2.Errors.First().Message.Should().Contain(nonExistentUserId.ToString());
        }

        [Fact]
        public async Task UpdateUserInformationAsync_ShouldUpdatePostgres_AndSyncExistingRedisCache()
        {
            var user = new UserEntity { Id = Guid.NewGuid(), Login = $"u_{Guid.NewGuid():N}"[..15], Password = "pwd" };
            var userInfo = new UserInformatonEntity
            {
                Id = user.Id,
                Name = "OldName",
                Height = 170,
                Weight = 70.0m,
                User = user
            };
            await _fixture.DbContext.Users.AddAsync(user);
            await _fixture.DbContext.UserInformation.AddAsync(userInfo);
            await _fixture.DbContext.SaveChangesAsync();

            string userCacheKey = $"user:{userInfo.Id}";
            await _redisDb.HashSetAsync(userCacheKey, new HashEntry[]
            {
                new(nameof(UserInformationDTO.Login), user.Login),
                new(nameof(UserInformationDTO.Name), "OldName"),
                new(nameof(UserInformationDTO.Height), 170)
            });

            var updateDto = new UserUpdateDTO(name: "NewName", birthDay: new DateOnly(1995, 5, 5), height: 175, weight: 75.0m);

            var updateResult = await _repository.UpdateUserInformationAsync(userInfo.Id, updateDto, CancellationToken.None);

            updateResult.IsSuccess.Should().BeTrue();

            var dbEntity = await _fixture.DbContext.UserInformation.AsNoTracking().FirstAsync(u => u.Id == userInfo.Id);
            dbEntity.Name.Should().Be("NewName");
            dbEntity.Height.Should().Be(175);
            dbEntity.Weight.Should().Be(75.0m);

            var redisName = await _redisDb.HashGetAsync(userCacheKey, nameof(UserInformationDTO.Name));
            var redisHeight = await _redisDb.HashGetAsync(userCacheKey, nameof(UserInformationDTO.Height));
            redisName.ToString().Should().Be("NewName");
            redisHeight.ToString().Should().Be("175");
        }
    }
}