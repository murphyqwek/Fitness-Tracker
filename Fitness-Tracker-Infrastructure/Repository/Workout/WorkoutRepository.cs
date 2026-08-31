using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Repository.Workout;
using Fitness_Tracker_Infrastructure.Data;
using Fitness_Tracker_Infrastructure.Model;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Fitness_Tracker_Infrastructure.Repository.Workout
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _cache;
        private readonly IMapper _mapper;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly TimeSpan _feedCacheTtl = TimeSpan.FromDays(7);
        private readonly TimeSpan _detailCacheTtl = TimeSpan.FromHours(24);

        public WorkoutRepository(ApplicationDbContext context, IConnectionMultiplexer connectionMultiplexer, IMapper mapper)
        {
            _context = context;
            _cache = connectionMultiplexer.GetDatabase();
            _mapper = mapper;
        }

        private static string GetWorkoutDetailKey(Guid userId, Guid workoutId) => $"workout:{userId}:{workoutId}";
        private static string GetWorkoutReducedKey(Guid workoutId) => $"workout:reduced:{workoutId}";
        private static string GetTimelineKey(Guid userId) => $"user:{userId}:timeline";

        public async Task<Result<Guid>> CreateOrUpdateWorkoutAsync(Guid userId, CreateWorkoutDTO createWorkoutDTO, CancellationToken cancellationToken)
        {
            var workoutEntity = _mapper.Map<WorkoutEntity>(createWorkoutDTO);
            workoutEntity.UserId = userId;

            if (workoutEntity.CreatedAt == default)
            {
                workoutEntity.CreatedAt = DateTimeOffset.UtcNow;
            }

            try
            {
                var exists = await _context.Workouts.AnyAsync(w => w.Id == workoutEntity.Id && w.UserId == userId, cancellationToken);

                if (exists)
                {
                    _context.Workouts.Update(workoutEntity);
                }
                else
                {
                    await _context.Workouts.AddAsync(workoutEntity, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);

                var batch = _cache.CreateBatch();

                string timelineKey = GetTimelineKey(userId);
                double score = workoutEntity.CreatedAt.ToUnixTimeMilliseconds();

                _ = batch.SortedSetAddAsync(timelineKey, workoutEntity.Id.ToString(), score);
                _ = batch.KeyExpireAsync(timelineKey, _feedCacheTtl);

                _ = batch.KeyDeleteAsync(GetWorkoutDetailKey(userId, workoutEntity.Id));
                _ = batch.KeyDeleteAsync(GetWorkoutReducedKey(workoutEntity.Id));

                batch.Execute();
            }
            catch (Exception)
            {
                return Result.Fail("Database error");
            }

            return Result.Ok(workoutEntity.Id);
        }

        public async Task<Result<ResponseWorkoutDTO>> GetWorkoutById(Guid userId, Guid workoutId, CancellationToken cancellationToken)
        {
            string cacheKey = GetWorkoutDetailKey(userId, workoutId);

            RedisValue cached = await _cache.StringGetAsync(cacheKey);

            if (cached.HasValue)
            {
                string value = cached.ToString();
                if (value == "NOT_FOUND")
                {
                    return Result.Fail($"No user's workout with {workoutId}");
                }

                return JsonSerializer.Deserialize<ResponseWorkoutDTO>(value, _jsonOptions)!;
            }

            var workout = await _context.Workouts
                .Where(w => w.Id == workoutId && w.UserId == userId)
                .ProjectTo<ResponseWorkoutDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (workout == null)
            {
                await _cache.StringSetAsync(cacheKey, "NOT_FOUND", TimeSpan.FromMinutes(2));
                return Result.Fail($"No user's workout with {workoutId}");
            }

            string json = JsonSerializer.Serialize(workout, _jsonOptions);
            await _cache.StringSetAsync(cacheKey, json, _detailCacheTtl);

            return workout;
        }

        public async Task<List<ResponseWorkoutReducedDTO>> GetWorkoutsTimelineAsync(Guid userId, DateTimeOffset? cursor, int limit)
        {
            string timelineKey = GetTimelineKey(userId);

            var cursorTime = cursor ?? DateTimeOffset.UtcNow;
            double cursorScore = cursorTime.ToUnixTimeMilliseconds();

            var cachedIds = await _cache.SortedSetRangeByScoreAsync(
                key: timelineKey,
                start: cursorScore - 0.001,
                stop: double.NegativeInfinity,
                exclude: Exclude.None,
                order: Order.Descending,
                take: limit);

            if (cachedIds.Length == limit)
            {
                var reducedKeys = cachedIds.Select(id => (RedisKey)GetWorkoutReducedKey(Guid.Parse(id.ToString()!))).ToArray();
                var cachedWorkoutsJson = await _cache.StringGetAsync(reducedKeys);

                if (cachedWorkoutsJson.All(v => v.HasValue))
                {
                    return cachedWorkoutsJson
                        .Select(v => JsonSerializer.Deserialize<ResponseWorkoutReducedDTO>(v.ToString(), _jsonOptions)!)
                        .ToList();
                }
            }

            var workouts = await _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == userId && w.CreatedAt < cursorTime)
                .OrderByDescending(w => w.CreatedAt)
                .Take(limit)
                .ProjectTo<ResponseWorkoutReducedDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (workouts.Count > 0)
            {
                _ = Task.Run(() => CacheTimelineFeedAsync(userId, workouts, timelineKey));
            }

            return workouts;
        }

        private async Task CacheTimelineFeedAsync(Guid userId, List<ResponseWorkoutReducedDTO> workouts, string timelineKey)
        {
            var batch = _cache.CreateBatch();
            var zsetEntries = new List<SortedSetEntry>();

            foreach (var workout in workouts)
            {
                string reducedKey = GetWorkoutReducedKey(workout.Id);
                string json = JsonSerializer.Serialize(workout, _jsonOptions);

                _ = batch.StringSetAsync(reducedKey, json, _feedCacheTtl);

                double score = new DateTimeOffset(workout.Date, TimeSpan.Zero).ToUnixTimeMilliseconds();
                zsetEntries.Add(new SortedSetEntry(workout.Id.ToString(), score));
            }

            _ = batch.SortedSetAddAsync(timelineKey, zsetEntries.ToArray());
            _ = batch.KeyExpireAsync(timelineKey, _feedCacheTtl);

            _ = batch.SortedSetRemoveRangeByRankAsync(timelineKey, 0, -101);

            batch.Execute();
        }
    }
}