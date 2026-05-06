using System.Data;
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

    private sealed class UserReportRow
    {
        public int? ReportToId { get; set; }
        public int? EmpId { get; set; }
    }
}
