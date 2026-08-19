using Fitness_Tracker_Application.Features.Users.Authorization;
using FluentValidation;
namespace Fitness_Tracker_Application.Validation.User
{
    public class AuthValidation : AbstractValidator<AuthorizateUserCommand>
    {
        public AuthValidation()
        {
            RuleFor(u => u.Login)
                .NotEmpty().WithMessage("Login cannot be empty")
                .Length(6, 20).WithMessage("Login must be between 6 and 20 characters");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password cannot be empty")
                .Length(8, 20).WithMessage("Password must be between 8 and 20 characters");
        }
    }
}
