using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fitness_Tracker_Application.Features.Exercise;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Service.Pagination;
using Fitness_Tracker_Infrastructure.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Fitness_Tracker_Infrastructure.Repository.Exercises
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDatabase _cache;
        private readonly ISearchCommandsAsync _searchCommands;
        private readonly IMemoryCache _memoryCache;
        private static readonly JsonSerializerOptions _jsonSerializationOptions = new JsonSerializerOptions()
        { 
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString
        };

        private static readonly Regex _sanitizeRegex = new(@"[^\p{L}\p{N}\s]", RegexOptions.Compiled);
        private const string INDEX_NAME = "idx:exercise";

        private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
            .SetSize(1);

        private static bool _isIndexInitialized = false;
        private static readonly object _lock = new();

        public ExerciseRepository(ApplicationDbContext context, IConnectionMultiplexer connection, IMapper mapper, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = connection.GetDatabase();
            _searchCommands = _cache.FT();
            _mapper = mapper;
            _memoryCache = memoryCache;

            if (!_isIndexInitialized)
            {
                lock (_lock)
                {
                    if (!_isIndexInitialized)
                    {
                        CreateIndexIfNotExists();
                        _isIndexInitialized = true;
                    }
                }
            }
        }

        private void CreateIndexIfNotExists()
        {
            try
            {
                _cache.FT().Info(INDEX_NAME);
            }
            catch
            {
                var schema = new Schema()
                    .AddTextField(new FieldName("$.Name", "name"), 5.0)
                    .AddTagField(new FieldName("$.Muscles[*].Id", "muscle_ids"));

                _cache.FT().Create(
                    INDEX_NAME,
                    new FTCreateParams()
                        .On(IndexDataType.JSON)
                        .Prefix("exercise:")
                        .Language("russian"),
                    schema
                );

            }
        }

        public async Task FillCacheFromDb(CancellationToken cancellationToken) 
        {
            var exercises = await _context.Exercises.ProjectTo<ExerciseSearchDTO>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

            foreach (var chunk in exercises.Chunk(50))
            {
                var tasks = chunk.Select(ex =>
                {
                    string json = JsonSerializer.Serialize(ex, _jsonSerializationOptions);
                    return _cache.JSON().SetAsync($"exercise:{ex.Id}", "$", json);
                });

                await Task.WhenAll(tasks);
            }
        }

        private static string GenerateCacheKey(string? name, IList<int>? musclesId, int page, int size)
        {
            var cleanName = name?.Trim().ToLowerInvariant() ?? "empty";
            var musclesPart = (musclesId != null && musclesId.Count > 0)
                ? string.Join(",", musclesId.OrderBy(id => id))
                : "all";

            return $"search:ex:name={cleanName}:muscles={musclesPart}:p={page}:s={size}";
        }

        public async Task<PaginationResponse<ExerciseSearchDTO>> GetExerciseAsync(string? name, IList<int>? musclesId, int page, int size, CancellationToken cancellationToken)
        {
            string cacheKey = GenerateCacheKey(name, musclesId, page, size);

            if (_memoryCache.TryGetValue(cacheKey, out PaginationResponse<ExerciseSearchDTO>? cachedResponse)) 
            {
                return cachedResponse!;
            }

            var result = await SearchInRedisAsync(name, musclesId, page, size, cancellationToken);

            _memoryCache.Set(cacheKey, result, _cacheOptions);

            return result;
        }

        public async Task<PaginationResponse<ExerciseSearchDTO>> SearchInRedisAsync(string? name, IList<int>? musclesId, int page, int size, CancellationToken cancellationToken) 
        {
            var queryFuzzy = new List<string>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                string sanitizedText = _sanitizeRegex.Replace(name, "").Trim();

                if (!string.IsNullOrEmpty(sanitizedText))
                {
                    var words = sanitizedText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    var safeWords = words.Select(word =>
                    {
                        if (word.Length <= 3) return $"{word}*";
                        return $"({word}*|%{word}%)";
                    });


                    queryFuzzy.Add($"@name:({string.Join(" ", safeWords)})");
                }
            }

            if (musclesId != null && musclesId.Count > 0)
            {
                var distincIds = musclesId.Distinct().ToList();
                var tags = string.Join("|", distincIds);

                queryFuzzy.Add($"@muscle_ids:{{{tags}}}");
            }

            string queryString = queryFuzzy.Count > 0 ? string.Join(" ", queryFuzzy) : "*";

            bool searchAll = queryString == "*";

            int offset = (page - 1) * size;

            Query query = new Query(queryString).SetLanguage("russian").Limit(offset, size);

            if (!searchAll)
            {
                query = query.SetWithScores();
            }

            var findedExercises = new List<ExerciseSearchDTO>(size);
            int total = 0;

            try
            {
                var searchResult = await _searchCommands.SearchAsync(INDEX_NAME, query);
                FillFindedExercises(searchResult, findedExercises);
                total = (int)searchResult.TotalResults;
            }
            catch (Exception)
            {
                findedExercises.Clear();
            }

            return new PaginationResponse<ExerciseSearchDTO>(page, size, total, findedExercises);
        }

        private void FillFindedExercises(SearchResult searchResult, List<ExerciseSearchDTO> findedExercises) 
        {
            foreach (var document in searchResult.Documents)
            {
                var prop = document.GetProperties().FirstOrDefault();

                if (!prop.Value.IsNull)
                {
                    var dto = JsonSerializer.Deserialize<ExerciseSearchDTO>((byte[])prop.Value!, _jsonSerializationOptions);

                    if (dto != null) findedExercises.Add(dto);
                }
            }
        }

        public async Task<Result<ExerciseSearchDTO>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken)
        {
            string exerciseIdCacheKey = $"exercise:{id}";

            if(_memoryCache.TryGetValue(exerciseIdCacheKey, out Result<ExerciseSearchDTO>? exerciseSearch)) 
            {
                return exerciseSearch!;
            }
            
            var cached = await _cache.JSON().GetAsync($"exercise:{id}");

            if (cached.IsNull)
            {
                _memoryCache.Set(exerciseIdCacheKey, Result.Fail($"No exercise by id: {id}"), _cacheOptions);
                return Result.Fail($"No exercise by id: {id}");
            }

            var exercise = JsonSerializer.Deserialize<ExerciseSearchDTO>(cached.ToString(), _jsonSerializationOptions);

            var result = Result.Ok(exercise!);

            _memoryCache.Set(exerciseIdCacheKey, result, _cacheOptions);

            return result;
        }
    }
}
