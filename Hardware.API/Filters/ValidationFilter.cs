using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using ValidationException = Hardware.Application.Exceptions.ValidationException;

namespace Hardware.API.Filters;

public sealed class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator) continue;

            var validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
            var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, argument)!;

            var result = await validator.ValidateAsync(validationContext);
            if (result.IsValid) continue;

            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ValidationException(errors);
        }

        await next();
    }
}
