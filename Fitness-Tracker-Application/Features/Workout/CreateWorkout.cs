using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Repository.Workout;
using FluentResults;
using MediatR;
using System.Reflection.Metadata.Ecma335;

namespace Fitness_Tracker_Application.Features.Workout
{
    public record CreateWorkoutCommand(Guid userId, Guid idempotencyKey, CreateWorkoutDTO workoutDTO) : IRequest<Result<Guid>>;
    public class CreateWorkoutCommandHandler : IRequestHandler<CreateWorkoutCommand, Result<Guid>>
    {
        private readonly IWorkoutIdempotencyKeyRepository _idempotencyKeyRepo;
        private readonly IExerciseRepository _exerciseRepo;
        private readonly IWorkoutRepository _repo;

        public CreateWorkoutCommandHandler(IWorkoutIdempotencyKeyRepository idempotencyKeyRepo, IWorkoutRepository repo, IExerciseRepository exerciseRepo)
        {
            _idempotencyKeyRepo = idempotencyKeyRepo;
            _repo = repo;
            _exerciseRepo = exerciseRepo;
        }

        public async Task<Result<Guid>> Handle(CreateWorkoutCommand request, CancellationToken cancellationToken)
        {
            var lockResult = await _idempotencyKeyRepo.LockWorkout(request.userId, request.idempotencyKey);
            
            if(lockResult.Status == IdempotencyStatus.FINISHED)
            {
                return Result.Ok(lockResult.Result!.Value);
            }

            if(lockResult.Status != IdempotencyStatus.TAKEN) 
            {
                return Result.Fail(lockResult.Status.ToString());
            }

            var exercisesIds = request.workoutDTO.workoutSets.Select(set => set.ExerciseId).ToList();
            bool isAllExists = await _exerciseRepo.IsAllExercisesExist(exercisesIds, cancellationToken);

            if (!isAllExists)
            {
                return Result.Fail("Request contains exercise's ids that does not exist in database");
            }


            var creationResult = await _repo.CreateNewWorkout(request.userId, request.workoutDTO, cancellationToken);

            await _idempotencyKeyRepo.DeleteWorkoutIdempotencyStatus(request.userId, request.idempotencyKey);

            return creationResult;
        }
    }
}
