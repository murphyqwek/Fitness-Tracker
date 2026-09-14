using Fitness_Tracker_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Security.Cryptography;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using AnalyticsDbContext = Fintess_Tracker_Analytics.Data.ApplicationDbContext;
using FitnessDbContext = Fitness_Tracker_Infrastructure.Data.ApplicationDbContext;

namespace Fitness_Tracker.Tests.Integration;

public class TestDatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer MainPostgreContainer { get; }

    public PostgreSqlContainer AnalyticsPostgresContainer { get; }

    public RedisContainer RedisContainer { get; }

    public KafkaContainer KafkaContainer { get; }

    public IConnectionMultiplexer RedisMultiplexer { get; private set; } = null!;

    public FitnessDbContext DbContext { get; private set; } = null!;
    public AnalyticsDbContext AnalyticsDbContext { get; private set; } = null!;


    public string JwtPublicKeyPem { get; private set; } = null!;

    public TestDatabaseFixture()
    {
        MainPostgreContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("fitness_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        AnalyticsPostgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("analytics_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        RedisContainer = new RedisBuilder()
            .WithImage("redis/redis-stack:latest")
            .Build();

        KafkaContainer = new KafkaBuilder()
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            MainPostgreContainer.StartAsync(),
            AnalyticsPostgresContainer.StartAsync(),
            RedisContainer.StartAsync(),
            KafkaContainer.StartAsync());

        using var rsa = RSA.Create(2048);
        JwtPublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();

        var fitnessOptions =
            new DbContextOptionsBuilder<FitnessDbContext>()
                .UseNpgsql(
                    MainPostgreContainer.GetConnectionString())
                .Options;

        DbContext = new FitnessDbContext(fitnessOptions);

        await DbContext.Database.EnsureCreatedAsync();

        var analyticsOptions =
            new DbContextOptionsBuilder<AnalyticsDbContext>()
                .UseNpgsql(
                    AnalyticsPostgresContainer.GetConnectionString())
                .Options;

        AnalyticsDbContext =
            new AnalyticsDbContext(analyticsOptions);

        await AnalyticsDbContext.Database.EnsureCreatedAsync();

        RedisMultiplexer =
            await ConnectionMultiplexer.ConnectAsync(
                RedisContainer.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await AnalyticsDbContext.DisposeAsync();

        RedisMultiplexer.Dispose();

        await Task.WhenAll(
            MainPostgreContainer.DisposeAsync().AsTask(),
            AnalyticsPostgresContainer.DisposeAsync().AsTask(),
            RedisContainer.DisposeAsync().AsTask(),
            KafkaContainer.DisposeAsync().AsTask());
    }
}