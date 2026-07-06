namespace Fintess_Tracker_Domain.Entity
{
    public class ExerciseMuscle
    {
        public Muscle Muscle { get; private set; }
        public decimal PercentageOfUsage { get; private set; }

        public ExerciseMuscle(Muscle muscle, decimal percentageOfUsage)
        {
            Muscle = muscle;
            PercentageOfUsage = percentageOfUsage;
        }
    }
}
