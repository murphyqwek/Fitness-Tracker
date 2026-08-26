using Fitness_Tracker_Application.Features.Exercise;
using FluentValidation;

namespace Fitness_Tracker_Application.Validation.Exercise
{
    public class ExerciseSearchQueryValidation : AbstractValidator<ExerciseSearchCommand>
    {
        public ExerciseSearchQueryValidation()
        {
            RuleForEach(command => command.MusclesId)
                .GreaterThan(0)
                .WithMessage("Muscles' ids must be greater or equal to 0")
                .When(command => command.MusclesId != null);

            RuleFor(command => command.Name)
                    .MaximumLength(50)
                    .WithMessage("Exercise name's length must be less than 50 characters")
                    .When(command => command.Name != null);
                
        }
    }
}
