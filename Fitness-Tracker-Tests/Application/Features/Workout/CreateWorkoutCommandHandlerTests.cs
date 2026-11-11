using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Features.Workout;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Repository.Workout;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace Fitness_Tracker_Tests.Application.Features.Workout
{
    public class CreateWorkoutCommandHandlerTests
    {
        private readonly Mock<IWorkoutIdempotencyKeyRepository> _idempotencyRepoMock;
        private readonly Mock<IExerciseRepository> _exerciseRepoMock;
        private readonly Mock<IWorkoutRepository> _workoutRepoMock;
        private readonly CreateWorkoutCommandHandler _handler;

        public CreateWorkoutCommandHandlerTests()
        {
            _idempotencyRepoMock = new Mock<IWorkoutIdempotencyKeyRepository>();
            _exerciseRepoMock = new Mock<IExerciseRepository>();
            _workoutRepoMock = new Mock<IWorkoutRepository>();

            _handler = new CreateWorkoutCommandHandler(
                _idempotencyRepoMock.Object,
                _workoutRepoMock.Object,
                _exerciseRepoMock.Object);
        }

        private static CreateWorkoutDTO CreateTestWorkoutDTO(List<CreateWorkoutSetDTO>? sets = null)
        {
            return new CreateWorkoutDTO(
                Name: "Morning Workout",
                Description: "Push day",
                CreateAt: DateTimeOffset.UtcNow,
                workoutSets: sets ?? new List<CreateWorkoutSetDTO>()
            );
        }

        [Fact]
        public async Task Handle_WhenIdempotencyStatusIsFinished_ShouldReturnExistingWorkoutIdImmediately()
        {
            var userId = Guid.NewGuid();
            var idempotencyKey = Guid.NewGuid();
            var existingWorkoutId = Guid.NewGuid();
            var workoutDto = CreateTestWorkoutDTO();

            _idempotencyRepoMock
                .Setup(r => r.LockWorkout(userId, idempotencyKey))
                .ReturnsAsync((IdempotencyStatus.FINISHED, existingWorkoutId));

            var command = new CreateWorkoutCommand(userId, idempotencyKey, workoutDto);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(existingWorkoutId);

            _workoutRepoMock.Verify(r => r.CreateOrUpdateWorkoutAsync(
                It.IsAny<Guid>(), It.IsAny<CreateWorkoutDTO>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenExerciseDoesNotExist_ShouldReturnFailure()
        {
            var userId = Guid.NewGuid();
            var idempotencyKey = Guid.NewGuid();

            var sets = new List<CreateWorkoutSetDTO>
            {
                new CreateWorkoutSetDTO(ExerciseId: 999, Repetitions: 10, Weight: 50.0m, Order: 1)
            };
            var workoutDto = CreateTestWorkoutDTO(sets);

            var command = new CreateWorkoutCommand(userId, idempotencyKey, workoutDto);

            _idempotencyRepoMock
                .Setup(r => r.LockWorkout(userId, idempotencyKey))
                .ReturnsAsync((IdempotencyStatus.TAKEN, null));

            _exerciseRepoMock
                .Setup(r => r.IsAllExercisesExist(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Contain("does not exist in database");

            _workoutRepoMock.Verify(r => r.CreateOrUpdateWorkoutAsync(
                It.IsAny<Guid>(), It.IsAny<CreateWorkoutDTO>(), It.IsAny<CancellationToken>()), Times.Never);
            _idempotencyRepoMock.Verify(r => r.DeleteWorkoutIdempotencyStatus(userId, idempotencyKey), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenValidRequest_ShouldCreateWorkoutAndRetainResultForRetry()
        {
            var userId = Guid.NewGuid();
            var idempotencyKey = Guid.NewGuid();
            var createdWorkoutId = Guid.NewGuid();

            var sets = new List<CreateWorkoutSetDTO>
            {
                new CreateWorkoutSetDTO(ExerciseId: 1, Repetitions: 12, Weight: 80.0m, Order: 1)
            };
            var workoutDto = CreateTestWorkoutDTO(sets);
            var command = new CreateWorkoutCommand(userId, idempotencyKey, workoutDto);

            _idempotencyRepoMock
                .Setup(r => r.LockWorkout(userId, idempotencyKey))
                .ReturnsAsync((IdempotencyStatus.TAKEN, null));

            _exerciseRepoMock
                .Setup(r => r.IsAllExercisesExist(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _workoutRepoMock
                .Setup(r => r.CreateOrUpdateWorkoutAsync(userId, workoutDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(createdWorkoutId));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(createdWorkoutId);

            _idempotencyRepoMock.Verify(r => r.UpdateWorkoutIdempotencyStatus(userId, idempotencyKey, createdWorkoutId), Times.Once);
            _idempotencyRepoMock.Verify(r => r.DeleteWorkoutIdempotencyStatus(userId, idempotencyKey), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenSuccessfulRequestIsRepeated_ShouldReturnSavedResultWithoutCreatingAgain()
        {
            var userId = Guid.NewGuid();
            var idempotencyKey = Guid.NewGuid();
            var workoutId = Guid.NewGuid();
            var workoutDto = CreateTestWorkoutDTO();
            var command = new CreateWorkoutCommand(userId, idempotencyKey, workoutDto);
            Guid? savedWorkoutId = null;

            _idempotencyRepoMock
                .Setup(r => r.LockWorkout(userId, idempotencyKey))
                .ReturnsAsync(() => savedWorkoutId.HasValue
                    ? (IdempotencyStatus.FINISHED, savedWorkoutId)
                    : (IdempotencyStatus.TAKEN, (Guid?)null));
            _idempotencyRepoMock
                .Setup(r => r.UpdateWorkoutIdempotencyStatus(userId, idempotencyKey, workoutId))
                .Callback(() => savedWorkoutId = workoutId)
                .Returns(Task.CompletedTask);
            _exerciseRepoMock
                .Setup(r => r.IsAllExercisesExist(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _workoutRepoMock
                .Setup(r => r.CreateOrUpdateWorkoutAsync(userId, workoutDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(workoutId));

            var firstResult = await _handler.Handle(command, CancellationToken.None);
            var repeatedResult = await _handler.Handle(command, CancellationToken.None);

            firstResult.Value.Should().Be(workoutId);
            repeatedResult.Value.Should().Be(workoutId);
            _workoutRepoMock.Verify(r => r.CreateOrUpdateWorkoutAsync(
                userId, workoutDto, It.IsAny<CancellationToken>()), Times.Once);
            _idempotencyRepoMock.Verify(r => r.DeleteWorkoutIdempotencyStatus(userId, idempotencyKey), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRequestIsRunning_ShouldRejectWithoutSavingOrChangingStatus()
        {
            var userId = Guid.NewGuid();
            var idempotencyKey = Guid.NewGuid();
            var command = new CreateWorkoutCommand(userId, idempotencyKey, CreateTestWorkoutDTO());
            _idempotencyRepoMock
                .Setup(r => r.LockWorkout(userId, idempotencyKey))
                .ReturnsAsync((IdempotencyStatus.RUNNING, null));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.Single().Message.Should().Be(IdempotencyStatus.RUNNING.ToString());
            _workoutRepoMock.Verify(r => r.CreateOrUpdateWorkoutAsync(
                It.IsAny<Guid>(), It.IsAny<CreateWorkoutDTO>(), It.IsAny<CancellationToken>()), Times.Never);
            _idempotencyRepoMock.Verify(r => r.DeleteWorkoutIdempotencyStatus(userId, idempotencyKey), Times.Never);
            _idempotencyRepoMock.Verify(r => r.UpdateWorkoutIdempotencyStatus(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenSavingFails_ShouldReleaseKeyWithoutStoringSuccessfulResult()
        {
            var userId = Guid.NewGuid();
            var idempotencyKey = Guid.NewGuid();
            var workoutDto = CreateTestWorkoutDTO();
            var command = new CreateWorkoutCommand(userId, idempotencyKey, workoutDto);
            _idempotencyRepoMock
                .Setup(r => r.LockWorkout(userId, idempotencyKey))
                .ReturnsAsync((IdempotencyStatus.TAKEN, null));
            _exerciseRepoMock
                .Setup(r => r.IsAllExercisesExist(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _workoutRepoMock
                .Setup(r => r.CreateOrUpdateWorkoutAsync(userId, workoutDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<Guid>("Database error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.Single().Message.Should().Be("Database error");
            _idempotencyRepoMock.Verify(r => r.DeleteWorkoutIdempotencyStatus(userId, idempotencyKey), Times.Once);
            _idempotencyRepoMock.Verify(r => r.UpdateWorkoutIdempotencyStatus(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }
    }
}
