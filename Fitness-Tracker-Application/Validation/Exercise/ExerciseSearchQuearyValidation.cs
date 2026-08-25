using Fitness_Tracker_Application.Features.Exercise;
using FluentValidation;

namespace Fitness_Tracker_Application.Validation.Exercise
{
    public class ExerciseSearchQuearyValidation : AbstractValidator<ExerciseSearchCommand>
    {
        public ExerciseSearchQuearyValidation()
        {
            RuleForEach(command => command.MusclesId)
                .GreaterThan(0)
                .WithMessage("Muscles' ids must be greater or equal to 0")
                .When(command => command.MusclesId != null);
        }
    }
}
