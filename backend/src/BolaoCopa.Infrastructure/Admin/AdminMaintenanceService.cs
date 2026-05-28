using System.Text.Json;
using BolaoCopa.Domain.Entities;
using BolaoCopa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopa.Infrastructure.Admin;

public sealed class AdminMaintenanceService(AppDbContext dbContext)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<AdminMaintenanceResponse> ImportBrasileiraoSerieA2026TeamsAsync(
        CancellationToken cancellationToken = default)
    {
        return importTeamsAsync(
            "brasileirao-serie-a-2026-teams.json",
            "brasileirao-serie-a-2026",
            cancellationToken);
    }

    public Task<AdminMaintenanceResponse> ImportWorldCup2026TeamsAsync(
        CancellationToken cancellationToken = default)
    {
        return importTeamsAsync(
            "world-cup-2026-teams.json",
            "world-cup-2026",
            cancellationToken);
    }

    public async Task<AdminMaintenanceResponse> ClearApplicationDataAsync(
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var bets = await dbContext.Bets.ToListAsync(cancellationToken);
        dbContext.Bets.RemoveRange(bets);
        await dbContext.SaveChangesAsync(cancellationToken);

        var matches = await dbContext.Matches.ToListAsync(cancellationToken);
        dbContext.Matches.RemoveRange(matches);
        await dbContext.SaveChangesAsync(cancellationToken);

        var teams = await dbContext.Teams.ToListAsync(cancellationToken);
        dbContext.Teams.RemoveRange(teams);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return AdminMaintenanceResponse.ApplicationDataReset(
            "application-data-reset",
            bets.Count,
            matches.Count,
            teams.Count);
    }

    private async Task<AdminMaintenanceResponse> importTeamsAsync(
        string fileName,
        string action,
        CancellationToken cancellationToken)
    {
        var teams = await readAdminDataFileAsync<TeamDataItem>(fileName, cancellationToken);
        validateTeamData(teams, fileName);

        var existingTeams = await dbContext.Teams.ToListAsync(cancellationToken);
        validateNoDuplicateExistingTeamCodes(existingTeams);

        var teamsByCode = existingTeams.ToDictionary(
            team => normalizeCode(team.Code),
            StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        var updated = 0;

        foreach (var item in teams)
        {
            var code = normalizeCode(item.Code);
            var name = item.Name.Trim();
            var flagUrl = item.FlagUrl?.Trim() ?? string.Empty;

            if (!teamsByCode.TryGetValue(code, out var team))
            {
                team = new Team
                {
                    Name = name,
                    Code = code,
                    FlagUrl = flagUrl
                };

                dbContext.Teams.Add(team);
                teamsByCode.Add(code, team);
                inserted++;
                continue;
            }

            if (team.Name == name && team.FlagUrl == flagUrl)
            {
                continue;
            }

            team.Name = name;
            team.FlagUrl = flagUrl;
            updated++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return AdminMaintenanceResponse.TeamsImport(action, inserted, updated);
    }

    private static void validateTeamData(IReadOnlyCollection<TeamDataItem> teams, string fileName)
    {
        if (teams.Count == 0)
        {
            throw new InvalidOperationException($"Admin data file '{fileName}' is empty.");
        }

        var duplicateCodes = teams
            .GroupBy(team => normalizeCode(team.Code), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateCodes.Length > 0)
        {
            throw new InvalidOperationException(
                $"Admin data file '{fileName}' contains duplicate Code values: {string.Join(", ", duplicateCodes)}.");
        }

        var invalidTeams = teams
            .Where(team => string.IsNullOrWhiteSpace(team.Name) || string.IsNullOrWhiteSpace(team.Code))
            .ToArray();

        if (invalidTeams.Length > 0)
        {
            throw new InvalidOperationException($"Admin data file '{fileName}' contains items with missing Name or Code.");
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
            throw new InvalidOperationException(
                $"Database already contains duplicate Team.Code values: {string.Join(", ", duplicateCodes)}.");
        }
    }

    private static async Task<IReadOnlyCollection<T>> readAdminDataFileAsync<T>(
        string fileName,
        CancellationToken cancellationToken)
    {
        var path = findAdminDataFilePath(fileName);
        await using var stream = File.OpenRead(path);
        var items = await JsonSerializer.DeserializeAsync<IReadOnlyCollection<T>>(
            stream,
            JsonOptions,
            cancellationToken);

        return items ?? throw new InvalidOperationException($"Admin data file '{fileName}' could not be parsed.");
    }

    private static string findAdminDataFilePath(string fileName)
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var outputCandidate = Path.Combine(
                currentDirectory.FullName,
                "Persistence",
                "AdminData",
                fileName);

            if (File.Exists(outputCandidate))
            {
                return outputCandidate;
            }

            var sourceCandidate = Path.Combine(
                currentDirectory.FullName,
                "backend",
                "src",
                "BolaoCopa.Infrastructure",
                "Persistence",
                "AdminData",
                fileName);

            if (File.Exists(sourceCandidate))
            {
                return sourceCandidate;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new FileNotFoundException($"Admin data file '{fileName}' was not found.");
    }

    private static string normalizeCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    private sealed record TeamDataItem(string Name, string Code, string? FlagUrl);
}
