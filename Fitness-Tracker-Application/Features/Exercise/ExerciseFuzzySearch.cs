using Fitness_Tracker_Application.Service.Pagination;
using FluentResults;
using FuzzySharp;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public class ExerciseFuzzySearch
    {
        private List<ExerciseSearchDTO> _exercises = new();
        private List<string[]> _splitedExercises = new();

        public void Initialize(IEnumerable<ExerciseSearchDTO> items)
        {
            _exercises = items.OrderBy(ex => ex.Name).ToList();
            _splitedExercises = new List<string[]>(_exercises.Count);

            foreach (var exercise in _exercises)
            {
                _splitedExercises.Add(exercise.Name.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private IEnumerable<ExerciseSearchDTO> Search(string? name, IList<int>? muscleIds, int minScore)
        {
            IEnumerable<ExerciseSearchDTO> selectedExercise = _exercises;

            bool hasName = !string.IsNullOrEmpty(name);

            if (hasName)
            {
                string[] splittedName = name.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                selectedExercise = _exercises
                    .Select((ex, index) => new
                    {
                        Item = ex,
                        Score = GetTokenFuzzyScore(splittedName, _splitedExercises[index])
                    })
                    .Where(ex => ex.Score >= minScore)
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Item.Id)
                    .Select(ex => ex.Item);
            }

            if (muscleIds != null && muscleIds.Count > 0)
            {
                selectedExercise = selectedExercise.Where(ex => muscleIds.All(id => ex.Muscles.Any(muscle => muscle.Id == id)));
            }

            return selectedExercise;
        }

        public PaginationResponse<ExerciseSearchDTO> SearchByPage(string? name, IList<int>? muscleIds, int page, int size, int minScore = 60) 
        {
            var selectedExercise = Search(name, muscleIds, minScore).ToList();
            int totalCount = selectedExercise.Count;

            var exercisePaginated = selectedExercise.Skip((page - 1) * size).Take(size).ToList();

            return new PaginationResponse<ExerciseSearchDTO>(page, size, totalCount, exercisePaginated);                  
        }

        public Result<ExerciseSearchDTO> GetExerciseById(int id) 
        {
            var exercise = _exercises.FirstOrDefault(x => x.Id == id);

            if (exercise == null)
            {
                return Result.Fail($"Exercise with id {id} does not exist");
            }

            return Result.Ok(exercise);
        }

        public static double GetTokenFuzzyScore(string[] nameWords, string[] targetWords, int wordCutoff = 60)
        {
            if (nameWords.Length == 0 || targetWords.Length == 0) return 0;

            double totalScore = 0;

            foreach (var qWord in nameWords)
            {
                int bestWordMatch = 0;

                foreach (var tWord in targetWords)
                {
                    int score = Fuzz.Ratio(qWord, tWord);
                    if (score > bestWordMatch)
                    {
                        bestWordMatch = score;
                    }
                }

                if (bestWordMatch < wordCutoff)
                {
                    bestWordMatch = 0;
                }

                totalScore += bestWordMatch;
            }

            return totalScore / nameWords.Length;
        }
    }
}
