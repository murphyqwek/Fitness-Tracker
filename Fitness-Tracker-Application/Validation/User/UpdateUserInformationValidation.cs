using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Features.Users.Infomration;
using FluentValidation;

namespace Fitness_Tracker_Application.Validation.User
{
    public class UserInformationUpdateDTOValidation: AbstractValidator<UserUpdateDTO> 
    {
        public UserInformationUpdateDTOValidation()
        {
            RuleFor(dto => dto.name)
                .MinimumLength(1)
                .MaximumLength(20);

            RuleFor(dto => dto.birthDay)
                .LessThan(_ => DateOnly.FromDateTime(DateTime.Now))
                .WithMessage("Дата рождения должна раньше, чеме сегодняшний день");

            RuleFor(dto => dto.height)
                .InclusiveBetween(0, 300);

            RuleFor(dto => dto.weight)
                .InclusiveBetween(0, 1000);
        }
    }

    public class UpdateUserInformationValidation : AbstractValidator<UpdateUserInformationCommand>
    {
        public UpdateUserInformationValidation()
        {
            RuleFor(command => command.id)
                .NotEmpty();

            RuleFor(command => command.userUpdateDto)
                    .SetValidator(new UserInformationUpdateDTOValidation());
        }
    }
}
