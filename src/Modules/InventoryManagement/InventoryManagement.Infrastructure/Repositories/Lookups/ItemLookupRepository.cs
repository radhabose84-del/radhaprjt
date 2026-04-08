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
                SELECT IM.Id,
                       IM.ItemCode,
                       IM.ItemName,
                       IM.ParentItemId,
                       PIM.ItemName AS ParentItemName,
                       IM.TariffNumber,
                       H.HSNCode,
                       ISNULL(H.GSTPercentage, 0) AS GSTPercentage,
                       IM.IsOnSpot,
                       ISNULL(IP.SourceOfItem, 0) AS SourceOfItem
                FROM Inventory.ItemMaster IM
                LEFT JOIN Inventory.ItemMaster PIM ON PIM.Id = IM.ParentItemId AND PIM.IsDeleted = 0
                LEFT JOIN Inventory.ItemPurchase IP ON IP.ItemId = IM.Id
                LEFT JOIN Inventory.HSNMaster H ON H.Id = IM.HSNId AND H.IsDeleted = 0
                WHERE IM.Id IN @ItemIds AND IM.IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<ItemLookupDto>(
                new CommandDefinition(sql, new { ItemIds = ids }, cancellationToken: ct));

            return result.ToList();
        }

        public async Task<IReadOnlyList<ItemLookupDto>> GetVariantsByParentIdAsync(int parentItemId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT IM.Id,
                       IM.ItemCode,
                       IM.ItemName,
                       IM.ParentItemId,
                       PIM.ItemName AS ParentItemName,
                       IM.TariffNumber,
                       H.HSNCode,
                       ISNULL(H.GSTPercentage, 0) AS GSTPercentage,
                       IM.IsOnSpot                       
                FROM Inventory.ItemMaster IM
                LEFT JOIN Inventory.ItemMaster PIM ON PIM.Id = IM.ParentItemId AND PIM.IsDeleted = 0                
                LEFT JOIN Inventory.HSNMaster H ON H.Id = IM.HSNId AND H.IsDeleted = 0
                WHERE IM.ParentItemId = @ParentItemId
                  AND IM.IsDeleted = 0
                  AND IM.IsActive = 1
                ORDER BY IM.ItemName ASC;";

            var result = await _dbConnection.QueryAsync<ItemLookupDto>(
                new CommandDefinition(sql, new { ParentItemId = parentItemId }, cancellationToken: ct));

            return result.ToList();
        }
    }
}
