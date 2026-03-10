#nullable disable
using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase
{
    internal sealed class IncotermLookupRepository : IIncotermLookup
    {
        private readonly IDbConnection _dbConnection;

        public IncotermLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<IncotermLookupDto>> GetAllIncotermAsync()
        {
            const string sql = @"
                SELECT
                    m.Id,
                    m.Code,
                    m.Description
                FROM Purchase.MiscMaster m
                INNER JOIN Purchase.MiscTypeMaster mt ON m.MiscTypeId = mt.Id
                WHERE mt.Description = @MiscTypeDescription
                  AND m.IsActive = 1
                  AND m.IsDeleted = 0
                ORDER BY m.Description ASC;
            ";

            var result = await _dbConnection.QueryAsync<IncotermLookupDto>(sql, new { MiscTypeDescription = MiscEnumEntity.Incoterms });
            return result.ToList();
        }
    }
}
