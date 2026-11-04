namespace Fitness_Tracker_Application.Service.Metrics;

public interface IFitnessMetrics
{
    void RecordWorkoutCreated();
    void RecordUserRegistered();
    void RecordLoginAttempt(bool success);
    void SetPendingOutboxMessages(long count);
    IDisposable MeasureExerciseRetrieval(ExerciseRetrievalOperation operation);
}

public enum ExerciseRetrievalOperation
{
    Search,
    ById
}
