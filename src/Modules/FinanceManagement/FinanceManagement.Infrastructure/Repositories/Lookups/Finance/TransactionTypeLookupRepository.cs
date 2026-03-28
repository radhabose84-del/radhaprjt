using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class TransactionTypeLookupRepository : ITransactionTypeLookup
    {
        private readonly IDbConnection _dbConnection;

        public TransactionTypeLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<TransactionTypeLookupDto>> GetAllTransactionTypeAsync()
        {
            const string sql = @"
                SELECT Id, TypeName, ShortName, Description
                FROM Finance.TransactionTypeMaster
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY TypeName ASC";

            var result = await _dbConnection.QueryAsync<TransactionTypeLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<TransactionTypeLookupDto>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0)
                return new List<TransactionTypeLookupDto>();

            const string sql = @"
                SELECT Id, TypeName, ShortName, Description
                FROM Finance.TransactionTypeMaster
                WHERE Id IN @Ids AND IsDeleted = 0";

            var result = await _dbConnection.QueryAsync<TransactionTypeLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
