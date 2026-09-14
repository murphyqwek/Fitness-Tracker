using Fitness_Tracker.Tests.Integration;
using Fitness_Tracker_Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fitness_Tracker_Tests.Integration.Factory;

public sealed class FitnessTrackerFactory
    : WebApplicationFactory<Fitness_Tracker_Api.Program>
{
    private readonly TestDatabaseFixture _fixture;

    public FitnessTrackerFactory(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(
                    _fixture.MainPostgreContainer.GetConnectionString());
            });
        });

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Kafka:BootstrapServers"] = _fixture.KafkaContainer.GetBootstrapAddress(),

                    ["ConnectionStrings:Redis"] = _fixture.RedisContainer.GetConnectionString(),

                    ["Jwt:PublicKeyPem"] = _fixture.JwtPublicKeyPem
                });
        });
    }
}