using Fitness_Tracker_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Fitness_Tracker.Tests.Integration
{
    public class TestDatabaseFixture : IAsyncLifetime
    {
        public PostgreSqlContainer PostgresContainer { get; }
        public RedisContainer RedisContainer { get; }

        public ApplicationDbContext DbContext { get; private set; } = null!;
        public IConnectionMultiplexer RedisMultiplexer { get; private set; } = null!;

        public TestDatabaseFixture()
        {
            PostgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("fitness_test_db")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            RedisContainer = new RedisBuilder()
                .WithImage("redis/redis-stack:latest")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await Task.WhenAll(PostgresContainer.StartAsync(), RedisContainer.StartAsync());
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(PostgresContainer.GetConnectionString())
                .Options;

            DbContext = new ApplicationDbContext(options);
            await DbContext.Database.EnsureCreatedAsync();
            RedisMultiplexer = await ConnectionMultiplexer.ConnectAsync(RedisContainer.GetConnectionString());
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
            RedisMultiplexer.Dispose();
            await Task.WhenAll(PostgresContainer.DisposeAsync().AsTask(), RedisContainer.DisposeAsync().AsTask());
        }
    }
}