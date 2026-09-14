using Fintess_Tracker_Analytics.DTO;

namespace Fintess_Tracker_Analytics.Service
{
    public static class WorkoutAnalyticsCalculator
    {
        public static decimal CalculateVolume(CompletedWorkoutDTO workout)
        {
            return workout.Sets.Sum(set =>
                set.Weight * set.Repetitions);
        }

        public static decimal CalculateMaxE1Rm(CompletedWorkoutDTO workout)
        {
            return workout.Sets
                .Select(CalculateE1Rm)
                .DefaultIfEmpty(0)
                .Max();
        }

        public static decimal CalculateE1Rm(CompletedSetDTO set)
        {
            if (set.Weight <= 0 || set.Repetitions <= 0)
                return 0;

            if (set.Repetitions == 1)
                return set.Weight;

            return set.Weight * (1 + set.Repetitions / 30m);
        }
    }
}
