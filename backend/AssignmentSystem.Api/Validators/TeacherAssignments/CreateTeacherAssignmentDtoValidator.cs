using AssignmentSystem.Api.DTOs.TeacherAssignments;
using FluentValidation;

namespace AssignmentSystem.Api.Validators.TeacherAssignments;

public class CreateTeacherAssignmentDtoValidator : AbstractValidator<CreateTeacherAssignmentDto>
{
    public CreateTeacherAssignmentDtoValidator()
    {
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
        RuleFor(x => x.SubjectId).GreaterThan(0);
    }
}
