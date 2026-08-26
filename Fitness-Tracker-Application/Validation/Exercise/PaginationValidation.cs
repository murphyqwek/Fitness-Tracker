using Fitness_Tracker_Application.Service.Pagination;
using FluentValidation;

namespace Fitness_Tracker_Application.Validation.Exercise
{
    public class PaginationValidation : AbstractValidator<IPaginationCommand>
    {
        private const int MAX_PAGE_COUNT = 1000;
        private const int MAX_SIZE_COUNT = 100;

        public PaginationValidation() 
        {
            RuleFor(pag => pag.Page).GreaterThan(0).LessThan(MAX_PAGE_COUNT);
            RuleFor(pag => pag.Size).GreaterThan(0).LessThan(MAX_SIZE_COUNT);
        }

    }
}
