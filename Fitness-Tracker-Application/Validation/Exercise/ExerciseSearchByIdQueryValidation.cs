using Fitness_Tracker_Application.Features.Exercise;
using FluentValidation;

namespace Fitness_Tracker_Application.Validation.Exercise
{
    public class ExerciseSearchByIdQueryValidation : AbstractValidator<ExerciseSearchByIdCommand>
    {
        public ExerciseSearchByIdQueryValidation()
        {
            RuleFor(command => command.Id)
                .GreaterThan(0);
        }
    }
}
