using FuzzySharp;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public class ExerciseFuzzySearch
    {
        private List<ExerciseSearchDTO> _exercises = new();
        private List<string[]> _splitedExercises = new();

        public void Initialize(IEnumerable<ExerciseSearchDTO> items)
        {
            _exercises = items.ToList();
            _splitedExercises = new List<string[]>(items.Count());

            foreach (var exercise in items)
            {
                _splitedExercises.Add(exercise.Name.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public IList<ExerciseSearchDTO> Search(string query, int limit = 10, int minScore = 70)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<ExerciseSearchDTO>();

            string[] splittedQuery = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int i = 0;

            var temp = _exercises
                .Select(ex => new
                {
                    Item = ex,
                    Score = GetTokenFuzzyScore(splittedQuery, _splitedExercises[i++])
                })
                .Where(ex => ex.Score >= minScore)
                .OrderByDescending(x => x.Score);

            return temp
                .Select(x => x.Item).ToList();
        }

        public static double GetTokenFuzzyScore(string[] queryWords, string[] targetWords, int wordCutoff = 70)
        {
            if (queryWords.Length == 0 || targetWords.Length == 0) return 0;

            double totalScore = 0;

            foreach (var qWord in queryWords)
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

            return totalScore / queryWords.Length;
        }
    }
}
