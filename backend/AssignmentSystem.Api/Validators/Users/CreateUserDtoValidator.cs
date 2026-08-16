using AssignmentSystem.Api.DTOs.Users;
using FluentValidation;

namespace AssignmentSystem.Api.Validators.Users;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => new[] { "Admin", "Teacher", "Student" }.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role must be one of: Admin, Teacher, Student.");
    }
}
