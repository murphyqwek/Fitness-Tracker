using Fintess_Tracker_Analytics.Data;
using Fintess_Tracker_Analytics.Data.Entity;
using Fintess_Tracker_Analytics.Repository;
using Fitness_Tracker.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fitness_Tracker_Tests.Integration.Workout;

[Collection("Integration")]
public sealed class AnalyticRepositoryTests
{
    private readonly TestDatabaseFixture _fixture;

    public AnalyticRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetWeeklyRecord_WhenAnalyticsAreMissing_ReturnsZero()
    {
        await using var context = CreateContext();
        var repository = new AnalyticRepository(context, NullLogger<AnalyticRepository>.Instance);

        var result = await repository.GetWeeklyRecordAsync(
            Guid.NewGuid(), new DateOnly(2026, 11, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.Record);
    }

    [Fact]
    public async Task GetWeeklyRecord_ReturnsMaximumForUserWithinRequestedWeek()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var start = new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
        context.Workouts.AddRange(
            Workout(userId, start, 100m),
            Workout(userId, start.AddDays(7).AddMilliseconds(-1), 150m),
            Workout(userId, start.AddMilliseconds(-1), 900m),
            Workout(userId, start.AddDays(7), 1000m),
            Workout(Guid.NewGuid(), start.AddDays(1), 1100m));
        await context.SaveChangesAsync();
        var repository = new AnalyticRepository(context, NullLogger<AnalyticRepository>.Instance);

        var result = await repository.GetWeeklyRecordAsync(
            userId, new DateOnly(2026, 11, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(150m, result.Value.Record);
    }

    private ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_fixture.AnalyticsPostgresContainer.GetConnectionString())
            .Options);

    private static WorkoutAnalyticsEntity Workout(Guid userId, DateTimeOffset completedAt, decimal record) => new()
    {
        WorkoutId = Guid.NewGuid(),
        UserId = userId,
        CompletedAt = completedAt,
        Volume = 1000m,
        MaxEstimatedOneRepMax = record
    };
}
