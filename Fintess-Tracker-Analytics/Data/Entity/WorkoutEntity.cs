namespace Fintess_Tracker_Analytics.Data.Entity
{
    public class WorkoutAnalyticsEntity
    {
        public Guid WorkoutId { get; private set; }
        public Guid UserId { get; private set; }

        public DateTimeOffset CompletedAt { get; private set; }

        public decimal Volume { get; private set; }
        public decimal MaxEstimatedOneRepMax { get; private set; }
    }
}
