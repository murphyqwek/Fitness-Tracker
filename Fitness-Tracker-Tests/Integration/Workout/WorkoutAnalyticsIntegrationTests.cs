using Fintess_Tracker_Analytics.Data.Entity;
using Fitness_Tracker.Tests.Integration;
using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Repository.Workout;
using Fitness_Tracker_Infrastructure.Model;
using Fitness_Tracker_Shared;
using Fitness_Tracker_Tests.Integration.Factory;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using AnalyticsDbContext = Fintess_Tracker_Analytics.Data.ApplicationDbContext;
using FitnessDbContext = Fitness_Tracker_Infrastructure.Data.ApplicationDbContext;

[Collection("Integration")]
public sealed class WorkoutAnalyticsIntegrationTests
{
    private readonly TestDatabaseFixture _fixture;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public WorkoutAnalyticsIntegrationTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CompletingWorkout_ShouldPublishEvent_AndCreateAnalytics()
    {
        await using var fitnessFactory =
            new FitnessTrackerFactory(_fixture);

        await using var analyticsFactory =
            new AnalyticsFactory(_fixture);

        _fixture.JwtPublicKeyPem.Should().NotBeNullOrWhiteSpace();

        using var fitnessClient = fitnessFactory.CreateClient();
        using var analyticsClient = analyticsFactory.CreateClient();

        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        await using (var scope = fitnessFactory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<FitnessDbContext>();

            var user = new UserEntity
            {
                Id = userId,
                Login = $"test_{Guid.NewGuid():N}"[..15],
                Password = "test_hash"
            };

            context.Users.Add(user);

            await context.SaveChangesAsync();
        }

        int benchPressId;
        int squatId;

        await using (var scope = fitnessFactory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<FitnessDbContext>();

            var benchPress = new ExerciseEntity
            {
                Id = 1,
                Name = "Bench Press",
                Description = "Test bench press",
                Muscles = new List<ExerciseMuscleEntity>()
            };

            var squat = new ExerciseEntity
            {
                Id = 2,
                Name = "Squat",
                Description = "Test squat",
                Muscles = new List<ExerciseMuscleEntity>()
            };

            context.Exercises.AddRange(
                benchPress,
                squat);

            await context.SaveChangesAsync();

            benchPressId = benchPress.Id;
            squatId = squat.Id;
        }

        var workout = new CreateWorkoutDTO(
            "Integration test workout",
            "Kafka integration test",
            DateTimeOffset.UtcNow,
            new List<CreateWorkoutSetDTO>
            {
                new(
                    ExerciseId: 1,
                    Repetitions: 10,
                    Weight: 100m,
                    Order: 1),

                new(
                    ExerciseId: 1,
                    Repetitions: 5,
                    Weight: 110m,
                    Order: 2),

                new(
                    ExerciseId: 2,
                    Repetitions: 5,
                    Weight: 150m,
                    Order: 3)
            }
        );

        await using (var scope =
                     fitnessFactory.Services.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkoutRepository>();

            var result = await repository.CreateOrUpdateWorkoutAsync(
                userId,
                workout,
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();

            workoutId = result.Value;
        }


        await using (var scope =
                     fitnessFactory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();

            var message = await context.OutboxMessages.SingleAsync(x => x.Type == nameof(WorkoutCompletedV1Event));

            message.Should().NotBeNull();

            var integrationEvent =
                JsonSerializer.Deserialize<WorkoutCompletedV1Event>(message.Payload, _jsonOptions);

            integrationEvent.Should().NotBeNull();

            integrationEvent!.WorkoutId
                .Should().Be(workoutId);

            integrationEvent.UserId
                .Should().Be(userId);

            integrationEvent.Exercises
                .Should().HaveCount(2);
        }


        var analytics = await WaitForAnalyticsAsync(
            analyticsFactory.Services,
            workoutId,
            TimeSpan.FromSeconds(15));

        analytics.Should().NotBeNull();

        analytics!.WorkoutId.Should().Be(workoutId);
        analytics.UserId.Should().Be(userId);

        analytics.Volume.Should().Be(2300m);

        analytics.MaxEstimatedOneRepMax.Should().Be(175m);
    }

    private static async Task<WorkoutAnalyticsEntity> WaitForAnalyticsAsync(IServiceProvider services, Guid workoutId, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            await using var scope = services.CreateAsyncScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AnalyticsDbContext>();

            var analytics = await context.Workouts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.WorkoutId == workoutId);

            if (analytics is not null)
                return analytics;

            await Task.Delay(200);
        }

        throw new TimeoutException($"Analytics for workout {workoutId} was not created within {timeout}.");
    }
}