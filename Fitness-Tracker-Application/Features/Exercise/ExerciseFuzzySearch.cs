using FluentResults;
using FuzzySharp;
using MediatR;

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

        public IList<ExerciseSearchDTO> Search(string? name, IList<int>? muscleIds, int limit = 10, int minScore = 70)
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
                    .Select(ex => ex.Item);
            }

            if(muscleIds != null && muscleIds.Count > 0) 
            {
                selectedExercise = selectedExercise.Where(ex => muscleIds.All(id => ex.Muscles.Any(muscle => muscle.Id == id)));
            }

            return selectedExercise.ToList();
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

        public static double GetTokenFuzzyScore(string[] nameWords, string[] targetWords, int wordCutoff = 70)
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
