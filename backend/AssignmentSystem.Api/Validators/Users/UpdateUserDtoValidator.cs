using AssignmentSystem.Api.DTOs.Users;
using FluentValidation;

namespace AssignmentSystem.Api.Validators.Users;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
    }
}
