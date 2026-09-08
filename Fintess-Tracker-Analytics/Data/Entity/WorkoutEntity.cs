namespace Fintess_Tracker_Analytics.Data.Entity
{
    public class WorkoutAnalyticsEntity
    {
        public Guid WorkoutId { get; set; }
        public Guid UserId { get; set; }

        public DateTimeOffset CompletedAt { get; set; }

        public decimal Volume { get; set; }
        public decimal MaxEstimatedOneRepMax { get; set; }
    }
}
