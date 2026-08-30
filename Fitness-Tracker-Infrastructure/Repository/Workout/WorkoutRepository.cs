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

        public WorkoutRepository(ApplicationDbContext context, IConnectionMultiplexer connectionMultiplexer, IMapper mapper)
        {
            _context = context;
            _cache = connectionMultiplexer.GetDatabase();
            _mapper = mapper;
        }

        private string GetWorkoutKey(Guid userId, Guid workoutId) 
        {
            return $"workout:{userId}:{workoutId}";
        }

        public async Task<Result<Guid>> CreateNewWorkout(Guid userId, CreateWorkoutDTO createWorkoutDTO, CancellationToken cancellationToken)
        {
            var workoutEntity = _mapper.Map<WorkoutEntity>(createWorkoutDTO);

            workoutEntity.UserId = userId;

            try
            {
                await _context.Workouts.AddAsync(workoutEntity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                return Result.Fail("Database error");
            }

            return Result.Ok(workoutEntity.Id);
        }

        public async Task<Result<ResponseWorkoutDTO>> GetWorkoutById(Guid userId, Guid workoutId, CancellationToken cancellationToken)
        {
            string cacheKey = GetWorkoutKey(userId, workoutId);

            RedisValue cached = await _cache.StringGetAsync(cacheKey);

            if(cached.HasValue) 
            {
                string value = cached.ToString();

                if(value == "NOT_FOUND")
                {
                    return Result.Fail($"No user's workout with {workoutId}");
                }

                return JsonSerializer.Deserialize<ResponseWorkoutDTO>(value, _jsonOptions)!;
            }

            var workout = await _context.Workouts
                                        .Where(workout => workout.Id == workoutId && workout.UserId == userId)
                                        .ProjectTo<ResponseWorkoutDTO>(_mapper.ConfigurationProvider)
                                        .FirstOrDefaultAsync(cancellationToken);
            
            if(workout == null) 
            {
                await _cache.StringSetAsync(cacheKey, "NOT_FOUND", TimeSpan.FromMinutes(2));

                return Result.Fail($"No user's workout with {workoutId}");
            }

            string json = JsonSerializer.Serialize(workout, _jsonOptions);
            await _cache.StringSetAsync(cacheKey, json, TimeSpan.FromHours(24));

            return workout;
        }
    }
}
