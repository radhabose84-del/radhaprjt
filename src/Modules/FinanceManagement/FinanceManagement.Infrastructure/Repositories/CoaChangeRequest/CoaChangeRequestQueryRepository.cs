using System.Data;
using Dapper;
using FinanceManagement.Application.CoaChangeRequest.Dto;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;

namespace FinanceManagement.Infrastructure.Repositories.CoaChangeRequest
{
    public class CoaChangeRequestQueryRepository : ICoaChangeRequestQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public CoaChangeRequestQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<CoaChangeRequestDto> Items, int TotalCount)> GetChangeRequestsAsync(
            int companyId, string? status, int pageNumber, int pageSize, CancellationToken ct)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.CoaChangeRequest
                WHERE CompanyId = @CompanyId AND IsDeleted = 0
                  AND (@Status IS NULL OR Status = @Status);

                SELECT Id, CompanyId, TargetAccountId, TargetAccountGroupId, AccountCodeSnapshot,
                       ChangeType, Justification, ImpactAssessment, ImpactApprovedByUserId, ImpactApprovedOn,
                       UnfreezeRequestId, Status, IsPostFreeze, CommittedByUserId, CommittedOn,
                       RequestedByUserId, RequestedOn
                FROM Finance.CoaChangeRequest
                WHERE CompanyId = @CompanyId AND IsDeleted = 0
                  AND (@Status IS NULL OR Status = @Status)
                ORDER BY Id DESC
                OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using var multi = await _dbConnection.QueryMultipleAsync(new CommandDefinition(
                sql,
                new { CompanyId = companyId, Status = status, PageNumber = pageNumber, PageSize = pageSize },
                cancellationToken: ct));

            var total = await multi.ReadFirstAsync<int>();
            var items = (await multi.ReadAsync<CoaChangeRequestDto>()).ToList();
            return (items, total);
        }

        public async Task<CoaUnfreezeRequestDto?> GetUnfreezeRequestByIdAsync(int id, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 1 Id, CompanyId, Reason, CfoApproverUserId, CfoApprovedOn,
                       SysAdminApproverUserId, SysAdminApprovedOn, Status, WindowMinutes,
                       WindowOpenedOn, WindowExpiry, RequestedByUserId, RequestedOn
                FROM Finance.CoaUnfreezeRequest
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<CoaUnfreezeRequestDto>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        }

        public async Task<List<PostFreezeChangeLogDto>> GetPostFreezeChangeLogAsync(int companyId, CancellationToken ct)
        {
            // AC3 — committed, post-freeze changes joined to their unfreeze window for BOTH approvers + stamps.
            // Approver NAMES are enriched by the handler via IUserLookup (no cross-module JOIN — Rule #3).
            const string sql = @"
                SELECT  c.Id                       AS ChangeRequestId,
                        c.CompanyId                AS CompanyId,
                        c.TargetAccountId          AS TargetAccountId,
                        c.TargetAccountGroupId     AS TargetAccountGroupId,
                        c.AccountCodeSnapshot      AS AccountCode,
                        c.ChangeType               AS ChangeType,
                        c.Justification            AS Justification,
                        u.CfoApproverUserId        AS CfoApproverUserId,
                        u.CfoApprovedOn            AS CfoApprovedOn,
                        u.SysAdminApproverUserId   AS SysAdminApproverUserId,
                        u.SysAdminApprovedOn       AS SysAdminApprovedOn,
                        c.CommittedByUserId        AS CommittedByUserId,
                        c.CommittedOn              AS CommittedOn,
                        c.IsPostFreeze             AS IsPostFreeze
                FROM Finance.CoaChangeRequest c
                LEFT JOIN Finance.CoaUnfreezeRequest u
                       ON u.Id = c.UnfreezeRequestId AND u.IsDeleted = 0
                WHERE c.CompanyId = @CompanyId
                  AND c.IsDeleted = 0
                  AND c.IsPostFreeze = 1
                  AND c.Status = 'Committed'
                ORDER BY c.CommittedOn DESC, c.Id DESC";

            var rows = await _dbConnection.QueryAsync<PostFreezeChangeLogDto>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
            return rows.ToList();
        }

        public async Task<bool> ChangeRequestExistsAsync(int id, int companyId, CancellationToken ct)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.CoaChangeRequest
                    WHERE Id = @Id AND CompanyId = @CompanyId AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { Id = id, CompanyId = companyId }, cancellationToken: ct));
        }

        public async Task<string?> GetChangeRequestStatusAsync(int id, CancellationToken ct)
        {
            const string sql = "SELECT TOP 1 Status FROM Finance.CoaChangeRequest WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<string?>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        }

        public async Task<bool> UnfreezeRequestExistsAsync(int id, int companyId, CancellationToken ct)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.CoaUnfreezeRequest
                    WHERE Id = @Id AND CompanyId = @CompanyId AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { Id = id, CompanyId = companyId }, cancellationToken: ct));
        }

        public async Task<string?> GetUnfreezeRequestStatusAsync(int id, CancellationToken ct)
        {
            const string sql = "SELECT TOP 1 Status FROM Finance.CoaUnfreezeRequest WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<string?>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        }
    }
}
