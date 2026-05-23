using FluentValidation;

namespace BolaoCopa.Application.Matches.Contracts;

public sealed class UpdateMatchResultRequestValidator : AbstractValidator<UpdateMatchResultRequest>
{
    public UpdateMatchResultRequestValidator()
    {
        RuleFor(request => request.HomeGoals)
            .NotNull()
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.AwayGoals)
            .NotNull()
            .GreaterThanOrEqualTo(0);
    }
}
