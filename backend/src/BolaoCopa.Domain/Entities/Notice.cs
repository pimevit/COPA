namespace BolaoCopa.Domain.Entities;

public sealed class Notice
{
    public string Key { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
