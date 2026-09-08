namespace Fintess_Tracker_Analytics.Data.Model
{
    public class MonthlyVolume
    {
        public Guid UserId { get; private set; }
        public int Year { get; private set; }
        public int Month { get; private set; }
        public decimal TotalVolume { get; private set; }

        private MonthlyVolume() { }

        public MonthlyVolume(Guid userId, int year, int month)
        {
            UserId = userId;
            Year = year;
            Month = month;
        }

        public void Add(decimal volume)
        {
            if (volume < 0)
                throw new ArgumentOutOfRangeException(nameof(volume));

            TotalVolume += volume;
        }
    }
}
