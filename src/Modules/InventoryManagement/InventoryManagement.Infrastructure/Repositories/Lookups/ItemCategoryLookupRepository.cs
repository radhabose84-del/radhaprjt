using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    public class ItemCategoryLookupRepository : IInventoryCategoryLookup
    {
        private readonly IDbConnection _dbConnection;

        public ItemCategoryLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<CategoryMasterDto>> GetCategoryByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var list = ids?.Distinct().ToList() ?? new();
            if (list.Count == 0) return new();

            const string sql = @"
                SELECT
                    IC.Id,
                    IC.ItemCategoryName
                FROM Inventory.ItemCategory IC
                WHERE IC.IsDeleted = 0
                AND IC.Id IN @Ids;";

            var command = new CommandDefinition(sql, new { Ids = list }, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<CategoryMasterDto>(command);
            return rows.ToList();
        }
    }
}
