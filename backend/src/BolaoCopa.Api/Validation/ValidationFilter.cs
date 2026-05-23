using BolaoCopa.Api.Errors;
using FluentValidation;
using FluentValidation.Results;

namespace BolaoCopa.Api.Validation;

public sealed class ValidationFilter(IServiceProvider serviceProvider) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is null)
            {
                continue;
            }

            var validator = ResolveValidator(argument);
            if (validator is null)
            {
                continue;
            }

            var result = await ValidateAsync(validator, argument, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                return ApiProblemDetailsFactory.CreateValidationProblem(
                    context.HttpContext,
                    ToErrorDictionary(result));
            }
        }

        return await next(context);
    }

    private IValidator? ResolveValidator(object argument)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

        return serviceProvider.GetService(validatorType) as IValidator;
    }

    private static Task<ValidationResult> ValidateAsync(
        IValidator validator,
        object argument,
        CancellationToken cancellationToken)
    {
        var contextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
        var validationContext = (IValidationContext)Activator.CreateInstance(contextType, argument)!;

        return validator.ValidateAsync(validationContext, cancellationToken);
    }

    private static Dictionary<string, string[]> ToErrorDictionary(ValidationResult result)
    {
        return result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
    }
}
