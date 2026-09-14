using System.Text.Json;
using Fintess_Tracker_Analytics.DTO;
using Fintess_Tracker_Analytics.Repository;
using FluentResults;
using StackExchange.Redis;

namespace Fintess_Tracker_Analytics.Service;

public sealed class AnalyticService
{
    private static readonly TimeSpan CacheExpiration =
        TimeSpan.FromMinutes(10);

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IAnalyticRepository _repository;
    private readonly IDatabase _cache;

    public AnalyticService(
        IAnalyticRepository repository,
        IConnectionMultiplexer multiplexer)
    {
        _repository = repository;
        _cache = multiplexer.GetDatabase();
    }

    public async Task<Result> SaveWorkoutAsync(
        CompletedWorkoutDTO workout,
        CancellationToken cancellationToken)
    {
        var maxEstimatedOneRepMax = workout.Sets
            .Select(WorkoutAnalyticsCalculator.CalculateE1Rm)
            .DefaultIfEmpty(0)
            .Max();

        var workoutAnalytics = new SaveCompleteWorkoutDTO(
            workout.WorkoutId,
            workout.UserId,
            workout.CompletedAt.ToUniversalTime(),
            WorkoutAnalyticsCalculator.CalculateVolume(workout),
            maxEstimatedOneRepMax);

        var result = await _repository.SaveNewWorkoutAsync(
            workoutAnalytics,
            cancellationToken);

        if (result.IsFailed)
            return result;

        await InvalidateAffectedCacheAsync(
            workout,
            cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<MonthlyVolumeResponse>> GetMonthlyVolumeAsync(Guid userId, int year, int month, CancellationToken cancellationToken)
    {
        var cacheKey = GetMonthlyVolumeCacheKey(
            userId,
            year,
            month);

        var cachedValue =
            await TryGetFromCacheAsync<MonthlyVolumeResponse>(
                cacheKey,
                cancellationToken);

        if (cachedValue is not null)
            return Result.Ok(cachedValue);

        var result = await _repository.GetMonthlyVolumeAsync(
            userId,
            year,
            month,
            cancellationToken);

        if (result.IsSuccess)
        {
            await TrySetCacheAsync(
                cacheKey,
                result.Value,
                cancellationToken);
        }

        return result;
    }

    public async Task<Result<WeeklyRecordResponse>> GetWeeklyRecordAsync(Guid userId, DateOnly weekStart, CancellationToken cancellationToken)
    {
        var normalizedWeekStart = GetWeekStart(weekStart);

        var cacheKey = GetWeeklyRecordCacheKey(
            userId,
            normalizedWeekStart);

        var cachedValue =
            await TryGetFromCacheAsync<WeeklyRecordResponse>(
                cacheKey,
                cancellationToken);

        if (cachedValue is not null)
            return Result.Ok(cachedValue);

        var result = await _repository.GetWeeklyRecordAsync(
            userId,
            normalizedWeekStart,
            cancellationToken);

        if (result.IsSuccess)
        {
            await TrySetCacheAsync(
                cacheKey,
                result.Value,
                cancellationToken);
        }

        return result;
    }

    private async Task InvalidateAffectedCacheAsync(CompletedWorkoutDTO workout, CancellationToken cancellationToken)
    {
        var completedDate = DateOnly.FromDateTime(
            workout.CompletedAt.UtcDateTime);

        var weekStart = GetWeekStart(completedDate);

        var monthlyKey = GetMonthlyVolumeCacheKey(
            workout.UserId,
            completedDate.Year,
            completedDate.Month);

        var weeklyKey = GetWeeklyRecordCacheKey(
            workout.UserId,
            weekStart);

        await TryRemoveCacheAsync(
            monthlyKey,
            cancellationToken);

        await TryRemoveCacheAsync(
            weeklyKey,
            cancellationToken);
    }

    private static string GetMonthlyVolumeCacheKey(Guid userId, int year, int month)
    {
        return $"monthly_volume:{userId}:{year}:{month:D2}";
    }

    private static string GetWeeklyRecordCacheKey(Guid userId, DateOnly weekStart)
    {
        return $"weekly_record:{userId}:{weekStart:yyyy-MM-dd}";
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var daysFromMonday =
            ((int)date.DayOfWeek + 6) % 7;

        return date.AddDays(-daysFromMonday);
    }

    private async Task<T?> TryGetFromCacheAsync<T>(
        string key,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            var cachedValue = await _cache
                .StringGetAsync(key)
                .WaitAsync(cancellationToken);

            if (!cachedValue.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(
                cachedValue.ToString(),
                JsonOptions);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is RedisException or JsonException)
        {
            return null;
        }
    }

    private async Task TrySetCacheAsync<T>(string key, T value, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(
                value,
                JsonOptions);

            await _cache
                .StringSetAsync(
                    key,
                    json,
                    CacheExpiration)
                .WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (RedisException)
        {
            
        }
    }

    private async Task TryRemoveCacheAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _cache
                .KeyDeleteAsync(key)
                .WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (RedisException)
        {
            
        }
    }
}