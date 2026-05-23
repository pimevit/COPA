using FluentValidation;

namespace BolaoCopa.Application.Matches.Contracts;

public sealed class CreateMatchRequestValidator : AbstractValidator<CreateMatchRequest>
{
    public CreateMatchRequestValidator()
    {
        RuleFor(request => request.HomeTeamId)
            .NotNull()
            .GreaterThan(0);

        RuleFor(request => request.AwayTeamId)
            .NotNull()
            .GreaterThan(0);

        RuleFor(request => request.MatchDate)
            .NotNull();

        RuleFor(request => request.Stage)
            .NotNull()
            .IsInEnum();
    }
}
