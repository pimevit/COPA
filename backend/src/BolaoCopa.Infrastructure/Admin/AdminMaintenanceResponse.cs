namespace BolaoCopa.Infrastructure.Admin;

public sealed record AdminMaintenanceResponse(
    string Action,
    int InsertedTeams,
    int UpdatedTeams,
    int DeletedBets,
    int DeletedMatches,
    int DeletedTeams,
    int RecalculatedMatches,
    int RecalculatedBets)
{
    public static AdminMaintenanceResponse TeamsImport(string action, int insertedTeams, int updatedTeams)
    {
        return new AdminMaintenanceResponse(action, insertedTeams, updatedTeams, 0, 0, 0, 0, 0);
    }

    public static AdminMaintenanceResponse ApplicationDataReset(
        string action,
        int deletedBets,
        int deletedMatches,
        int deletedTeams)
    {
        return new AdminMaintenanceResponse(action, 0, 0, deletedBets, deletedMatches, deletedTeams, 0, 0);
    }

    public static AdminMaintenanceResponse PointsRecalculated(
        string action,
        int recalculatedMatches,
        int recalculatedBets)
    {
        return new AdminMaintenanceResponse(action, 0, 0, 0, 0, 0, recalculatedMatches, recalculatedBets);
    }
}
