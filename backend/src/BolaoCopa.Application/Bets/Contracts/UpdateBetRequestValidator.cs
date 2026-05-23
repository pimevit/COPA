using FluentValidation;

namespace BolaoCopa.Application.Bets.Contracts;

public sealed class UpdateBetRequestValidator : AbstractValidator<UpdateBetRequest>
{
    public UpdateBetRequestValidator()
    {
        RuleFor(request => request.HomeGoalsPrediction)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.AwayGoalsPrediction)
            .GreaterThanOrEqualTo(0);
    }
}
