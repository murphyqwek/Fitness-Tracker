using Fintess_Tracker_Analytics.Data.Model;
using Fintess_Tracker_Analytics.DTO;
using FluentResults;

namespace Fintess_Tracker_Analytics.Repository
{
    public interface IAnalyticRepository
    {
        Task<Result<MonthlyVolumeResponse>> GetMonthlyVolumeAsync(Guid userId, int year, int month, CancellationToken cancellationToken);

        Task<Result<WeeklyRecordResponse>> GetWeeklyRecordAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken);

        Task<Result> SaveNewWorkoutAsync(SaveCompleteWorkoutDTO completedWorkout, CancellationToken cancellationToken);
    }
}
