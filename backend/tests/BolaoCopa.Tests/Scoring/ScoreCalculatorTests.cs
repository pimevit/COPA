using BolaoCopa.Domain.Enums;
using BolaoCopa.Domain.Scoring;
using Xunit;

namespace BolaoCopa.Tests.Scoring;

public sealed class ScoreCalculatorTests
{
    private readonly ScoreCalculator calculator = new();

    [Fact]
    public void CalculateScore_WhenExactScoreInGroups_ReturnsFivePoints()
    {
        var score = calculator.CalculateScore(2, 1, 2, 1, Stage.Groups);

        Assert.Equal(5, score);
    }

    [Fact]
    public void CalculateScore_WhenHomeWinnerIsCorrectInGroups_ReturnsTwoPoints()
    {
        var score = calculator.CalculateScore(3, 1, 2, 0, Stage.Groups);

        Assert.Equal(2, score);
    }

    [Fact]
    public void CalculateScore_WhenAwayWinnerIsCorrectInGroups_ReturnsTwoPoints()
    {
        var score = calculator.CalculateScore(0, 2, 1, 3, Stage.Groups);

        Assert.Equal(2, score);
    }

    [Fact]
    public void CalculateScore_WhenDrawOutcomeIsCorrectWithoutExactScoreInGroups_ReturnsTwoPoints()
    {
        var score = calculator.CalculateScore(1, 1, 2, 2, Stage.Groups);

        Assert.Equal(2, score);
    }

    [Fact]
    public void CalculateScore_WhenOnlyHomeGoalsMatchWithoutCorrectOutcomeInGroups_ReturnsOnePoint()
    {
        var score = calculator.CalculateScore(1, 0, 1, 2, Stage.Groups);

        Assert.Equal(1, score);
    }

    [Fact]
    public void CalculateScore_WhenOnlyAwayGoalsMatchWithoutCorrectOutcomeInGroups_ReturnsOnePoint()
    {
        var score = calculator.CalculateScore(0, 1, 2, 1, Stage.Groups);

        Assert.Equal(1, score);
    }

    [Fact]
    public void CalculateScore_WhenNothingMatchesInGroups_ReturnsZeroPoints()
    {
        var score = calculator.CalculateScore(3, 2, 0, 1, Stage.Groups);

        Assert.Equal(0, score);
    }

    [Theory]
    [InlineData(Stage.Groups, 5)]
    [InlineData(Stage.RoundOf16, 10)]
    [InlineData(Stage.QuarterFinals, 15)]
    [InlineData(Stage.SemiFinals, 20)]
    [InlineData(Stage.Final, 25)]
    public void CalculateScore_AppliesStageMultiplier(Stage stage, int expectedScore)
    {
        var score = calculator.CalculateScore(2, 1, 2, 1, stage);

        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public void CalculateScore_WhenExactScoreAlsoHasCorrectWinner_UsesExactScorePrecedence()
    {
        var score = calculator.CalculateScore(2, 1, 2, 1, Stage.Groups);

        Assert.Equal(5, score);
    }

    [Fact]
    public void CalculateScore_WhenCorrectWinnerAlsoMatchesOneTeamGoals_ReturnsThreePoints()
    {
        var score = calculator.CalculateScore(2, 1, 2, 0, Stage.Groups);

        Assert.Equal(3, score);
    }

    [Fact]
    public void CalculateScore_WhenCombinedHitIsInKnockoutStage_AppliesStageMultiplier()
    {
        var score = calculator.CalculateScore(2, 1, 2, 0, Stage.QuarterFinals);

        Assert.Equal(9, score);
    }

    [Fact]
    public void CalculateScore_Example3ConfirmedRule_ReturnsThreePointsForOutcomeAndOneTeamGoals()
    {
        var score = calculator.CalculateScore(2, 1, 2, 0, Stage.Groups);

        Assert.Equal(3, score);
    }

    [Theory]
    [InlineData(-1, 0, 0, 0)]
    [InlineData(0, -1, 0, 0)]
    [InlineData(0, 0, -1, 0)]
    [InlineData(0, 0, 0, -1)]
    public void CalculateScore_WhenGoalsAreNegative_Throws(
        int homeGoalsPrediction,
        int awayGoalsPrediction,
        int homeGoalsResult,
        int awayGoalsResult)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.CalculateScore(
            homeGoalsPrediction,
            awayGoalsPrediction,
            homeGoalsResult,
            awayGoalsResult,
            Stage.Groups));
    }

    [Fact]
    public void CalculateScore_WhenStageIsInvalid_Throws()
    {
        var invalidStage = (Stage)999;

        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.CalculateScore(1, 0, 1, 0, invalidStage));
    }
}
