using AutoMapper;
using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Infrastructure.Model;
using Fitness_Tracker_Infrastructure.Repository.Workout;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Fitness_Tracker.Tests.Integration
{
    public class WorkoutRepositoryTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly WorkoutRepository _repository;
        private readonly IMapper _mapper;

        public WorkoutRepositoryTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateWorkoutDTO, WorkoutEntity>();
                cfg.CreateMap<CreateWorkoutSetDTO, WorkoutSetEntity>();

                cfg.CreateMap<WorkoutEntity, ResponseWorkoutDTO>();
                cfg.CreateMap<WorkoutSetEntity, ReponseWorkoutSetDTO>();

                cfg.CreateMap<WorkoutEntity, ResponseWorkoutReducedDTO>();
                cfg.CreateMap<WorkoutSetEntity, ResponseWorkoutSetReducedDTO>();
            }, NullLoggerFactory.Instance);

            _mapper = config.CreateMapper();

            var exerciseRepoMock = new Mock<IExerciseRepository>();

            _repository = new WorkoutRepository(
                _fixture.DbContext,
                _fixture.RedisMultiplexer,
                _mapper,
                exerciseRepoMock.Object
            );
        }

        [Fact]
        public async Task CreateOrUpdateWorkoutAsync_ShouldPersistInPostgres_And_SetTimelineInRedis()
        {
            var testUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Login = $"u_{Guid.NewGuid():N}"[..15],
                Password = "hash123"
            };
            await _fixture.DbContext.Users.AddAsync(testUser);
            await _fixture.DbContext.SaveChangesAsync();

            var userId = testUser.Id;

            var workoutDto = new CreateWorkoutDTO(
                Name: "Upper Body",
                Description: "Chest and back",
                CreateAt: DateTimeOffset.UtcNow,
                workoutSets: new List<CreateWorkoutSetDTO>()
            );

            var result = await _repository.CreateOrUpdateWorkoutAsync(userId, workoutDto, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            var workoutId = result.Value;

            var dbWorkout = await _fixture.DbContext.Workouts
                .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);
            dbWorkout.Should().NotBeNull();
            dbWorkout!.Name.Should().Be("Upper Body");

            var db = _fixture.RedisMultiplexer.GetDatabase();
            string timelineKey = $"user:{userId}:timeline";
            var timelineEntries = await db.SortedSetRangeByRankAsync(timelineKey);

            timelineEntries.Should().NotBeEmpty();
            timelineEntries.Select(x => x.ToString()).Should().Contain(workoutId.ToString());
        }

        [Fact]
        public async Task GetWorkoutById_WhenNotFoundInDb_ShouldCacheNegativeResult()
        {
            var userId = Guid.NewGuid();
            var nonExistentWorkoutId = Guid.NewGuid();
            var db = _fixture.RedisMultiplexer.GetDatabase();

            var result = await _repository.GetWorkoutById(userId, nonExistentWorkoutId, CancellationToken.None);
            result.IsFailed.Should().BeTrue();

            string cacheKey = $"workout:{userId}:{nonExistentWorkoutId}";
            var cachedValue = await db.StringGetAsync(cacheKey);

            cachedValue.ToString().Should().Be("NOT_FOUND");
        }
    }
}