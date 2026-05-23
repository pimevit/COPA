using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;
using BolaoCopa.Application.Matches;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeedRunner(AppDbContext dbContext)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting database seed.");

        var teams = await readSeedFileAsync<TeamSeedItem>("teams.json", cancellationToken);
        await seedTeamsAsync(teams, cancellationToken);

        var matches = await readSeedFileAsync<MatchSeedItem>("matches.json", cancellationToken);
        await seedMatchesAsync(matches, cancellationToken);

        Console.WriteLine("Database seed finished.");
    }

    private async Task seedTeamsAsync(IReadOnlyCollection<TeamSeedItem> teams, CancellationToken cancellationToken)
    {
        validateTeamSeed(teams);

        var existingTeams = await dbContext.Teams.ToListAsync(cancellationToken);
        validateNoDuplicateExistingTeamCodes(existingTeams);

        var teamsByCode = existingTeams.ToDictionary(team => normalizeCode(team.Code), StringComparer.OrdinalIgnoreCase);
        var inserted = 0;
        var updated = 0;

        foreach (var item in teams)
        {
            var code = normalizeCode(item.Code);

            if (!teamsByCode.TryGetValue(code, out var team))
            {
                team = new Team
                {
                    Name = item.Name.Trim(),
                    Code = code,
                    FlagUrl = item.FlagUrl.Trim()
                };

                dbContext.Teams.Add(team);
                teamsByCode.Add(code, team);
                inserted++;
                continue;
            }

            var name = item.Name.Trim();
            var flagUrl = item.FlagUrl.Trim();

            if (team.Name == name && team.FlagUrl == flagUrl)
            {
                continue;
            }

            team.Name = name;
            team.FlagUrl = flagUrl;
            updated++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Teams seed completed. Inserted: {inserted}; updated: {updated}.");
    }

    private async Task seedMatchesAsync(IReadOnlyCollection<MatchSeedItem> matches, CancellationToken cancellationToken)
    {
        validateMatchSeed(matches);

        var teamsByCode = await dbContext.Teams
            .ToDictionaryAsync(team => normalizeCode(team.Code), StringComparer.OrdinalIgnoreCase, cancellationToken);

        var existingMatches = await dbContext.Matches.ToListAsync(cancellationToken);
        validateNoDuplicateExistingMatches(existingMatches);

        var matchesByKey = existingMatches.ToDictionary(createMatchKey, StringComparer.OrdinalIgnoreCase);
        var inserted = 0;
        var updated = 0;

        foreach (var item in matches)
        {
            var homeTeamCode = normalizeCode(item.HomeTeamCode);
            var awayTeamCode = normalizeCode(item.AwayTeamCode);

            if (!teamsByCode.TryGetValue(homeTeamCode, out var homeTeam))
            {
                throw new InvalidOperationException($"Match seed references unknown HomeTeamCode '{homeTeamCode}'.");
            }

            if (!teamsByCode.TryGetValue(awayTeamCode, out var awayTeam))
            {
                throw new InvalidOperationException($"Match seed references unknown AwayTeamCode '{awayTeamCode}'.");
            }

            var matchDate = ensureUtc(item.MatchDate.UtcDateTime);
            var key = createMatchKey(homeTeam.Id, awayTeam.Id, matchDate, item.Stage);
            var allowBetUntil = BettingWindow.CalculateAllowBetUntil(matchDate, item.Stage);

            if (!matchesByKey.TryGetValue(key, out var match))
            {
                match = new Match
                {
                    HomeTeamId = homeTeam.Id,
                    AwayTeamId = awayTeam.Id,
                    MatchDate = matchDate,
                    Stage = item.Stage,
                    Status = item.Status,
                    AllowBetUntil = allowBetUntil,
                    HomeGoals = null,
                    AwayGoals = null
                };

                dbContext.Matches.Add(match);
                matchesByKey.Add(key, match);
                inserted++;
                continue;
            }

            var changed = false;

            if (match.AllowBetUntil != allowBetUntil)
            {
                match.AllowBetUntil = allowBetUntil;
                changed = true;
            }

            if (match.HomeGoals is null && match.AwayGoals is null && match.Status != item.Status)
            {
                match.Status = item.Status;
                changed = true;
            }

            if (changed)
            {
                updated++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Matches seed completed. Inserted: {inserted}; updated: {updated}.");
    }

    private static void validateTeamSeed(IReadOnlyCollection<TeamSeedItem> teams)
    {
        if (teams.Count == 0)
        {
            throw new InvalidOperationException("Team seed file is empty.");
        }

        var duplicateCodes = teams
            .GroupBy(team => normalizeCode(team.Code), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateCodes.Length > 0)
        {
            throw new InvalidOperationException($"Team seed contains duplicate Code values: {string.Join(", ", duplicateCodes)}.");
        }

        var invalidTeams = teams
            .Where(team => string.IsNullOrWhiteSpace(team.Name)
                || string.IsNullOrWhiteSpace(team.Code)
                || string.IsNullOrWhiteSpace(team.FlagUrl))
            .ToArray();

        if (invalidTeams.Length > 0)
        {
            throw new InvalidOperationException("Team seed contains items with missing Name, Code or FlagUrl.");
        }
    }

    private static void validateMatchSeed(IReadOnlyCollection<MatchSeedItem> matches)
    {
        if (matches.Count == 0)
        {
            throw new InvalidOperationException("Match seed file is empty.");
        }

        var duplicateMatches = matches
            .GroupBy(createMatchSeedKey, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateMatches.Length > 0)
        {
            throw new InvalidOperationException($"Match seed contains duplicate natural keys: {string.Join(", ", duplicateMatches)}.");
        }

        var invalidMatches = matches
            .Where(match => string.IsNullOrWhiteSpace(match.HomeTeamCode)
                || string.IsNullOrWhiteSpace(match.AwayTeamCode)
                || normalizeCode(match.HomeTeamCode) == normalizeCode(match.AwayTeamCode))
            .ToArray();

        if (invalidMatches.Length > 0)
        {
            throw new InvalidOperationException("Match seed contains invalid team codes.");
        }
    }

    private static void validateNoDuplicateExistingTeamCodes(IReadOnlyCollection<Team> teams)
    {
        var duplicateCodes = teams
            .GroupBy(team => normalizeCode(team.Code), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateCodes.Length > 0)
        {
            throw new InvalidOperationException($"Database already contains duplicate Team.Code values: {string.Join(", ", duplicateCodes)}.");
        }
    }

    private static void validateNoDuplicateExistingMatches(IReadOnlyCollection<Match> matches)
    {
        var duplicateMatches = matches
            .GroupBy(createMatchKey, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateMatches.Length > 0)
        {
            throw new InvalidOperationException($"Database already contains duplicate seeded match keys: {string.Join(", ", duplicateMatches)}.");
        }
    }

    private static async Task<IReadOnlyCollection<T>> readSeedFileAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var path = findSeedFilePath(fileName);
        await using var stream = File.OpenRead(path);
        var items = await JsonSerializer.DeserializeAsync<IReadOnlyCollection<T>>(stream, JsonOptions, cancellationToken);

        return items ?? throw new InvalidOperationException($"Seed file '{fileName}' could not be parsed.");
    }

    private static string findSeedFilePath(string fileName)
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var candidate = Path.Combine(currentDirectory.FullName, "Persistence", "SeedData", fileName);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new FileNotFoundException($"Seed file '{fileName}' was not found.");
    }

    private static string normalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static DateTime ensureUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static string createMatchSeedKey(MatchSeedItem match)
    {
        var matchDate = ensureUtc(match.MatchDate.UtcDateTime);

        return string.Join(
            "|",
            normalizeCode(match.HomeTeamCode),
            normalizeCode(match.AwayTeamCode),
            matchDate.ToString("O", CultureInfo.InvariantCulture),
            match.Stage);
    }

    private static string createMatchKey(Match match)
    {
        return createMatchKey(match.HomeTeamId, match.AwayTeamId, ensureUtc(match.MatchDate), match.Stage);
    }

    private static string createMatchKey(int homeTeamId, int awayTeamId, DateTime matchDate, Stage stage)
    {
        return string.Join(
            "|",
            homeTeamId.ToString(CultureInfo.InvariantCulture),
            awayTeamId.ToString(CultureInfo.InvariantCulture),
            ensureUtc(matchDate).ToString("O", CultureInfo.InvariantCulture),
            stage);
    }

    private sealed record TeamSeedItem(string Name, string Code, string FlagUrl);

    private sealed record MatchSeedItem(
        string HomeTeamCode,
        string AwayTeamCode,
        DateTimeOffset MatchDate,
        Stage Stage,
        MatchStatus Status);
}
