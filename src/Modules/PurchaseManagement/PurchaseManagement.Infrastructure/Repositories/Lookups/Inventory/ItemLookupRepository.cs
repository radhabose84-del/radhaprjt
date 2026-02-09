using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Inventory
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
                SELECT Id, ItemCode, ItemName
                FROM Inventory.ItemMaster
                WHERE Id IN @ItemIds AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<ItemLookupDto>(
                new CommandDefinition(sql, new { ItemIds = ids }, cancellationToken: ct));

            return result.ToList();
        }
    }
}
