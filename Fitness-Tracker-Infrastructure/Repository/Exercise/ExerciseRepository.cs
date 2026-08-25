using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fitness_Tracker_Application.Features.Exercise;
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

        public async Task<IList<ExerciseSearchDTO>> GetAllExerciseAsync(CancellationToken cancellationToken)
        {
            var query = _context.Exercises.ProjectTo<ExerciseSearchDTO>(_mapper.ConfigurationProvider)
                            .AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<IList<ExerciseSearchDTO>> GetExerciseAsync(string? Name, IList<int>? MusclesId, CancellationToken cancellationToken)
        {
            var query = _context.Exercises.AsNoTracking();

            if (MusclesId != null && MusclesId.Any())
            {
                query = query.Where(ex => ex.Muscles.Any(exMuscle => MusclesId.Contains(exMuscle.MuscleId)));
            }

            if (!string.IsNullOrEmpty(Name))
            {
                query = query.Where(ex => EF.Functions.TrigramsWordSimilarityDistance(Name, ex.Name) < SIMILARITTY_TRESHOLD)
                             .OrderBy(ex => EF.Functions.TrigramsWordSimilarityDistance(Name, ex.Name));
            }

            return await query
                .ProjectTo<ExerciseSearchDTO>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<ExerciseSearchDTO>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken)
        {
            var result = await _context.Exercises
                        .Where(ex => ex.Id == id)
                        .ProjectTo<ExerciseSearchDTO>(_mapper.ConfigurationProvider)
                        .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                return Result.Fail($"No exercise by id: {id}");
            }

            return Result.Ok(result);
        }
    }
}
