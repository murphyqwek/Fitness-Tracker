using Fitness_Tracker_Application.Features.Exercise;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Service.Pagination;
using FluentAssertions;
using FluentResults;
using Moq;
using Xunit;

namespace Fitness_Tracker_Tests.Application.Features.Exercise
{
    public class ExerciseHandlersTests
    {
        private readonly Mock<IExerciseRepository> _exerciseRepoMock;
        private readonly ExerciseFuzzySearch _fuzzySearch;

        public ExerciseHandlersTests()
        {
            _exerciseRepoMock = new Mock<IExerciseRepository>();
            _fuzzySearch = new ExerciseFuzzySearch();
        }

        [Fact]
        public async Task FillCacheExercise_WhenRepositorySucceeds_ShouldReturnOk()
        {
            _exerciseRepoMock
                .Setup(r => r.FillCacheFromDb(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new FillCacheExercise(_exerciseRepoMock.Object);

            var result = await handler.Handle(new FillCacheExerciseCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _exerciseRepoMock.Verify(r => r.FillCacheFromDb(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FillCacheExercise_WhenRepositoryThrows_ShouldReturnFailWithError()
        {
            _exerciseRepoMock
                .Setup(r => r.FillCacheFromDb(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB connection error"));

            var handler = new FillCacheExercise(_exerciseRepoMock.Object);

            var result = await handler.Handle(new FillCacheExerciseCommand(), CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be("DB connection error");
        }

        [Fact]
        public async Task GetExerciseByIdQueary_ShouldReturnRepositoryResult()
        {
            var exerciseDto = new ExerciseSearchDTO(10, "Тяга верхнего блока", "Спина", new List<ExerciseMuscleDTO>());
            _exerciseRepoMock
                .Setup(r => r.GetExerciseByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(exerciseDto));

            var handler = new GetExerciseByIdQueary(_fuzzySearch, _exerciseRepoMock.Object);

            var result = await handler.Handle(new ExerciseSearchByIdCommand(10), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(exerciseDto);
        }

        [Fact]
        public async Task ExerciseSearchQueary_ShouldCallRepositoryWithCorrectParameters()
        {
            var expectedResponse = new PaginationResponse<ExerciseSearchReducedDTO>(
                page: 1,
                size: 10,
                total: 1,
                data: new List<ExerciseSearchReducedDTO>
                {
                    new(1, "Отжимания", new List<ExerciseMuscleDTO>())
                }
            );

            _exerciseRepoMock
                .Setup(r => r.GetExerciseAsync("отжим", It.IsAny<IList<int>>(), 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var handler = new ExerciseSearchQueary(_fuzzySearch, _exerciseRepoMock.Object);
            var command = new ExerciseSearchCommand("отжим", new List<int> { 1 })
            {
                Page = 1,
                Size = 10
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeEquivalentTo(expectedResponse);
            _exerciseRepoMock.Verify(r => r.GetExerciseAsync("отжим", command.MusclesId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}