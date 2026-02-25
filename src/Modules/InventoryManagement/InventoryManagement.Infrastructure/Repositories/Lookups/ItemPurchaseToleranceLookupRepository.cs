using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    internal class ItemPurchaseToleranceLookupRepository : IItemPurchaseToleranceLookup
    {
        private readonly IDbConnection _dbConnection;

        public ItemPurchaseToleranceLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<ItemPurchaseToleranceLookupDto>> GetByIdsAsync(IEnumerable<int> itemIds, CancellationToken ct = default)
        {
            var ids = itemIds?.Where(i => i > 0).Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<ItemPurchaseToleranceLookupDto>();

            const string sql = @"
                SELECT
                    b.Id            AS ItemId,
                    b.ItemCode,
                    b.ItemName,
                    a.LowerTolerance,
                    a.UpperTolerance,
                    ip.PurchaseUomId,
                    u.UOMName
                FROM Inventory.ItemMaster AS b
                LEFT JOIN Inventory.ItemInventory AS a
                    ON a.ItemId = b.Id
                OUTER APPLY (
                    SELECT TOP (1) *
                    FROM Inventory.ItemPurchase p
                    WHERE p.ItemId = b.Id
                    ORDER BY p.Id DESC
                ) AS ip
                LEFT JOIN Inventory.UOM AS u
                    ON u.Id = ip.PurchaseUomId
                WHERE b.Id IN @ItemIds
                ORDER BY b.ItemCode;";

            var rows = await _dbConnection.QueryAsync<ItemPurchaseToleranceLookupDto>(
                new CommandDefinition(sql, new { ItemIds = ids }, cancellationToken: ct));

            return rows.ToList();
        }
    }
}
