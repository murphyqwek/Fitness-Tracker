using Fitness_Tracker.Tests.Integration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AnalyticsDbContext = Fintess_Tracker_Analytics.Data.ApplicationDbContext;

public sealed class AnalyticsFactory : WebApplicationFactory<Fintess_Tracker_Analytics.Program>
{
    private readonly TestDatabaseFixture _fixture;

    public AnalyticsFactory(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AnalyticsDbContext>>();

            services.AddDbContext<AnalyticsDbContext>(options =>
            {
                options.UseNpgsql(
                    _fixture.AnalyticsPostgresContainer.GetConnectionString());
            });
        });

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Kafka:BootstrapServers"] =_fixture.KafkaContainer.GetBootstrapAddress(),
                    ["Kafka:GroupId"] = $"analytics-test-{Guid.NewGuid()}",
                    ["ConnectionStrings:Redis"] = _fixture.RedisContainer.GetConnectionString(),

                    ["Jwt:PublicKeyPem"] = _fixture.JwtPublicKeyPem,
                });
        });
    }
}