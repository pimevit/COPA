using FluentValidation;

namespace BolaoCopa.Application.Bets.Contracts;

public sealed class CreateBetRequestValidator : AbstractValidator<CreateBetRequest>
{
    public CreateBetRequestValidator()
    {
        RuleFor(request => request.MatchId)
            .GreaterThan(0);

        RuleFor(request => request.HomeGoalsPrediction)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.AwayGoalsPrediction)
            .GreaterThanOrEqualTo(0);
    }
}
