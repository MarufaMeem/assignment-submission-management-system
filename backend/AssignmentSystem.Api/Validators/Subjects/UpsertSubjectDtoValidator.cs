using AssignmentSystem.Api.DTOs.Subjects;
using FluentValidation;

namespace AssignmentSystem.Api.Validators.Subjects;

public class UpsertSubjectDtoValidator : AbstractValidator<UpsertSubjectDto>
{
    public UpsertSubjectDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).MaximumLength(30);
    }
}
