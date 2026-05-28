using BolaoCopa.Domain.Enums;

namespace BolaoCopa.Domain.Entities;

public sealed class Match
{
    public int Id { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }
    public DateTime MatchDate { get; set; }
    public Stage Stage { get; set; }
    public MatchStatus Status { get; set; }
    public DateTime AllowBetUntil { get; set; }
    public bool IsBettingLocked { get; set; }

    public Team? HomeTeam { get; set; }
    public Team? AwayTeam { get; set; }
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();
}
