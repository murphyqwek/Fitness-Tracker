using Fitness_Tracker_Application.Features.Users.Registration;
using FluentValidation;
namespace Fitness_Tracker_Application.Validation.User
{
    public class RegisterValidation : AbstractValidator<RegisterUserCommand>
    {
        public RegisterValidation() 
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
