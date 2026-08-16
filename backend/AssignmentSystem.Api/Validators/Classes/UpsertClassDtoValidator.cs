using AssignmentSystem.Api.DTOs.Classes;
using FluentValidation;

namespace AssignmentSystem.Api.Validators.Classes;

public class UpsertClassDtoValidator : AbstractValidator<UpsertClassDto>
{
    public UpsertClassDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
