using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    internal class ItemLookupRepository : IItemLookup 
    {
        private readonly IDbConnection _dbConnection;

        public ItemLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<ItemLookupDto>> GetByIdsAsync(IEnumerable<int> itemIds, CancellationToken ct = default)
        {
            var ids = itemIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<ItemLookupDto>();

            const string sql = @"
                SELECT Id,
                       ItemCode,
                       ItemName,
                       TariffNumber,
                       HSNCode,
                       GSTPercentage,
                       IsOnSpot,
                       SourceOfItem
                FROM Inventory.ItemMaster
                WHERE Id IN @ItemIds AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<ItemLookupDto>(
                new CommandDefinition(sql, new { ItemIds = ids }, cancellationToken: ct));

            return result.ToList();
        }
    }
}
