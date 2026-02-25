using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.Lookups
{
    internal class ItemGroupLookupRepository : IItemGroupLookup
    {
        private readonly IDbConnection _dbConnection;

        public ItemGroupLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<ItemGroupLookupDto>> GetAllItemGroupsAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, ItemGroupCode, ItemGroupName
                FROM Inventory.ItemGroup
                WHERE IsDeleted = 0 AND IsActive = 1
                ORDER BY ItemGroupName;";

            var result = await _dbConnection.QueryAsync<ItemGroupLookupDto>(
                new CommandDefinition(sql, cancellationToken: ct));

            return result.ToList();
        }

        public async Task<IReadOnlyList<ItemGroupLookupDto>> GetItemGroupsByIdsAsync(IEnumerable<int> itemGroupIds, CancellationToken ct = default)
        {
            var ids = itemGroupIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<ItemGroupLookupDto>();

            const string sql = @"
                SELECT Id, ItemGroupCode, ItemGroupName
                FROM Inventory.ItemGroup
                WHERE Id IN @ItemGroupIds AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<ItemGroupLookupDto>(
                new CommandDefinition(sql, new { ItemGroupIds = ids }, cancellationToken: ct));

            return result.ToList();
        }
    }
}
