using Fitness_Tracker_Application.Features.Exercise;
using FluentValidation;

namespace Fitness_Tracker_Application.Validation.Exercise
{
    public class ExerciseSearchByIdQuearyValidation : AbstractValidator<ExerciseSearchByIdCommand>
    {
        public ExerciseSearchByIdQuearyValidation()
        {
            RuleFor(command => command.Id)
                .GreaterThan(0);
        }
    }
}
