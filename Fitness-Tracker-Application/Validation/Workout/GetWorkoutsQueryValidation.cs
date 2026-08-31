using Fitness_Tracker_Application.Features.Workout;
using FluentValidation;

namespace Fitness_Tracker_Application.Validation.Workout
{
    public class GetWorkoutsQueryValidation : AbstractValidator<GetWorkoutsQuery>
    {
        public GetWorkoutsQueryValidation()
        {
            RuleFor(query => query.Limit)
                .GreaterThan(0)
                .LessThan(100);
        }
    }
}
