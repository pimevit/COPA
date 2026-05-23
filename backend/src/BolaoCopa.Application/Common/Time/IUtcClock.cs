namespace BolaoCopa.Application.Common.Time;

public interface IUtcClock
{
    DateTime UtcNow { get; }
}
