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

        public IEnumerable<ExerciseSearchDTO> Search(string query, int limit = 10, int minScore = 30)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<ExerciseSearchDTO>();

            return _exercises
                .Select(ex => new
                {
                    Item = ex,
                    // WeightedRatio идеально работает и с подстроками, и с опечатками
                    Score = Fuzz.WeightedRatio(query, ex.Name)
                })
                .Where(x => x.Score >= minScore)           // Отсекаем совпадения ниже порога
                .OrderByDescending(x => x.Score)           // Сначала самые точные совпадения
                .Select(x => x.Item);
        }
    }
}
