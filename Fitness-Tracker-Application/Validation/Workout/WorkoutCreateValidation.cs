using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Features.Workout;
using FluentValidation;

namespace Fitness_Tracker_Application.Validation.Workout
{
    public class WorkoutSetCreateDTOValidation : AbstractValidator<CreateWorkoutSetDTO>
    {
        public WorkoutSetCreateDTOValidation()
        {
            RuleFor(set => set.ExerciseId)
                .GreaterThan(0);

            RuleFor(set => set.Weight)
                .GreaterThanOrEqualTo(0)
                .LessThan(5000);

            RuleFor(set => set.Repetitions)
                .GreaterThanOrEqualTo(0)
                .LessThan(5000);
        }
    }
    
    public class WorkoutCreateDTOValidation : AbstractValidator<CreateWorkoutDTO>
    {
        public WorkoutCreateDTOValidation()
        {
            RuleFor(dto => dto.Name)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(dto => dto.Description)
                .MaximumLength(500);

            RuleForEach(dto => dto.workoutSets)
                .SetValidator(new WorkoutSetCreateDTOValidation());
        }
    }

    public class WorkoutCreateValidation : AbstractValidator<CreateWorkoutCommand>
    {
        public WorkoutCreateValidation()
        {
            RuleFor(command => command.idempotencyKey)
                    .NotEmpty();

            RuleFor(command => command.workoutDTO)
                .NotEmpty()
                .SetValidator(new WorkoutCreateDTOValidation());
        }
    }
}
