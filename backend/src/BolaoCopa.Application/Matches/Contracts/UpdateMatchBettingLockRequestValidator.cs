using FluentValidation;

namespace BolaoCopa.Application.Matches.Contracts;

public sealed class UpdateMatchBettingLockRequestValidator : AbstractValidator<UpdateMatchBettingLockRequest>
{
    public UpdateMatchBettingLockRequestValidator()
    {
        RuleFor(request => request.IsBettingLocked)
            .NotNull();
    }
}
