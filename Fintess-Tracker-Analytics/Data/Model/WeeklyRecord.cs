namespace Fintess_Tracker_Analytics.Data.Model
{
    public class WeeklyRecord
    {
        public Guid UserId { get; private set; }
        public DateOnly WeekStart { get; private set; }
        public decimal EstimatedOneRepMax { get; private set; }

        private WeeklyRecord() { }

        public WeeklyRecord(Guid userId, DateOnly weekStart)
        {
            UserId = userId;
            WeekStart = weekStart;
        }

        public void UpdateIfGreater(decimal estimatedOneRepMax)
        {
            if (estimatedOneRepMax < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(estimatedOneRepMax));

            if (estimatedOneRepMax > EstimatedOneRepMax)
                EstimatedOneRepMax = estimatedOneRepMax;
        }
    }
}
