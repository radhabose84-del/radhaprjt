using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase
{
    internal sealed class TnCTemplateLookupRepository : ITnCTemplateLookup
    {
        private readonly IDbConnection _dbConnection;

        public TnCTemplateLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<TnCTemplateLookupDto?> GetByTransactionTypeAsync(int transactionTypeId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1 t.Id, t.TemplateCode, t.TemplateName, t.TermsHtml
                FROM Purchase.TnCTemplateMaster t WITH (NOLOCK)
                INNER JOIN Purchase.TnCTemplateApplicability a WITH (NOLOCK)
                       ON a.TnCTemplateMasterId = t.Id AND a.IsDeleted = 0 AND a.IsActive = 1
                WHERE a.TransactionTypeId = @TransactionTypeId
                  AND t.IsDeleted = 0 AND t.IsActive = 1
                ORDER BY t.ModifiedDate DESC, t.CreatedDate DESC, t.Id DESC;";

            return await _dbConnection.QueryFirstOrDefaultAsync<TnCTemplateLookupDto>(
                new CommandDefinition(sql, new { TransactionTypeId = transactionTypeId }, cancellationToken: ct));
        }
    }
}
