namespace Fintess_Tracker_Analytics.Data.Entity
{
    public class WeeklyRecordEntity
    {
        public Guid UserId { get; private set; }
        public DateOnly WeekStart { get; private set; }
        public decimal EstimatedOneRepMax { get; private set; }
    }
}