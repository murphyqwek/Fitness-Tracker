using FuzzySharp;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public class ExerciseFuzzySearch
    {
        private List<ExerciseSearchDTO> _exercises = new();
        private List<string> _names = new();

        public void Initialize(IEnumerable<ExerciseSearchDTO> items)
        {
            _exercises = items.ToList();
            _names = _exercises.Select(x => x.Name).ToList();
        }

        public IEnumerable<ExerciseSearchDTO> Search(string query, int limit = 10, int minScore = 60)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<ExerciseSearchDTO>();

            var matches = Process.ExtractTop(
                query: query,
                choices: _names,
                limit: limit,
                cutoff: minScore
            );

            return matches.Select(m => _exercises[m.Index]);
        }
    }
}
