using System.Data;
using Dapper;
using FinanceManagement.Application.AccountAuditTrail.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountAuditTrail;

namespace FinanceManagement.Infrastructure.Repositories.AccountAuditTrail
{
    public class AccountAuditTrailQueryRepository : IAccountAuditTrailQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public AccountAuditTrailQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<AccountAuditTrailDto>> GetHistoryAsync(
            int companyId, string entityName, int entityId, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, CompanyId, EntityName, EntityId, Action, PropertyName, OldValue, NewValue,
                       CreatedBy, CreatedByName, CreatedByRole, CreatedIP, CreatedDate
                FROM Finance.AccountAuditTrail
                WHERE CompanyId = @CompanyId AND EntityName = @EntityName AND EntityId = @EntityId
                ORDER BY CreatedDate ASC, Id ASC";

            var rows = await _dbConnection.QueryAsync<AccountAuditTrailDto>(
                new CommandDefinition(sql,
                    new { CompanyId = companyId, EntityName = entityName, EntityId = entityId },
                    cancellationToken: ct));
            return rows.ToList();
        }

        public async Task<List<AccountAuditTrailDto>> ExportAsync(
            int companyId, DateTimeOffset from, DateTimeOffset to, string? entityName, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, CompanyId, EntityName, EntityId, Action, PropertyName, OldValue, NewValue,
                       CreatedBy, CreatedByName, CreatedByRole, CreatedIP, CreatedDate
                FROM Finance.AccountAuditTrail
                WHERE CompanyId = @CompanyId
                  AND CreatedDate >= @From AND CreatedDate < @To
                  AND (@EntityName IS NULL OR EntityName = @EntityName)
                ORDER BY CreatedDate ASC, Id ASC";

            var rows = await _dbConnection.QueryAsync<AccountAuditTrailDto>(
                new CommandDefinition(sql,
                    new { CompanyId = companyId, From = from, To = to, EntityName = entityName },
                    cancellationToken: ct));
            return rows.ToList();
        }
    }
}
