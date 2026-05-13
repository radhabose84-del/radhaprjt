using System.Data;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class OfficerAgentUserLookupRepository : IOfficerAgentUserLookup
{
    private readonly IDbConnection _dbConnection;

    public OfficerAgentUserLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<int?> GetMarketingOfficerUserIdByAgentIdAsync(int agentId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TRY_CONVERT(int, u.UserId)
            FROM Sales.OfficerAgent oa
            INNER JOIN AppSecurity.Users u
                ON u.EmpId = oa.MarketingOfficerId
                AND u.IsActive = 1
            WHERE oa.AgentId = @AgentId
              AND oa.IsActive = 1;
        ";

        return await _dbConnection.QueryFirstOrDefaultAsync<int?>(sql, new { AgentId = agentId });
    }

    public async Task<int?> GetMarketingOfficerReportToUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TRY_CONVERT(int, u.ReportToId) AS ReportToId,
                   u.EmpId                        AS EmpId
            FROM AppSecurity.Users u
            WHERE u.UserId = @UserId
              AND u.IsActive = 1;
        ";

        var row = await _dbConnection.QueryFirstOrDefaultAsync<UserReportRow>(sql, new { UserId = userId });

        // EmpId null/0 → approver is NOT a Marketing Officer → no escalation notification
        if (row is null || row.EmpId is null or 0)
            return null;

        return row.ReportToId;
    }

    public async Task<IReadOnlyList<MoChainRow>> GetMarketingOfficerChainByAgentIdsAsync(
        IEnumerable<int> agentIds, CancellationToken cancellationToken = default)
    {
        var idList = agentIds?.Distinct().ToArray() ?? Array.Empty<int>();
        if (idList.Length == 0)
            return Array.Empty<MoChainRow>();

        const string sql = @"
            SELECT oa.AgentId,
                   TRY_CONVERT(int, u.UserId)     AS MoUserId,
                   TRY_CONVERT(int, u.ReportToId) AS ReportToId
            FROM Sales.OfficerAgent oa
            INNER JOIN AppSecurity.Users u
                ON u.EmpId = oa.MarketingOfficerId
                AND u.IsActive = 1
            WHERE oa.AgentId IN @AgentIds
              AND oa.IsActive = 1;";

        var rows = await _dbConnection.QueryAsync<MoChainRow>(
            new CommandDefinition(sql, new { AgentIds = idList }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    private sealed class UserReportRow
    {
        public int? ReportToId { get; set; }
        public int? EmpId { get; set; }
    }
}
