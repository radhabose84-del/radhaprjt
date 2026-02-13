using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.Lookups
{
    internal class WarehouseLookupRepository : IWarehouseLookup
    {
        private readonly IDbConnection _dbConnection;

        public WarehouseLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<WarehouseLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, WarehouseCode, WarehouseName, ParentWarehouseId
                FROM Warehouse.WarehouseMaster
                WHERE IsDeleted = 0
                ORDER BY WarehouseCode, WarehouseName;";

            var rows = await _dbConnection.QueryAsync<WarehouseLookupDto>(
                new CommandDefinition(sql, cancellationToken: ct));

            return rows.AsList();
        }

        public async Task<IReadOnlyList<WarehouseLookupDto>> GetByIdsAsync(IEnumerable<int> warehouseIds, CancellationToken ct = default)
        {
            var ids = warehouseIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<WarehouseLookupDto>();

            const string sql = @"
                SELECT Id, WarehouseCode, WarehouseName, ParentWarehouseId
                FROM Warehouse.WarehouseMaster
                WHERE IsDeleted = 0
                  AND Id IN @WarehouseIds
                ORDER BY WarehouseCode, WarehouseName;";

            var rows = await _dbConnection.QueryAsync<WarehouseLookupDto>(
                new CommandDefinition(sql, new { WarehouseIds = ids }, cancellationToken: ct));

            return rows.AsList();
        }
    }
}
