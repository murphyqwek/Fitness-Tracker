using Fitness_Tracker_Application.Service.Metrics;
using Prometheus;

namespace Fitness_Tracker_Infrastructure.Metrics;

public sealed class PrometheusFitnessMetrics : IFitnessMetrics
{
    private readonly Counter _workoutsCreated;
    private readonly Counter _usersRegistered;
    private readonly Counter _loginAttempts;
    private readonly Gauge _pendingOutboxMessages;
    private readonly Histogram _exerciseRetrievalDuration;

    public PrometheusFitnessMetrics(CollectorRegistry? registry = null)
    {
        var factory = Prometheus.Metrics.WithCustomRegistry(registry ?? Prometheus.Metrics.DefaultRegistry);
        _workoutsCreated = factory.CreateCounter(
            "fitness_workouts_created_total", "Number of newly persisted workouts.");
        _usersRegistered = factory.CreateCounter(
            "fitness_users_registered_total", "Number of newly persisted users.");
        _loginAttempts = factory.CreateCounter(
            "fitness_login_attempts_total", "Number of completed credential checks, by result.",
            new CounterConfiguration { LabelNames = ["result"] });
        _loginAttempts.WithLabels("success");
        _loginAttempts.WithLabels("failure");
        _pendingOutboxMessages = factory.CreateGauge(
            "fitness_outbox_pending_messages", "Last observed number of all unprocessed outbox messages.",
            new GaugeConfiguration { SuppressInitialValue = true });
        _exerciseRetrievalDuration = factory.CreateHistogram(
            "fitness_exercise_retrieval_duration_seconds",
            "Duration of exercise retrieval including cache preparation, also on failure or cancellation.",
            new HistogramConfiguration
            {
                LabelNames = ["operation"],
                Buckets = [0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30]
            });
        _exerciseRetrievalDuration.WithLabels("search");
        _exerciseRetrievalDuration.WithLabels("by_id");
    }

    public void RecordWorkoutCreated() => _workoutsCreated.Inc();
    public void RecordUserRegistered() => _usersRegistered.Inc();
    public void RecordLoginAttempt(bool success) =>
        _loginAttempts.WithLabels(success ? "success" : "failure").Inc();
    public void SetPendingOutboxMessages(long count) => _pendingOutboxMessages.Set(count);

    public IDisposable MeasureExerciseRetrieval(ExerciseRetrievalOperation operation) =>
        _exerciseRetrievalDuration.WithLabels(operation switch
        {
            ExerciseRetrievalOperation.Search => "search",
            ExerciseRetrievalOperation.ById => "by_id",
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        }).NewTimer();
}
