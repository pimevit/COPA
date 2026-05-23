namespace BolaoCopa.Domain.Entities;

public sealed class Bet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MatchId { get; set; }
    public int HomeGoalsPrediction { get; set; }
    public int AwayGoalsPrediction { get; set; }
    public int PointsEarned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Match? Match { get; set; }
}
