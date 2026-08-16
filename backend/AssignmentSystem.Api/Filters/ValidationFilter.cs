using AssignmentSystem.Api.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AssignmentSystem.Api.Filters;

/// <summary>
/// Runs before every controller action. For each action argument (typically the
/// [FromBody] request DTO), looks up whether a FluentValidation IValidator&lt;T&gt;
/// is registered for that exact type and, if so, validates it - throwing a
/// ValidationAppException (-> 400 with field-level Errors) on failure.
///
/// This is necessary plumbing, not incidental: FluentValidation.AspNetCore no
/// longer auto-validates on model binding as of v11 (the maintainers removed
/// that integration), so without this filter every Validators/*.cs class in
/// this project would be dead code - registered in DI, never actually called.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue; // no validator registered for this DTO type - nothing to check
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                throw new ValidationAppException("One or more validation errors occurred.", errors);
            }
        }

        await next();
    }
}
