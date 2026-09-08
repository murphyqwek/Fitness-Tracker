namespace Fintess_Tracker_Analytics.Data.Entity
{
    public class WorkoutEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public DateTimeOffset CompletedAt { get; private set; }
    }
}
