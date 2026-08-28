using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fitness_Tracker_Application.Features.Exercise;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Service.Pagination;
using Fitness_Tracker_Infrastructure.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using StackExchange.Redis;
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
        private static readonly JsonSerializerOptions _jsonSerializationOptions = new JsonSerializerOptions()
        { 
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString
        };

        private const double SCORE_THRESHOLD = 0.2;
        private const string INDEX_NAME = "idx:exercise";

        private static bool _isIndexInitialized = false;
        private static readonly object _lock = new();

        public ExerciseRepository(ApplicationDbContext context, IConnectionMultiplexer connection, IMapper mapper)
        {
            _context = context;
            _cache = connection.GetDatabase();
            _searchCommands = _cache.FT();
            _mapper = mapper;


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

        public async Task<PaginationResponse<ExerciseSearchDTO>> GetExerciseAsync(string? name, IList<int>? musclesId, int page, int size, CancellationToken cancellationToken)
        {
            var queryFuzzy = new List<string>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                string sanitizedText = Regex.Replace(name, @"[^\p{L}\p{N}\s]", "").Trim();

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

            int offset = (page - 1) * size;

            Query query = new Query(queryString).SetLanguage("russian").SetWithScores();

            var findedExercises = new List<ExerciseSearchDTO>(20);
            int total = 0;

            try
            {
                List<string> passedExercises = await SortExerciseByScore(query, SCORE_THRESHOLD);
                total = passedExercises.Count;
                FillFindedExercises(findedExercises, passedExercises, offset, size);
            }
            catch (Exception)
            {
                findedExercises.Clear();
            }
            

            return new PaginationResponse<ExerciseSearchDTO>(page, size, total, findedExercises);
        }

        private async Task<List<string>> SortExerciseByScore(Query query, double minScore) 
        {
            var searchResult = await _searchCommands.SearchAsync(INDEX_NAME, query);
            List<string> result = new List<string>();

            foreach (var document in searchResult.Documents)
            {
                if (query.QueryString != "*" && document.Score < minScore)
                {
                    break;
                }

                var jsonProperty = document.GetProperties().FirstOrDefault();
                var jsonString = jsonProperty.Value.ToString();

                result.Add(jsonString);
            }

            return result;
        }

        private void FillFindedExercises(List<ExerciseSearchDTO> findedExercises, List<string> passedExercises, int offset, int size) 
        {
            var pagedList = passedExercises.Skip(offset).Take(size);

            foreach (var jsonString in pagedList)
            {
                if (!string.IsNullOrEmpty(jsonString))
                {
                    var dto = JsonSerializer.Deserialize<ExerciseSearchDTO>(jsonString, _jsonSerializationOptions);

                    if (dto != null) findedExercises.Add(dto);
                }
            }
        }

        public async Task<Result<ExerciseSearchDTO>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken)
        {
            var cached = await _cache.JSON().GetAsync($"exercise:{id}");

            if (cached.IsNull)
            {
                return Result.Fail($"No exercise by id: {id}");
            }

            var exercise = JsonSerializer.Deserialize<ExerciseSearchDTO>(cached.ToString(), _jsonSerializationOptions);

            return Result.Ok(exercise!);
        }
    }
}
