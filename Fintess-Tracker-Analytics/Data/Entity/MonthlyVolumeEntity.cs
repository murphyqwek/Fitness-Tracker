namespace Fintess_Tracker_Analytics.Data.Entity
{
    public class MonthlyVolumeEntity
    {
        public Guid UserId { get; private set; }
        public int Year { get; private set; }
        public int Month { get; private set; }

        public decimal TotalVolume { get; private set; }
    }
}
