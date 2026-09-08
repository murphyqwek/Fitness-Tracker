using Fintess_Tracker_Analytics.Data;
using Fintess_Tracker_Analytics.Data.Entity;
using Fintess_Tracker_Analytics.DTO;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Fintess_Tracker_Analytics.Repository
{
    public class AnalyticRepository : IAnalyticRepository
    {
        private readonly ApplicationDbContext _context;
        public AnalyticRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<Result<MonthlyVolumeResponse>> GetMonthlyVolumeAsync(Guid userId, int year, int month, CancellationToken cancellationToken)
        {
            var periodStart = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);

            var periodEnd = periodStart.AddMonths(1);

            var totalVolume = await _context.Workouts
                .AsNoTracking()
                .Where(cw => cw.UserId == userId && cw.CompletedAt >= periodStart && cw.CompletedAt < periodEnd)
                .SumAsync(cw => cw.Volume);

            return Result.Ok(new MonthlyVolumeResponse(totalVolume));
        }

        public async Task<Result<WeeklyRecordResponse>> GetWeeklyRecordAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken)
        {
            var periodStart = new DateTimeOffset(
            weekStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

            var periodEnd = periodStart.AddDays(7);

            var estimatedOneRepMax = await _context.Workouts
                .AsNoTracking()
                .Where(cw => cw.UserId == userId && cw.CompletedAt >= periodStart && cw.CompletedAt < periodEnd)
                .MaxAsync(cw => cw.MaxEstimatedOneRepMax);
            return Result.Ok(new WeeklyRecordResponse(estimatedOneRepMax));
        }

        public async Task<Result> SaveNewWorkoutAsync(SaveCompleteWorkoutDTO completedWorkout, CancellationToken cancellationToken)
        {
            var alreadyExists = await _context.Workouts
                .AsNoTracking()
                .AnyAsync(workout => workout.WorkoutId == completedWorkout.WorkoutId, cancellationToken);

            if (alreadyExists)
            {
                return Result.Ok();
            }

            var workoutAnalytics = new WorkoutAnalyticsEntity()
            {
                WorkoutId = completedWorkout.WorkoutId,
                UserId = completedWorkout.UserId,
                CompletedAt = completedWorkout.CompletedAt.ToUniversalTime(),
                Volume = completedWorkout.volume,
                MaxEstimatedOneRepMax = completedWorkout.e1RP
            };

            await _context.Workouts.AddAsync(
                workoutAnalytics,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
