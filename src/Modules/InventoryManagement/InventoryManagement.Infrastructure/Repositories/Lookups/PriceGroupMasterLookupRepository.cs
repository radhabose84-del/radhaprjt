using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class PriceGroupMasterLookupRepository : IPriceGroupMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public PriceGroupMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<PriceGroupMasterLookupDto>> GetAllPriceGroupMasterAsync()
        {
            const string sql = @"
                SELECT
                    Id,
                    PriceGroupCode,
                    PriceGroupName
                FROM Inventory.PriceGroupMaster
                WHERE IsActive = 1
                  AND IsDeleted = 0
                ORDER BY PriceGroupCode";

            var result = await _dbConnection.QueryAsync<PriceGroupMasterLookupDto>(sql);
            return result.ToList();
        }
    }
}
