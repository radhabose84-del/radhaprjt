#nullable disable
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase
{
    internal sealed class PaymentTermLookupRepository : IPaymentTermLookup
    {
        private readonly IDbConnection _dbConnection;

        public PaymentTermLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<PaymentTermLookupDto>> GetAllPaymentTermAsync()
        {
            const string sql = @"
                SELECT
                    Id,
                    Code,
                    Description
                FROM Purchase.PaymentTermMaster
                WHERE IsActive = 1
                  AND IsDeleted = 0
                ORDER BY Description ASC;
            ";

            var result = await _dbConnection.QueryAsync<PaymentTermLookupDto>(sql);
            return result.ToList();
        }
    }
}
