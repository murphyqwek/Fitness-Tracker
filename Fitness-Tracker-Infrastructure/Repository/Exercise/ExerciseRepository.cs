using AutoMapper;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Domain.Entity;
using Fitness_Tracker_Infrastructure.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace Fitness_Tracker_Infrastructure.Repository.Exercises
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        private const double SIMILARITTY_TRESHOLD = 0.7;

        public ExerciseRepository(ApplicationDbContext context, IMapper mapper) 
        { 
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<Exercise>> GetAllExerciseAsync(CancellationToken cancellationToken)
        {
            var result = await _context.Exercises.Include(ex => ex.Muscles)
                            .ThenInclude(exMuscle => exMuscle.Muscle)
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);

            return _mapper.Map<IList<Exercise>>(result);
        }

        public async Task<IList<Exercise>> GetExerciseAsync(string? Name, IList<int>? MusclesId, CancellationToken cancellationToken)
        {
            var queary = _context.Exercises.Include(ex => ex.Muscles)
                                            .ThenInclude(exMuscle => exMuscle.Muscle)
                                            .AsNoTracking()
                                            .AsQueryable();

            if(MusclesId != null && MusclesId.Any()) {
                queary = queary.Where(ex => ex.Muscles.Any(exMuscle => MusclesId.Contains(exMuscle.MuscleId)));
            }

            if(!string.IsNullOrEmpty(Name)) {
                queary = queary.Where(ex =>
                        EF.Functions.TrigramsWordSimilarityDistance(Name, ex.Name) < SIMILARITTY_TRESHOLD
                    )
                    .OrderBy(ex => EF.Functions.TrigramsWordSimilarityDistance(Name, ex.Name));
            }

            var result = await queary.ToListAsync(cancellationToken);

            return _mapper.Map<IList<Exercise>>(result);
        }

        public async Task<Result<Exercise>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken)
        {
            var result = await _context.Exercises.Include(ex => ex.Muscles)
                                            .ThenInclude(exMuscle => exMuscle.Muscle)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(ex => ex.Id == id, cancellationToken);

            if (result == null)
            {
                return Result.Fail($"No exercise by id: {id}");
            }

            return _mapper.Map<Exercise>(result);
        }
    }
}
