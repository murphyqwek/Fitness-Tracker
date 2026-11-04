using System.Text;
using Fitness_Tracker_Application.DTO.Exercise;
using Fitness_Tracker_Application.Features.Exercise;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Service.Pagination;
using Fitness_Tracker_Infrastructure.Metrics;
using Moq;
using Prometheus;

namespace Fitness_Tracker_Tests.Application.Exercise;

public class ExerciseMetricsExportTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Search_ShouldExportDurationOnlyAfterRetrievalFinishes(bool failDuringPreparation)
    {
        var registry = Metrics.NewCustomRegistry();
        var metrics = new PrometheusFitnessMetrics(registry);
        var repository = new Mock<IExerciseRepository>();
        var preparation = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        repository.Setup(r => r.FillCacheIfEmpty(It.IsAny<CancellationToken>()))
            .Returns(preparation.Task);
        repository.Setup(r => r.GetExerciseAsync(null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationResponse<ExerciseSearchReducedDTO>(1, 10, 0, []));
        var handler = new ExerciseSearchQueary(repository.Object, metrics);

        var retrieval = handler.Handle(
            new ExerciseSearchCommand(null, null) { Page = 1, Size = 10 }, CancellationToken.None);

        Assert.False(retrieval.IsCompleted);
        Assert.Contains("fitness_exercise_retrieval_duration_seconds_count{operation=\"search\"} 0", await Export(registry));

        if (failDuringPreparation)
        {
            preparation.SetException(new InvalidOperationException("Redis is unavailable"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => retrieval);
        }
        else
        {
            preparation.SetResult();
            await retrieval;
        }

        var exported = await Export(registry);
        Assert.Contains("fitness_exercise_retrieval_duration_seconds_count{operation=\"search\"} 1", exported);
        Assert.Contains("fitness_exercise_retrieval_duration_seconds_count{operation=\"by_id\"} 0", exported);
        Assert.DoesNotContain("Redis is unavailable", exported);
    }

    private static async Task<string> Export(CollectorRegistry registry)
    {
        using var stream = new MemoryStream();
        await registry.CollectAndExportAsTextAsync(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
