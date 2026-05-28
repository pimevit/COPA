namespace BolaoCopa.Application.Matches;

public static class BettingWindow
{
    public static DateTime CalculateAllowBetUntil(DateTime matchDateUtc, Domain.Enums.Stage stage)
    {
        var utcMatchDate = ensureUtc(matchDateUtc);
        var offset = stage == Domain.Enums.Stage.Groups ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(30);

        return ensureUtc(utcMatchDate.Subtract(offset));
    }

    public static bool IsBettingOpen(DateTime allowBetUntilUtc, DateTime nowUtc, bool isBettingLocked = false)
    {
        return !isBettingLocked && ensureUtc(nowUtc) < ensureUtc(allowBetUntilUtc);
    }

    private static DateTime ensureUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
