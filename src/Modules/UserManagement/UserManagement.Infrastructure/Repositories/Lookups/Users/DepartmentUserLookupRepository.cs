using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users;

/// <summary>
/// Mirrors <c>AppData.sp_EvaluateApproval</c> Block 4 (COMPLAINT_QC_REVIEWER_USER):
/// ApprovalStepDetail → ApprovalStepDepartmentMapping → AppSecurity.Users on the
/// direct <c>u.DepartmentId = dm.DepartmentId AND u.IsActive = 1</c> join.
/// UserManagement owns <c>AppSecurity.Users</c>, so the cross-schema lookup lives here
/// (same pattern as Sales' OfficerAgentUserLookupRepository).
/// </summary>
internal sealed class DepartmentUserLookupRepository : IDepartmentUserLookup
{
    private readonly IDbConnection _dbConnection;

    public DepartmentUserLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<int>> GetActiveUserIdsByApprovalStepTargetTypeAsync(
        string targetTypeCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(targetTypeCode))
            return Array.Empty<int>();

        // Exact mirror of sp_EvaluateApproval Block 4 (the QC step is uniquely
        // identified by its TargetType.Code, so no per-workflow / per-complaint
        // filter is needed — the QC department team is the same for every complaint).
        const string sql = @"
            SELECT DISTINCT TRY_CONVERT(int, u.UserId) AS UserId
            FROM   [AppData].[ApprovalStepDetail]                s
            INNER JOIN [AppData].[MiscMaster]                    tt ON tt.Id = s.TargetTypeId
            INNER JOIN [AppData].[ApprovalStepDepartmentMapping] dm ON dm.ApprovalStepDetailId = s.Id
            INNER JOIN [AppSecurity].[Users]                     u  ON u.DepartmentId = dm.DepartmentId
                                                                   AND u.IsActive = 1
            WHERE  UPPER(LTRIM(RTRIM(ISNULL(tt.Code,'')))) = @TargetTypeCode
               AND TRY_CONVERT(int, u.UserId) IS NOT NULL;";

        var rows = await _dbConnection.QueryAsync<int>(
            new CommandDefinition(
                sql,
                new { TargetTypeCode = targetTypeCode.Trim().ToUpperInvariant() },
                cancellationToken: ct));

        return rows.ToList();
    }
}
