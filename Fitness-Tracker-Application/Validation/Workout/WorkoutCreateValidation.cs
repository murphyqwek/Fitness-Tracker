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

            RuleFor(set => set.Order)
                .GreaterThanOrEqualTo(0);
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

            RuleFor(x => x.workoutSets)
                .NotNull().WithMessage("Список подходов не может быть пустым")
                .NotEmpty().WithMessage("Тренировка должна содержать как минимум один подход")
                .Must(HaveSequentialOrder)
                .WithMessage(dto => $"Порядок подходов (Order) некорректен. Номера должны начинаться с 0 и идти строго по порядку без пропусков (от 0 до {dto.workoutSets?.Count - 1}).");
        }

        private bool HaveSequentialOrder(IList<CreateWorkoutSetDTO> sets)
        {
            if (sets == null || sets.Count == 0)
            {
                return true;
            }

            var actualOrders = sets.Select(s => s.Order).OrderBy(o => o);

            var expectedOrders = Enumerable.Range(0, sets.Count);

            return actualOrders.SequenceEqual(expectedOrders);
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
